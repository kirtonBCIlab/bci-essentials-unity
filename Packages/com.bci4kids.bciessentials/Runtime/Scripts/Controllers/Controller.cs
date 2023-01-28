using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using BCIEssentials.LSL;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;

//using LSLMarkerStreams;

/*SSVEP Controller
 * This is the SPO Controller base class for an object-oriented design (OOD) approach to SSVEP BCI
 * Author: Brian Irvine
 * 
 * Attributes
 * 
 * 
 * Methods
 * 
 * 
 * 
 */

public class Controller : MonoBehaviour
{
    //Display
    public int refreshRate = 60;
    private float currentRefreshRate;
    private float sumRefreshRate;
    private float avgRefreshRate;
    private int refreshCounter = 0;

    //Matrix Setup
    public bool setupRequired;

    //PopulateObjectList
    public bool listExists;
    //public GameObject[] objectList;
    public List<GameObject> objectList = new();
    [HideInInspector] public List<GameObject> objectsToRemove = new();

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

    // Selection
    protected bool voteOnWindows = false;

    //Deal with the BCI Tag in a scene with mor flexibility.
    [SerializeField]
    private string _myTag = "BCI";
    public string myTag
    {
        get { return _myTag; }
        set { _myTag = value; }
    }

    // Receive markers
    protected bool receivingMarkers = false;

    // Scripts
    [HideInInspector] public MatrixSetup setup;
    [HideInInspector] public LSLMarkerStream marker;
    [HideInInspector] public LSLResponseStream response;



    // Start is called before the first frame update
    void Start()
    {
        // Attach Scripts
        setup = GetComponent<MatrixSetup>();
        marker = GetComponent<LSLMarkerStream>();
        response = GetComponent<LSLResponseStream>();

        // Set the target framerate
        Application.targetFrameRate = refreshRate;

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

        // settingsMenu = GameObject.FindGameObjectWithTag("Settings");
        // settingsMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Check the average framerate every second
        currentRefreshRate = 1 / Time.deltaTime;
        refreshCounter += 1;
        sumRefreshRate += currentRefreshRate;
        if (refreshCounter >= refreshRate)
        {
            avgRefreshRate = sumRefreshRate / (float)refreshCounter;
            if (avgRefreshRate < 0.95 * (float)refreshRate)
            {
                Debug.Log("Refresh rate is below 95% of target, avg refresh rate " + avgRefreshRate.ToString());
            }

            sumRefreshRate = 0;
            refreshCounter = 0;
        }



        // Check key down

        // Press S to start/stop stimulus
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartStopStimulus();
        }

        // Press T to do automated training
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(ReceiveMarkers());
            }

            StartCoroutine(DoTraining());
        }

        // Press I to do Iterative training (MI only)
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(ReceiveMarkers());
            }

            StartCoroutine(DoIterativeTraining());
        }

        // Press U to do User training, stimulus without BCI
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartCoroutine(DoUserTraining());
        }


        // Check for a selection if stim is on
        if (stimOn )
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                StartCoroutine(SelectObject(0));
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(SelectObject(1));
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(SelectObject(2));
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartCoroutine(SelectObject(3));
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                StartCoroutine(SelectObject(4));
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartCoroutine(SelectObject(5));
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                StartCoroutine(SelectObject(6));
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                StartCoroutine(SelectObject(7));
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                StartCoroutine(SelectObject(8));
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                StartCoroutine(SelectObject(9));
            }
        }
    }

    // Populate a list of SPOs
    public virtual void PopulateObjectList(string popMethod)
    {
        //print("populating the list using method " + popMethod);
        //Collect objects with the BCI tag
        if (popMethod == "tag")
        {
            // Remove everything from the existing list
            objectList.Clear();
            
            try
            {
                //Add game objects to list by tag "BCI"
                //GameObject[] objectList = GameObject.FindGameObjectsWithTag("BCI");
                //Editing this to be programmable in the editor under myTag
                GameObject[] objectArray = GameObject.FindGameObjectsWithTag(myTag);
                for (int i = 0; i < objectArray.Length; i++)
                {
                    objectList.Add(objectArray[i]);
                }
                //objectList.Add(GameObject.FindGameObjectsWithTag("BCI"));

                //The object list exists
                listExists = true;
            }
            catch
            {
                //the list does not exist
                print("Unable to create a list based on 'BCI' object tag");
                listExists = false;
            }

        }

        //List is predefined
        else if (popMethod == "predefined")
        {
            if (listExists == true)
            {
                print("The predefined list exists");
            }
            if (listExists == false)
            {
                print("The predefined list doesn't exist, try a different pMethod");
            }
        }

        // Collect by children ??
        else if (popMethod == "children")
        {
            Debug.Log("Populute by children is not yet implemented");
        }

        // Womp womp
        else
        {
            print("No object list exists");
        }

        // Remove from the list any entries that have includeMe set to false
        //print(objectList.Count.ToString());


        foreach (GameObject thisObject in objectList)
        {
            var spo = thisObject.GetComponent<SPO>();
            if (spo == null || spo.Selectable == false)
            {
                objectsToRemove.Add(thisObject);
            }
        }
        foreach (GameObject thisObject in objectsToRemove)
        {
            objectList.Remove(thisObject);
        }
        objectsToRemove.Clear();

        for (int i = 0; i < objectList.Count; i++)
        {
            GameObject thisObject = objectList[i];
            thisObject.GetComponent<SPO>().SelectablePoolIndex = i;
        }
    }

    public virtual void StartStopStimulus()
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
            PopulateObjectList("tag");
            StimulusOn();
        }
    }

    // Turn the stimulus on
    protected virtual void StimulusOn(bool sendConstantMarkers = true)
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
            UnityEngine.Debug.Log("start stimulus coroutine error");
        }
    }

    protected virtual void StimulusOff()
    {
        // End thhe stimulus Coroutine
        stimOn = false;

        // Send the marker to end
        marker.Write("Trial Ends");
    }

    // Select an object from the objectList
    public IEnumerator SelectObject(int objectIndex)
    {
        // When a selection is made, turn the stimulus off
        //stimOn = false;

        Debug.Log("Waiting to select object " + objectIndex.ToString());

        // Wait for stimulus to end
        while(stimOn == true)
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
        PopulateObjectList("tag");

        // Get number of selectable objects by counting the objects in the objectList
        int numOptions = objectList.Count;

        // Create a random non repeating array 
        int[] trainArray = new int[numTrainingSelections];
        trainArray = MakeRNRA(numTrainingSelections, numOptions);
        PrintArray(trainArray);

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
        UnityEngine.Debug.Log("No user training available for this paradigm");

        yield return null;
    }

    public virtual IEnumerator DoIterativeTraining()
    {
        UnityEngine.Debug.Log("No iterative training available for this controller");

        yield return null;
    }

    // Make a random non repeating array of shuffled subarrays
    // 
    public int[] MakeRNRA(int arrayLength, int numOptions)
    {
        return ArrayUtilities.GenerateRNRA(arrayLength, 0, numOptions);
    }

    public void PrintArray(int[] array)
    {
        string[] strings = new string[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            strings[i] = array[i].ToString();
        }
        print(string.Join(" ", strings));
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
        if (!response.Connected)
        {
            response.Connect();
        }

        //Set interval at which to receive markers
        float receiveInterval = 1 / refreshRate;

        //Ping count
        int pingCount = 0;

        // Receive markers continuously
        while (true)
        {
            // Receive markers
            // Initialize the default response string
            string[] defaultResponseStrings = { "" };
            string[] responseStrings = defaultResponseStrings;

            // Pull the python response and add it to the responseStrings array
            responseStrings = response.GetResponses();

            // Check if there is 
            bool newResponse = !responseStrings[0].Equals(defaultResponseStrings[0]);


            if (responseStrings[0] == "ping")
            {
                pingCount++;
                if (pingCount % 100 == 0)
                {
                    Debug.Log("Ping Count: " + pingCount.ToString());
                }
            }

            else if (responseStrings[0] != "")
            {
                for (int i = 0; i < responseStrings.Length; i++)
                {
                    string responseString = responseStrings[i];
                    //print("WE GOT A RESPONSE");
                    print("response : " + responseString);

                    // If there are square brackets then remove them
                    responseString.Replace("[", "").Replace("]","").Replace(".", "");

                    // If it is a single value then select that value
                    int n;
                    bool isNumeric = int.TryParse(responseString, out n);
                    if (isNumeric && n < objectList.Count)
                    {
                        //Run on selection
                        objectList[n].GetComponent<SPO>().Select();
                    }

                    if (voteOnWindows == true)
                    {
                        // Otherwise split 
                        string[] responses = responseString.Split(" ");

                        int[] objectVotes = new int[objectList.Count];

                        foreach(string response in responses)
                        {
                            isNumeric = int.TryParse(response, out n);
                            if (isNumeric == true)
                            {
                                //Run the vote
                                objectVotes[n] = objectVotes[n] + 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (isNumeric == false)
                        {
                            continue;
                        }

                        // make a selection based on the vote
                        int voteSelection = 0;
                        for (int v=1; v<objectList.Count; v++)
                        {
                            if (objectVotes[v] > objectVotes[voteSelection])
                            {
                                voteSelection = v;
                            }
                        }

                        //Run on selection
                        UnityEngine.Debug.Log("Voting selected object " + voteSelection.ToString());
                        objectList[voteSelection].GetComponent<SPO>().Select();
                    }
                }
            }

            // Wait for the next receive interval
            yield return new WaitForSecondsRealtime(receiveInterval);
        }

        Debug.Log("Done receiving markers");
    }



}