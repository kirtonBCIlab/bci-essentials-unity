using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using BCIEssentials.Controllers;
using BCIEssentials.LSL;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;
using Random = System.Random;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// This is the SPO Controller base class for an object-oriented design (OOD) approach to SSVEP BCI
    /// </summary>
    public abstract class BCIControllerBehavior : MonoBehaviour
    {
        public abstract BCIBehaviorType BehaviorType { get; }

        [SerializeField] private int targetFrameRate = 60;

        //Matrix Setup
        public bool setupRequired;

        //PopulateObjectList
        public bool listExists;

        //public GameObject[] objectList;
        [SerializeField] protected List<SPO> objectList = new();
        public List<SPO> ObjectList => objectList;

        //StimulusOn/Off + sending Markers
        public float windowLength = 1.0f;
        public float interWindowInterval = 0f;
        public bool stimOn = false;

        //Training
        public int numTrainingSelections;
        public int numTrainWindows = 3;
        public float pauseBeforeTraining = 2;
        public bool trainTargetPersistent = false;
        public float trainTargetPresentationTime = 3f;
        public float trainBreak = 1f;
        public bool shamFeedback = false;
        public int trainTarget = 99;

        //Deal with the BCI Tag in a scene with mor flexibility.
        [SerializeField] private string _myTag = "BCI";

        public string myTag
        {
            get { return _myTag; }
            set { _myTag = value; }
        }

        // Receive markers
        protected bool receivingMarkers = false;

        // Scripts
        [SerializeField] protected MatrixSetup setup;

        protected LSLMarkerStream marker;
        protected LSLResponseStream response;

        protected virtual void Start()
        {
            if (BCIController.Instance != null)
            {
                BCIController.Instance.RegisterBehavior(this);
            }
        }

        private void OnDestroy()
        {
            if (BCIController.Instance != null)
            {
                BCIController.Instance.UnregisterBehavior(this);
            }
        }

        public void Initialize(LSLMarkerStream lslMarkerStream, LSLResponseStream lslResponseStream)
        {
            Application.targetFrameRate = targetFrameRate;

            marker = lslMarkerStream;
            response = lslResponseStream;

            //Setup if required
            if (setupRequired)
            {
                try
                {
                    setup.SetUpMatrix();
                }
                catch (Exception e)
                {
                    Debug.Log("Setup failed, make sure that the fields in setup matrix are filled");
                    Debug.Log(e.Message);
                }
            }
        }

        public void CleanUp()
        {
            setup.DestroyMatrix();
            StopAllCoroutines();
        }

        // Populate a list of SPOs
        public virtual void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            //Populate the selected list
            switch (populationMethod)
            {
                case SpoPopulationMethod.Predefined:
                    listExists = true;
                    break;
                case SpoPopulationMethod.Children:
                    throw new NotImplementedException("Populating by children is not yet implemented");
                default:
                case SpoPopulationMethod.Tag:
                    objectList.Clear();
                    GameObject[] taggedGOs = GameObject.FindGameObjectsWithTag(myTag);
                    foreach (var taggedGO in taggedGOs)
                    {
                        if (taggedGO.TryGetComponent<SPO>(out var spo) && spo.Selectable)
                        {
                            AddSpo(spo);
                        }
                    }

                    listExists = true;
                    break;
            }

            void AddSpo(SPO spo)
            {
                objectList.Add(spo);
                spo.SelectablePoolIndex = objectList.Count - 1;
            }
        }

        /// <summary>
        /// Obsolete Method. Use enum variation.
        /// </summary>
        /// <param name="populationMethod"></param>
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

        public void StartStopStimulus()
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(ReceiveMarkers());
            }

            // Turn off if on
            if (stimOn)
            {
                StimulusOff();
            }

            // Turn on if off
            else
            {
                PopulateObjectList();
                StimulusOn();
            }
        }

        // Turn the stimulus on
        public virtual void StimulusOn(bool sendConstantMarkers = true)
        {
            stimOn = true;

            // Send the marker to start
            marker.Write("Trial Started");

            // Start the stimulus Coroutine
            try
            {
                StartCoroutine(Stimulus());

                // Not required for P300
                if (sendConstantMarkers)
                {
                    StartCoroutine(SendMarkers(trainTarget));
                }
            }
            catch
            {
                Debug.Log("start stimulus coroutine error");
            }
        }

        public virtual void StimulusOff()
        {
            // End thhe stimulus Coroutine
            stimOn = false;

            // Send the marker to end
            marker.Write("Trial Ends");
        }

        // Select an object from the objectList
        public void SelectObject(int objectIndex)
        {
            if (!stimOn)
            {
                return;
            }

            StartCoroutine(SelectObjectAfterRun(objectIndex));
        }

        public void StartAutomatedTraining()
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(ReceiveMarkers());
            }

            StartCoroutine(DoTraining());
        }

        public void StartIterativeTraining()
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(ReceiveMarkers());
            }

            StartCoroutine(DoIterativeTraining());
        }

        public void StartUserTraining()
        {
            StartCoroutine(DoUserTraining());
        }

        protected IEnumerator SelectObjectAfterRun(int objectIndex)
        {
            // When a selection is made, turn the stimulus off
            //stimOn = false;

            Debug.Log("Waiting to select object " + objectIndex.ToString());

            // Wait for stimulus to end
            while (stimOn == true)
            {
                yield return null;
            }

            try
            {
                // Run the SPO onSelection script
                objectList[objectIndex].GetComponent<SPO>().Select();
            }
            catch
            {
                // Debug
                Debug.Log("Could not select object " + objectIndex.ToString() + " from list");
                Debug.Log("Object list contains " + objectList.Count.ToString() + " objects");
            }
        }

        // Setup a matrix if setup is required
        // Could add this in or leave as a seperate script
        //public void setupMatrix()
        //{


        //}

        // Do training
        public virtual IEnumerator DoTraining()
        {
            // Generate the target list
            PopulateObjectList();

            // Get number of selectable objects by counting the objects in the objectList
            int numOptions = objectList.Count;

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
                objectList[trainTarget].GetComponent<SPO>().OnTrainTarget();


                yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

                if (trainTargetPersistent == false)
                {
                    objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }

                yield return new WaitForSecondsRealtime(0.5f);

                // Go through the training sequence
                //yield return new WaitForSecondsRealtime(3f);

                StimulusOn();
                yield return new WaitForSecondsRealtime((windowLength + interWindowInterval) * (float)numTrainWindows);
                StimulusOff();

                // Turn off train target
                if (trainTargetPersistent == true)
                {
                    objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }


                // If sham feedback is true, then show it
                if (shamFeedback)
                {
                    objectList[trainTarget].GetComponent<SPO>().Select();
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

        public static void LogArrayValues(int[] values)
        {
            print(string.Join(", ", values));
        }

        // Coroutine for the stimulus
        public virtual IEnumerator Stimulus()
        {
            // Present the stimulus until it is turned off
            while (stimOn)
            {
                // What to do each frame
                for (int i = 0; i < objectList.Count; i++)
                {
                    try
                    {
                        objectList[i].GetComponent<SPO>().StartStimulus();
                    }
                    catch
                    {
                        Debug.Log("There is no object " + i.ToString());
                    }
                }

                //Wait until next frame
                yield return 0;
            }

            // Reset the SPOs
            for (int i = 0; i < objectList.Count; i++)
            {
                try
                {
                    objectList[i].GetComponent<SPO>().StopStimulus();
                }
                catch
                {
                    Debug.Log("There is no object " + i.ToString());
                }
            }

            yield return 0;
        }

        // Send markers
        public virtual IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (stimOn)
            {
                string markerString = "marker";

                if (trainingIndex <= objectList.Count)
                {
                    markerString = markerString + "," + trainingIndex.ToString();
                }

                // Send the marker
                marker.Write(markerString);

                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }

        // Coroutine to continuously receive markers
        public IEnumerator ReceiveMarkers()
        {
            if (receivingMarkers == false)
            {
                //Get response stream from Python
                print("Looking for a response stream");
                int diditwork = response.ResolveResponse();
                print(diditwork.ToString());
                receivingMarkers = true;
            }

            //Set interval at which to receive markers
            float receiveInterval = 1 / Application.targetFrameRate;
            float responseTimeout = 0f;

            //Ping count
            int pingCount = 0;

            // Receive markers continuously
            while (receivingMarkers) //TODO: Nothing sets this to false, relies on stopping coroutine instead
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
                        if (int.TryParse(responseString, out var index) && index < objectList.Count)
                        {
                            //Run on selection
                            objectList[index].GetComponent<SPO>().Select();
                        }
                    }
                }

                // Wait for the next receive interval
                yield return new WaitForSecondsRealtime(receiveInterval);
            }

            Debug.Log("Done receiving markers");
        }
    }
}