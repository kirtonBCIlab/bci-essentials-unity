using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BCIEssentials.Controllers;
using BCIEssentials.LSL;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;
using UnityEngine.Serialization;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// This is the SPO Controller base class for an object-oriented design (OOD) approach to SSVEP BCI
    /// </summary>
    public abstract class BCIControllerBehavior : MonoBehaviour
    {
        /// <summary>
        /// The type of BCI behavior implemented.
        /// </summary>
        public abstract BCIBehaviorType BehaviorType { get; }

        [SerializeField]
        [Tooltip("Register and Unregister with the BCI Controller instance using Start and OnDestroy")]
        private bool _selfRegister = true;
        
        [SerializeField]
        [Tooltip("Whether to set as active behavior when self registering.")]
        private bool _selfRegisterAsActive;
        
        [SerializeField]
        [Tooltip("The applications target frame rate. 0 results in no override being applied. -1 or higher than 0 is still applied.")]
        [Min(-1)]
        private int targetFrameRate = 60;

        [SerializeField]
        [Tooltip("Provide an initial set of SPO.")]
        protected List<SPO> _selectableSPOs = new();


        #region Refactorable Properties

        //StimulusOn/Off + sending Markers
        public float windowLength = 1.0f;
        public float interWindowInterval = 0f;

        //Training
        [SerializeField] protected MatrixSetup setup;
        public bool setupRequired;
        public int numTrainingSelections;
        public int numTrainWindows = 3;
        public float pauseBeforeTraining = 2;
        public bool trainTargetPersistent = false;
        public float trainTargetPresentationTime = 3f;
        public float trainBreak = 1f;
        public bool shamFeedback = false;
        public int trainTarget = 99;

        [FormerlySerializedAs("_myTag")]
        public string myTag = "BCI";

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
        public SPO LastSelectedSPO { get; private set; }

        
        protected LSLMarkerStream marker;
        protected LSLResponseStream response;

        protected Coroutine _receiveMarkers;
        protected Coroutine _sendMarkers;
        
        protected Coroutine _runStimulus;
        protected Coroutine _waitToSelect;


        #region Life Cycle Methods

        protected virtual void Start()
        {
            if (_selfRegister)
            {
                RegisterWithControllerInstance(_selfRegisterAsActive);
            }
        }

        private void OnDestroy()
        {
            if (_selfRegister)
            {
                UnregisterFromControllerInstance();
            }
        }

        /// <summary>
        /// Initialize the behavior ready for future stimulus runs.
        /// </summary>
        /// <param name="lslMarkerStream">The lsl stream to write markers to.</param>
        /// <param name="lslResponseStream">The stream to poll for markers.</param>
        public void Initialize(LSLMarkerStream lslMarkerStream, LSLResponseStream lslResponseStream)
        {
            marker = lslMarkerStream;
            response = lslResponseStream;

            //A value of -1 is the default
            //A value of 0 can break stimulus effects
            if (targetFrameRate is -1 or > 0)
            {
                Application.targetFrameRate = targetFrameRate; 
            }
            
            if (setupRequired)
            {
                setup.SetUpMatrix();
            }
        }

        /// <summary>
        /// Stop running coroutines and clean up any resources
        /// controlled by this behavior.
        /// </summary>
        public void CleanUp()
        {
            if (setup != null)
            {
                setup.DestroyMatrix();
            }

            StimulusRunning = false;
            StopCoroutineReference(ref _receiveMarkers);
            StopCoroutineReference(ref _sendMarkers);
            StopCoroutineReference(ref _runStimulus);
            StopCoroutineReference(ref _waitToSelect);
        }

        /// <summary>
        /// Register this behavior with the active <see cref="BCIController.Instance"/>.
        /// <param name="setAsActive">If true will attempt to set itself as active behavior.</param>
        /// </summary>
        public void RegisterWithControllerInstance(bool setAsActive = false)
        {
            if (BCIController.Instance != null)
            {
                BCIController.Instance.RegisterBehavior(this, setAsActive);
            }
        }

        /// <summary>
        /// Unregister this behavior from the active <see cref="BCIController.Instance"/>.
        /// </summary>
        public void UnregisterFromControllerInstance()
        {
            if (BCIController.Instance != null)
            {
                BCIController.Instance.UnregisterBehavior(this);
            }
        }

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
        /// <param name="sendConstantMarkers">
        /// If true will also write to the marker stream until
        /// the stimulus run ends or the number of markers sent equals <see cref="trainTarget"/>.
        /// </param>
        public virtual void StartStimulusRun(bool sendConstantMarkers = true)
        {
            if (StimulusRunning)
            {
                StopStimulusRun();
            }
            
            StimulusRunning = true;
            LastSelectedSPO = null;
            
            // Send the marker to start
            marker.Write("Trial Started");

            ReceiveMarkers();
            PopulateObjectList();
            StopStartCoroutine(ref _runStimulus, RunStimulus());

            // Not required for P300
            if (sendConstantMarkers)
            {
                StopStartCoroutine(ref _sendMarkers, SendMarkers(trainTarget));
            }
        }

        /// <summary>
        /// Stops the current stimulus run.
        /// </summary>
        public virtual void StopStimulusRun()
        {
            StimulusRunning = false;

            // Send the marker to end
            marker.Write("Trial Ends");
        }
        
        protected IEnumerator RunStimulus()
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
        public virtual void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            switch (populationMethod)
            {
                case SpoPopulationMethod.Predefined:
                    //Use the current contents of object list
                    break;
                case SpoPopulationMethod.Children:
                    Debug.LogWarning("Populating by children is not yet implemented");
                    break;
                default:
                case SpoPopulationMethod.Tag:
                    _selectableSPOs.Clear();
                    var taggedGOs = GameObject.FindGameObjectsWithTag(myTag);
                    foreach (var taggedGO in taggedGOs)
                    {
                        if (!taggedGO.TryGetComponent<SPO>(out var spo) || !spo.Selectable)
                        {
                            continue;
                        }

                        _selectableSPOs.Add(spo);
                        spo.SelectablePoolIndex = _selectableSPOs.Count - 1;
                    }
                    break;
            }
        }

        /// <summary>
        /// Obsolete Method.
        /// <para>Use <see cref="PopulateObjectList(BCIEssentials.Controllers.SpoPopulationMethod)"/></para>
        /// </summary>
        /// <param name="populationMethod">method serializable to <see cref="SpoPopulationMethod"/></param>
        [Obsolete]
        public void PopulateObjectList(string populationMethod)
        {
            if (!Enum.TryParse(populationMethod, out SpoPopulationMethod method))
            {
                Debug.LogError($"Unable to convert {populationMethod} to a valid method");
                return;
            }

            PopulateObjectList(method);
        }

        /// <summary>
        /// Select an object from <see cref="SelectableSPOs"/>.
        /// </summary>
        /// <param name="objectIndex">The index value of the object to select.</param>
        /// <param name="stopStimulusRun">If true will end the current stimulus run.</param>
        public void SelectSPO(int objectIndex, bool stopStimulusRun = true)
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
        public void SelectSPOAtEndOfRun(int objectIndex)
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

        // Send markers
        public virtual IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (StimulusRunning)
            {
                string markerString = "marker";

                if (trainingIndex <= _selectableSPOs.Count)
                {
                    markerString = markerString + "," + trainingIndex.ToString();
                }

                // Send the marker
                marker.Write(markerString);

                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }

        protected void ReceiveMarkers()
        {
            StopStartCoroutine(ref _receiveMarkers, PollForMarkers());
        }

        protected IEnumerator PollForMarkers()
        {
            if (response.responseInlet == null || response.responseInlet.IsClosed)
            {
                Debug.Log("Looking for a response stream");
                int resolveCode = response.ResolveResponse();

                if (resolveCode > 0)
                {
                    Debug.Log($"Resolved Stream with code: '{resolveCode}'");
                }
                else
                {
                    Debug.LogError("Failed to open response");
                    _receiveMarkers = null;
                    yield break;
                }
            }

            //Set interval at which to receive markers
            float receiveInterval = 1 / Application.targetFrameRate;
            float responseTimeout = 0f;

            //Ping count
            int pingCount = 0;

            // Receive markers continuously
            while (true) //TODO: Nothing sets this to false, relies on stopping coroutine instead
            {
                // Receive markers
                // Pull the python response and add it to the responseStrings array
                var responseStrings = response.PullResponse(new[] { "" }, responseTimeout);
                var responseString = responseStrings[0];

                if (responseString.Equals("ping"))
                {
                    pingCount++;
                    if (pingCount % 100 == 0)
                    {
                        Debug.Log($"Ping Count: {pingCount}");
                    }
                }
                else if (!responseString.Equals(""))
                {
                    //Question: Why do we only get here if the first value is good, but are then concerned about all other values?
                    //Question: Do we get more than once response string?
                    for (int i = 0; i < responseStrings.Length; i++)
                    {
                        Debug.Log($"response : {responseString}");
                        if (int.TryParse(responseString, out var index))
                        {
                            SelectSPO(index);
                        }
                    }
                }

                // Wait for the next receive interval
                yield return new WaitForSecondsRealtime(receiveInterval);
            }
        }
        #endregion

        #region Training

        public void StartAutomatedTraining()
        {
            ReceiveMarkers();
            StartCoroutine(DoTraining());
        }

        public void StartIterativeTraining()
        {
            ReceiveMarkers();
            StartCoroutine(DoIterativeTraining());
        }

        public void StartUserTraining()
        {
            StartCoroutine(DoUserTraining());
        }

        // Do training
        public virtual IEnumerator DoTraining()
        {
            // Generate the target list
            PopulateObjectList();

            // Get number of selectable objects by counting the objects in the objectList
            int numOptions = _selectableSPOs.Count;

            // Create a random non repeating array 
            int[] trainArray = ArrayUtilities.GenerateRNRA(numTrainingSelections, 0, numOptions);
            LogArrayValues(trainArray);

            yield return new WaitForSecondsRealtime(0.001f);

            // Loop for each training target
            for (int i = 0; i < numTrainingSelections; i++)
            {
                // Get the target from the array
                trainTarget = trainArray[i];

                // 
                Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

                // Turn on train target
                _selectableSPOs[trainTarget].GetComponent<SPO>().OnTrainTarget();


                yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

                if (trainTargetPersistent == false)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }

                yield return new WaitForSecondsRealtime(0.5f);

                // Go through the training sequence
                //yield return new WaitForSecondsRealtime(3f);

                StartStimulusRun();
                yield return new WaitForSecondsRealtime((windowLength + interWindowInterval) * (float)numTrainWindows);
                StopStimulusRun();

                // Turn off train target
                if (trainTargetPersistent == true)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }


                // If sham feedback is true, then show it
                if (shamFeedback)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().Select();
                }

                trainTarget = 99;

                // Take a break
                yield return new WaitForSecondsRealtime(trainBreak);
            }

            marker.Write("Training Complete");
        }

        public virtual IEnumerator DoUserTraining()
        {
            Debug.Log("No user training available for this paradigm");

            yield return null;
        }

        public virtual IEnumerator DoIterativeTraining()
        {
            Debug.Log("No iterative training available for this controller");

            yield return null;
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

        protected void StopCoroutineReference(ref Coroutine reference)
        {
            if (reference != null)
            {
                StopCoroutine(reference);
            }

            reference = null;
        }

        #endregion
    }
}