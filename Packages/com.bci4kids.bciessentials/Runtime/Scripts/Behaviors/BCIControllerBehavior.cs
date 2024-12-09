using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;
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
        [Header("BCI Registration")]
        [SerializeField]
        [Tooltip("Register and Unregister with the BCI Controller instance using Start and OnDestroy")]
        private bool _selfRegister = true;
        
        [SerializeField]
        [Tooltip("Whether to set as active behavior when self registering.")]
        private bool _selfRegisterAsActive;

        [Header("Default SPO Setup")]
        [Tooltip("Component used to set-up default SPO objects")]
        [SerializeField] protected MatrixSetup setup;
        [Tooltip("Indicate whether or not to use the setup component")]
        public bool setupRequired;
        
        [Header("System Properties and Targets")]
        [SerializeField]
        [Tooltip("The applications target frame rate. 0 results in no override being applied. -1 or higher than 0 is still applied.")]
        [Min(-1)]
        protected int targetFrameRate = 60;

        [SerializeField]
        [Tooltip("Enable BCIController Hotkeys")]
        public bool _hotkeysEnabled = true;

        [Header("SPO Objects")]
        [SerializeField]
        [Tooltip("Provide an initial set of SPO.")]
        protected List<SPO> _selectableSPOs = new();

        private int __uniqueID = 1;

        #region Refactorable Properties

        [Header("BCI Signal Properties")]
        //StimulusOn/Off + sending Markers
        [Tooltip("The length of the processing window")]
        //TODO: Rename this more appropriately to our Epoch/scheme
        public float windowLength = 1.0f;
        [Tooltip("The interval between processing windows")]
        public float interWindowInterval = 0f;

        //Training
        [Header("BCI Training Properties")]
        [Tooltip("The number of training iterations")]
        public int numTrainingSelections;
        [Tooltip("The number of windows used in each training iteration")]
        public int numTrainWindows = 3;
        [Tooltip("Before training starts, pause for this amount of time")]
        public float pauseBeforeTraining = 2;
        [Tooltip("The time the target is displayed for, before the sequence begins")]
        public float trainTargetPresentationTime = 3f;
        public float trainBreak = 1f;
        [Tooltip("If true, the train target will pretend to be selected")]
        public bool shamFeedback = false;
        [Tooltip("If true, the train target will remain in the 'target displayed' state")]
        public bool trainTargetPersistent = false;
        [Tooltip("The target object to train on, defaulted to a random high number")]
        public int trainTarget = 99;

        [Header("BCI Training Tag")]
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
        public SPO LastSelectedSPO { get; protected set; }

        /// <summary>
        /// If the behavior is currently running a training session.
        /// </summary>
        public bool TrainingRunning => CurrentTrainingType != BCITrainingType.None;
        
        /// <summary>
        /// The type of training behavior being run.
        /// </summary>
        public BCITrainingType CurrentTrainingType { get; private set; }
        
        
        protected LSLMarkerStream marker;
        protected LSLResponseStream response;

        protected Coroutine _receiveMarkers;
        protected Coroutine _sendMarkers;
        
        protected Coroutine _runStimulus;
        protected Coroutine _waitToSelect;

        protected Coroutine _training;

        private Dictionary<int, SPO> _objectIDtoSPODict = new();


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

            if (response != null)
            {
                response.Disconnect();
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
            BCIController.RegisterBehavior(this, setAsActive);
            BCIController.EnableDisableHotkeys(_hotkeysEnabled);
        }

        /// <summary>
        /// Unregister this behavior from the active <see cref="BCIController.Instance"/>.
        /// </summary>
        public void UnregisterFromControllerInstance()
        {
            BCIController.UnregisterBehavior(this);
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
            if (marker != null)
            {
                marker.Write("Trial Ends");
            }
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
                    _objectIDtoSPODict.Clear(); 
                    var taggedGOs = GameObject.FindGameObjectsWithTag(myTag);
                    foreach (var taggedGO in taggedGOs)
                    {
                        if (!taggedGO.TryGetComponent<SPO>(out var spo) || !spo.Selectable)
                        {
                            continue;
                        }

                        // Check if the object has a unique ObjectID, 
                        // if not assign it a unique ID
                        if (taggedGO.GetComponent<SPO>().ObjectID == 0)
                        {
                            taggedGO.GetComponent<SPO>().ObjectID = __uniqueID;
                            __uniqueID++;
                        }

                        _selectableSPOs.Add(spo);
                        _objectIDtoSPODict.Add(taggedGO.GetComponent<SPO>().ObjectID, spo);
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

        protected virtual IEnumerator SendMarkers(int trainingIndex = 99)
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

        public virtual void ReceiveMarkers()
        {
            if (!response.Connected)
            {
                response.Connect();
            }

            if (response.Polling)
            {
                response.StopPolling();
            }

            //Ping count
            int pingCount = 0;
            response.StartPolling(responses =>
            {
                foreach (var response in responses)
                {
                    if (response.Equals("ping"))
                    {
                        pingCount++;
                        if (pingCount % 100 == 0)
                        {
                            Debug.Log($"Ping Count: {pingCount}");
                        }
                    }

                    // Else if response contains "received" then skip it
                    else if (response.Contains("received"))
                    {
                        continue;
                    }

                    else if (response != "")
                    {
                        string responseString = response;
                        //print("WE GOT A RESPONSE");
                        print("response : " + responseString);

                        // If there are square brackets then remove them
                        responseString = responseString.Replace("[", "").Replace("]","").Replace(".", "");

                        //try to parse the rest of the response as an integer. Handle if it is the formate np.int64().
                        //This grabs the value out of the parenthesis
                        if (responseString.Contains("(") && responseString.Contains(")"))
                        {
                            int startIndex = responseString.IndexOf('(') + 1;
                            int endIndex = responseString.IndexOf(')');
                            if (startIndex < endIndex)
                            {
                                responseString = responseString.Substring(startIndex, endIndex - startIndex);
                            }
                        }

                        // If it is a single value then select that value
                        int n;
                        bool isNumeric = int.TryParse(responseString, out n);
                        if (isNumeric)
                        {
                            //Run on selection based on index
                            if (n < SelectableSPOs.Count)
                            {
                                Debug.Log("Selected object " + n.ToString());
                                //Select the correct unique ObjectID
                                SelectableSPOs[n].Select();
                            }
                        }

                        else
                        {
                            Debug.Log("Response not numeric. Here is the response: " + responseString);
                            continue;
                        }
                    }
                }
            });
        }

        public void StopReceivingMarkers()
        {
            response.StopPolling();
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
                    ReceiveMarkers();
                    trainingBehavior = WhileDoAutomatedTraining();
                    break;
                case BCITrainingType.Iterative:
                    ReceiveMarkers();
                    trainingBehavior = WhileDoIterativeTraining();
                    break;
                case BCITrainingType.User:
                    trainingBehavior = WhileDoUserTraining();
                    break;
                case BCITrainingType.Single:
                    ReceiveMarkers();
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
            // Generate the target list
            PopulateObjectList();

            // Get number of selectable objects by counting the objects in the objectList
            int numOptions = _selectableSPOs.Count;

            // Create a random non repeating array 
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions-1);
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

        protected virtual IEnumerator WhileDoUserTraining()
        {
            Debug.Log("No user training available for this paradigm");

            yield return null;
        }

        //TODO: Figure out why protected IS working, but isn't for other training types
        protected virtual IEnumerator WhileDoIterativeTraining()
        {
            Debug.Log("No iterative training available for this controller");

            yield return null;
        }

        //TODO: Figure out why protected here isn't working, but is for other training types
        public virtual IEnumerator WhileDoSingleTraining()
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
            marker.Write(message);
        }

        #endregion
    }
}