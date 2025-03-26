using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// Base class for any implementation of a BCI paradigm
    /// to be used by a <see cref="BCIController"/>
    /// </summary>
    public abstract class BCIControllerBehavior : MonoBehaviourUsingExtendedAttributes
    {
        /// <summary>
        /// The type of BCI behavior implemented.
        /// </summary>
        public abstract BCIBehaviorType BehaviorType { get; }

        #region Inspector Properties

        [SerializeField, Min(-1)]
        [Tooltip("The applications target frame rate [Hz].\n"
            + "0 results in no override being applied.\n"
            + "-1 or higher than 0 is still applied."
        )]
        protected int targetFrameRate = 60;


        [StartFoldoutGroup("Controller Registration")]
        [SerializeField]
        [Tooltip("Register and Unregister with the BCI Controller instance using Start and OnDestroy")]
        private bool _selfRegister = true;
        
        [SerializeField, ShowIf("_selfRegister")]
        [Tooltip("Whether to set as active behavior when self registering.")]
        private bool _selfRegisterAsActive;

        [SerializeField, ShowIf("_selfRegister")]
        [Tooltip("Controller Instance with which to register.\n"
            + "Defaults to dynamic global controller."
        )]
        [EndFoldoutGroup]
        private BCIControllerInstance _selfRegistrationTarget;


        [StartFoldoutGroup("Stimulus Presenting Objects")]
        [Tooltip("Engine Tag used to programmatically identify Stimulus Presenting Objects")]
        public string SPOTag = "BCI";

        [SerializeField]
        [InspectorName("Selectable Objects")]
        [Tooltip("Provide an initial set of SPO.")]
        protected List<SPO> _selectableSPOs = new();
        protected int SPOCount => _selectableSPOs.Count;

        [Header("Factory Setup")]
        [SerializeField]
        [ContextMenuItem("Set up SPOs", "SetUpSPOs")]
        [ContextMenuItem("Remove Fabricated SPOs", "CleanUpSPOs")]
        [ContextMenuItem("Create New SPO Factory Asset", "CreateAndAssignSPOFactory")]
        [Tooltip("Component used to set-up default SPO objects")]
        private SPOFactory _spoFactory;
        [ShowIf("_spoFactory")]
        [Tooltip("Whether to automatically trigger the setup factory when initialized")]
        public bool FactorySetupRequired;

        private int __uniqueID = 1;


        [StartFoldoutGroup("Training Properties")]
        [Tooltip("The number of training iterations")]
        public int numTrainingSelections;
        [Tooltip("The time the target is displayed for, before the sequence begins [sec]")]
        public float trainTargetPresentationTime = 3f;
        [Tooltip("If true, the train target will remain in the 'target displayed' state")]
        public bool trainTargetPersistent = false;
        [Tooltip("Time between target presentation and stimulus sequence [sec]")]
        public float preStimulusBuffer = 0.5f;
        [Tooltip("Time between the end of a sequence and pretend selection (sham feedback) [sec]")]
        public float postStimulusBuffer = 0f;
        [Tooltip("If true, the train target will pretend to be selected")]
        public bool shamFeedback = false;
        [ShowIf("shamFeedback")]
        [Tooltip("Time allotted for the display of sham feedback [sec]")]
        public float shamFeedbackBuffer = 0.5f;
        [Tooltip("Rest time between training windows [sec]")]
        public float trainBreak = 1f;
        [Tooltip("Index of the object targetted for training,\n"
            + "defaulted to no target (-1)"
        ), EndFoldoutGroup]
        public int trainTarget = -1;

        #endregion
        
        /// <summary>
        /// If a stimulus run is currently taking place.
        /// </summary>
        public bool StimulusRunning { get; protected set; }
        
        /// <summary>
        /// Available SPOs for selection during a Stimulus run
        /// </summary>
        public List<SPO> SelectableSPOs => _selectableSPOs;
        
        /// <summary>
        /// The 
        /// </summary>
        public SPO LastSelectedSPO { get; protected set; }

        /// <summary>
        /// If the behavior is currently running a training session.
        /// </summary>
        public bool TrainingRunning => CurrentTrainingType != BCITrainingType.None;
        
        /// <summary>
        /// The type of training behavior being run.
        /// </summary>
        public BCITrainingType CurrentTrainingType { get; private set; }
        
        
        protected LSLMarkerWriter OutStream;
        protected LSLResponseProvider InStream;

        protected Coroutine _receiveMarkers;
        protected Coroutine _sendMarkers;
        
        protected Coroutine _runStimulus;
        protected Coroutine _waitToSelect;

        protected Coroutine _training;

        protected Dictionary<int, SPO> _objectIDtoSPODict = new();


        #region Life Cycle Methods

        protected virtual void Start()
        {
            if (_selfRegister)
            {
                if (_selfRegistrationTarget != null) { 
                    RegisterWithControllerInstance
                        (_selfRegistrationTarget, _selfRegisterAsActive);
                }
                else RegisterWithController(_selfRegisterAsActive);
            }
        }

        private void OnDestroy()
        {
            if (_selfRegister)
            {
                if (_selfRegistrationTarget != null) {
                    UnregisterFromControllerInstance
                        (_selfRegistrationTarget);
                }
                else UnregisterFromController();
            }
        }

        /// <summary>
        /// Initialize the behavior ready for future stimulus runs.
        /// </summary>
        /// <param name="lslMarkerStream">The lsl stream to write markers to.</param>
        /// <param name="lslResponseStream">The stream to poll for markers.</param>
        public void Initialize(LSLMarkerWriter lslMarkerStream, LSLResponseProvider lslResponseStream)
        {
            OutStream = lslMarkerStream;
            InStream = lslResponseStream;

            //A value of -1 is the default
            //A value of 0 can break stimulus effects
            if (targetFrameRate is -1 or > 0)
            {
                Application.targetFrameRate = targetFrameRate; 
            }
            
            if (FactorySetupRequired) SetUpSPOs();
        }

        /// <summary>
        /// Stop running coroutines and clean up any resources
        /// controlled by this behavior.
        /// </summary>
        public void CleanUp()
        {
            CleanUpSPOs();
            InStream?.CloseStream();

            StimulusRunning = false;
            StopCoroutineReference(ref _receiveMarkers);
            StopCoroutineReference(ref _sendMarkers);
            StopCoroutineReference(ref _runStimulus);
            StopCoroutineReference(ref _waitToSelect);
            StopCoroutineReference(ref _training);
        }


        [ContextMenu("Set Up SPOs")]
        protected void SetUpSPOs()
        {
            _spoFactory?.CreateObjects(transform);
        }

        [ContextMenu("Remove Fabricated SPOs")]
        protected void CleanUpSPOs()
        {
            _spoFactory?.DestroyObjects();
        }

        protected void CreateAndAssignSPOFactory()
        {
            if (_spoFactory)
            {
                Debug.LogWarning("An SPO Factory is Already Set");
                return;
            }
            _spoFactory = SPOGridFactory.CreateInstance(null, 3, 2, Vector2.one * 2);
            string scenePath = EditorSceneManager.GetActiveScene().path;
            string folderPath = Path.GetDirectoryName(scenePath);
            string fileName = $"{name} SPO Factory.asset";
            AssetDatabase.CreateAsset(_spoFactory,  $"{folderPath}\\{fileName}");
        }


        /// <summary>
        /// Register this behavior with the dynamic global <see cref="BCIController"/>.
        /// </summary>
        public void RegisterWithController(bool setAsActive = false)
        => BCIController.RegisterBehavior(this, setAsActive);
        /// <summary>
        /// Register this behavior with a specific <see cref="BCIControllerInstance"/>
        /// </summary>
        public void RegisterWithControllerInstance
        (
            BCIControllerInstance controllerInstance, bool setAsActive = false
        )
        => controllerInstance.RegisterBehavior(this, setAsActive);

        /// <summary>
        /// Unregister this behavior from the dynamic global <see cref="BCIController"/>.
        /// </summary>
        public void UnregisterFromController()
        => BCIController.UnregisterBehavior(this);
        /// <summary>
        /// Unregister this behavior from a specific <see cref="BCIControllerInstance"/>
        /// </summary>
        public void UnregisterFromControllerInstance
        (
            BCIControllerInstance controllerInstance
        )
        => controllerInstance.UnregisterBehavior(this);

        #endregion

        #region Stimulus Methods

        /// <summary>
        /// <para>Invokes <see cref="StartStimulusRun"/> if <see cref="StimulusRunning"/> is false.</para>
        /// <para>Invokes <see cref="StopStimulusRun"/> if <see cref="StimulusRunning"/> is true.</para>
        /// </summary>
        public void StartStopStimulusRun()
        {
            if (StimulusRunning)
            {
                StopStimulusRun();
            }
            else
            {
                StartStimulusRun();
            }
        }

        /// <summary>
        /// Start a new stimulus run. Will end an active stimulus run if present.
        /// </summary>
        /// </param>
        public virtual void StartStimulusRun()
        {
            if (StimulusRunning)
            {
                StopStimulusRun();
            }
            
            StimulusRunning = true;
            LastSelectedSPO = null;
            
            // Send the marker to start
            SendTrialStartedMarker();

            StartReceivingMarkers();
            PopulateObjectList();
            StopStartCoroutine(ref _runStimulus, RunStimulus());
        }

        /// <summary>
        /// Stops the current stimulus run.
        /// </summary>
        public virtual void StopStimulusRun()
        {
            StimulusRunning = false;

            // Send the marker to end
            SendTrialEndsMarker();
        }
        
        protected virtual IEnumerator RunStimulus()
        {
            while (StimulusRunning)
            {
                yield return OnStimulusRunBehavior();
            }

            yield return OnStimulusRunComplete();
            
            StopCoroutineReference(ref _runStimulus);
            StopCoroutineReference(ref _sendMarkers);
        }

        /// <summary>
        /// Behavior to invoke during a stimulus run.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator OnStimulusRunBehavior()
        {
            yield break;
        }

        /// <summary>
        /// Behavior that is run after a stimulus run has ended.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator OnStimulusRunComplete()
        {
            yield break;
        }

        #endregion

        #region SPO Management

        /// <summary>
        /// Populate the <see cref="SelectableSPOs"/> using a particular method.
        /// </summary>
        /// <param name="populationMethod">Method of population to use</param>
        public virtual void PopulateObjectList
        (SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            switch (populationMethod)
            {
                case SpoPopulationMethod.Predefined:
                    break;
                case SpoPopulationMethod.Tag:
                    _selectableSPOs = GetSelectableSPOsByTag();
                    AssignIDsToSelectableSPOs(ref __uniqueID);

                    _objectIDtoSPODict.Clear();
                    AppendSelectableSPOsToObjectIDDictionary();
                    break;
                default:
                    Debug.LogWarning($"Populating using {populationMethod} is not implemented");
                    break;
            }
        }

        protected List<SPO> GetSelectableSPOsByTag()
        {
            var taggedGOs = GameObject.FindGameObjectsWithTag(SPOTag);
            List<SPO> result = new();

            foreach (var taggedGO in taggedGOs)
            {
                if (
                    taggedGO.TryGetComponent(out SPO spo)
                    && spo.Selectable
                )
                result.Add(spo);
            }
            return result;
        }

        protected void AssignIDsToSelectableSPOs(ref int idTracker)
        {
            int poolIndex = 0;
            foreach (SPO spo in _selectableSPOs)
            {
                if (spo.ObjectID == -100) spo.ObjectID = idTracker++;
                spo.SelectablePoolIndex = poolIndex++;
            }
        }

        protected void AppendSelectableSPOsToObjectIDDictionary()
        => _selectableSPOs.ForEach(spo => {
            if (!_objectIDtoSPODict.ContainsKey(spo.ObjectID))
            {
                _objectIDtoSPODict.Add(spo.ObjectID, spo);
            }
        });


        /// <summary>
        /// Select an object from <see cref="SelectableSPOs"/>.
        /// </summary>
        /// <param name="objectIndex">The index value of the object to select.</param>
        /// <param name="stopStimulusRun">If true will end the current stimulus run.</param>
        public virtual void SelectSPO(int objectIndex, bool stopStimulusRun = false)
        {
            var objectCount = _selectableSPOs.Count;
            if (objectCount == 0)
            {
                Debug.Log("No Objects to select");
                return;
            }

            if (objectIndex < 0 || objectIndex >= objectCount)
            {
                Debug.LogWarning($"Invalid Selection. Must be or be between 0 and {_selectableSPOs.Count}");
                return;
            }

            var spo = _selectableSPOs[objectIndex];
            // var spo = _objectIDtoSPODict[objectIndex]; //TODO: Implement this for ObjectID selection
            if (spo == null)
            {
                Debug.LogWarning("SPO is now null and can't be selected");
                return;
            }
            
            spo.Select();
            LastSelectedSPO = spo;
            Debug.Log($"SPO '{spo.gameObject.name}' selected.");

            if (stopStimulusRun)
            {
                StopStimulusRun();
            }
        }

        /// <summary>
        /// Select an object from <see cref="SelectableSPOs"/> if no objects were
        /// selected during a stimulus run.
        /// </summary>
        /// <param name="objectIndex"></param>
        public virtual void SelectSPOAtEndOfRun(int objectIndex)
        {
            StopStartCoroutine(ref _waitToSelect, InvokeAfterStimulusRun(() =>
            {
                if (LastSelectedSPO != null)
                {
                    return;
                }
                
                SelectSPO(objectIndex);
                _waitToSelect = null;
            }));
        }

        protected IEnumerator InvokeAfterStimulusRun(Action action)
        {
            while (StimulusRunning)
            {
                yield return null;
            }

            action?.Invoke();
        }

        #endregion
        
        #region Markers

        protected virtual void SendTrialStartedMarker()
        {
            if (OutStream != null) OutStream.PushTrialStartedMarker();
        }

        protected virtual void SendTrialEndsMarker()
        {
            if (OutStream != null) OutStream.PushTrialEndsMarker();
        }

        public virtual void StartReceivingMarkers()
        {
            InStream.UnsubscribePredictions(OnPredictionReceived);
            InStream.SubscribePredictions(OnPredictionReceived);
        }

        protected virtual void OnPredictionReceived(LSLPredictionResponse prediction)
        {
            SelectSPO(prediction.Value);
        }

        public void StopReceivingMarkers()
        {
            InStream.UnsubscribePredictions(OnPredictionReceived);
        }
        #endregion

        #region Training

        /// <summary>
        /// Start the training behavior for the requested type.
        /// </summary>
        /// <param name="trainingType">
        /// The training behavior type.
        /// Not all behaviors may be implemented by a controller behavior type
        /// </param>
        public void StartTraining(BCITrainingType trainingType)
        {
            if (StimulusRunning)
            {
                StopStimulusRun();
            }

            IEnumerator trainingBehavior = null;
            switch (trainingType)
            {
                case BCITrainingType.Automated:
                    StartReceivingMarkers();
                    trainingBehavior = WhileDoAutomatedTraining();
                    break;
                case BCITrainingType.Iterative:
                    StartReceivingMarkers();
                    trainingBehavior = WhileDoIterativeTraining();
                    break;
                case BCITrainingType.User:
                    trainingBehavior = WhileDoUserTraining();
                    break;
                case BCITrainingType.Single:
                    StartReceivingMarkers();
                    trainingBehavior = WhileDoSingleTraining();
                    break;
                default:
                case BCITrainingType.None:
                    StopTraining();
                    break;
            }

            if (trainingBehavior != null)
            {
                StopStartCoroutine(ref _training, RunControllerTraining(trainingType, trainingBehavior));
            }
        }

        /// <summary>
        /// Stops the current training run.
        /// </summary>
        public void StopTraining()
        {
            CurrentTrainingType = BCITrainingType.None;
            StopCoroutineReference(ref _training);
        }

        private IEnumerator RunControllerTraining(BCITrainingType trainingType, IEnumerator trainingBehavior)
        {
            CurrentTrainingType = trainingType;

            while (TrainingRunning)
            {
                yield return trainingBehavior;
            }

            StopTraining();
        }

        // Do training
        protected virtual IEnumerator WhileDoAutomatedTraining()
        {
            PopulateObjectList();
            
            int numOptions = _selectableSPOs.Count;
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions - 1);
            LogArrayValues(trainArray);

            yield return null;

            // Loop for each training target
            for (int i = 0; i < numTrainingSelections; i++)
            {
                trainTarget = trainArray[i];
                Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

                SPO targetObject = _selectableSPOs[trainTarget];
                yield return RunTrainingRound(targetObject);
            }

            OutStream.PushTrainingCompleteMarker();
        }


        protected IEnumerator RunTrainingRound(SPO targetObject)
        => RunTrainingRound(WaitForStimulusToComplete(), targetObject);
        protected IEnumerator RunTrainingRound
        (
            IEnumerator stimulusDelayRoutine,
            SPO targetObject,
            bool enableShamFeedback = true,
            bool forceTrainTargetPersistence = false
        )
        {
            targetObject.OnTrainTarget();
            yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

            bool shouldPersistTrainTarget
            = trainTargetPersistent || forceTrainTargetPersistence;
            if (!shouldPersistTrainTarget)
            {
                targetObject.OffTrainTarget();
            }

            yield return new WaitForSecondsRealtime(preStimulusBuffer);
            StartStimulusRun();
            yield return stimulusDelayRoutine;
            StopStimulusRun();
            yield return new WaitForSecondsRealtime(postStimulusBuffer);

            if (shamFeedback && enableShamFeedback)
            {
                targetObject.Select();
                yield return new WaitForSecondsRealtime(shamFeedbackBuffer);
            }

            if (shouldPersistTrainTarget)
            {
                targetObject.OffTrainTarget();
            }

            yield return new WaitForSecondsRealtime(trainBreak);
            trainTarget = -1;
        }


        protected virtual IEnumerator WhileDoUserTraining()
        {
            Debug.Log("No user training available for this paradigm");

            yield return null;
        }


        protected virtual IEnumerator WhileDoIterativeTraining()
        {
            Debug.Log("No iterative training available for this controller");

            yield return null;
        }

        protected virtual IEnumerator WhileDoSingleTraining()
        {
            //TODO: Implement a way to handle default null targetObject
            Debug.Log("No single training available for this controller");

            yield return null;
        }

        public virtual void UpdateClassifier()
        {
            Debug.Log("No classifier update available for this controller");
        }

        #endregion

        #region Helper Methods

        protected static void LogArrayValues(int[] values)
        {
            print(string.Join(", ", values));
        }

        protected void StopStartCoroutine(ref Coroutine reference, IEnumerator routine)
        {
            if (reference != null)
            {
                StopCoroutine(reference);
            }

            reference = StartCoroutine(routine);
        }

        protected virtual IEnumerator WaitForStimulusToComplete()
        {
            while (StimulusRunning)
            {
                yield return null;
            }
        }

        //This is a different Stop start Coroutine Method, that is used to return a coroutine reference. Might be better to use this one, but needs testing.
        protected Coroutine Stop_Coroutines_Then_Start_New_Coroutine(ref Coroutine reference_coroutine, IEnumerator routine)
        {
            if (reference_coroutine != null)
            {
                StopCoroutine(reference_coroutine);
            }

            reference_coroutine = StartCoroutine(routine);
            return reference_coroutine;
        }

        protected void StopCoroutineReference(ref Coroutine reference)
        {
            if (reference != null)
            {
                StopCoroutine(reference);
            }

            reference = null;
        }

        public void PassBessyPythonMessage(string message)
        {
            OutStream.PushString(message);
        }

        #endregion
    }
}