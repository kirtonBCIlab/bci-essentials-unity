using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LSL4Unity;

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

    //populateObjectList
    public bool listExists;
    //public GameObject[] objectList;
    public List<GameObject> objectList;
    [HideInInspector] public List<GameObject> objectsToRemove;

    //stimulusOn/Off + sending Markers
    public float windowLength = 1.0f;
    public float interWindowInterval = 0f;
    public bool stimOn = false;

    //Training
    public int numTrainingSelections;
    public int numTrainWindows = 3;
    public float trainBreak = 1f;
    public bool shamFeedback = false;
    public int trainTarget = 99;
    
    //Deal with the BCI Tag in a scene with mor flexibility.
    [SerializeField]
    private string _myTag = "BCI";
    public string myTag
    {
        get {return _myTag; }
        set {_myTag = value; }
    }

    // Receive markers
    private bool receivingMarkers = false;

    // Scripts
    [HideInInspector] public Matrix_Setup setup;
    [HideInInspector] public LSLMarkerStream marker;
    [HideInInspector] public LSLResponseStream response;


    // Start is called before the first frame update
    void Start()
    {
        // Attach Scripts
        setup = GetComponent<Matrix_Setup>();
        marker = GetComponent<LSLMarkerStream>();
        response = GetComponent<LSLResponseStream>();

        // Set the target framerate
        Application.targetFrameRate = refreshRate;

        //Setup if required
        if(setupRequired)
        {
            try
            {
                setup.SetUpMatrix();
            }
            catch
            {
                Debug.Log("Setup failed, make sure that the fields in setup matrix are filled");
            }
        }
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
            startStopStimulus();
        }

        // Press T to do automated training
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Receive incoming markers
            if (receivingMarkers == false)
            {
                StartCoroutine(receiveMarkers());
            }

            StartCoroutine(doTraining());
        }


        // Check for a selection if stim is on
        if (stimOn)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                selectObject(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                selectObject(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                selectObject(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                selectObject(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                selectObject(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                selectObject(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                selectObject(6);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                selectObject(7);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                selectObject(8);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                selectObject(9);
            }
        }


    }

    // Populate a list of SPOs
    public virtual void populateObjectList(string popMethod)
    {
        // Remove everything from the existing list
        objectList.Clear();

        print("populating the list using method " + popMethod);
        //Collect objects with the BCI tag
        if (popMethod == "tag")
        {
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
        print(objectList.Count.ToString());

        
        foreach (GameObject thisObject in objectList)
        {
            if (thisObject.GetComponent<SPO>().includeMe == false)
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
            thisObject.GetComponent<SPO>().myIndex = i;
        }

        print(objectList.Count.ToString());


    }

    public void startStopStimulus()
    {
        // Receive incoming markers
        if (receivingMarkers == false)
        {
            StartCoroutine(receiveMarkers());
        }

        // Turn off if on
        if (stimOn)
        {
            stimulusOff();
        }

        // Turn on if off
        else
        {
            populateObjectList("tag");
            stimulusOn();
        }
    }

    // Turn the stimulus on
    public void stimulusOn()
    {
        stimOn = true;

        // Start the stimulus Coroutine
        try
        {
            StartCoroutine(stimulus());

            // Send the marker to start
            marker.Write("Trial Started");
            StartCoroutine(sendMarkers(trainTarget));
        }
        catch
        {
            
        }
    }

    public void stimulusOff()
    {
        // End thhe stimulus Coroutine
        stimOn = false;

        // Send the marker to end
        marker.Write("Trial Ends");
    }

    //// Send a marker
    //public void sendMarker()
    //{

    //}

    // Receive a marker 
    //public  void receiveMarkers()
    //{

    //    // If stream doesn't exist then open it
    //    //if (responseExists == false)
    //    //{
    //    //    //Get response stream from Python
    //    //    print("Looking for a response stream");
    //    //    response = GetComponent<LSLResponseStream>();
    //    //    int diditwork = response.ResolveResponse();
    //    //    print(diditwork.ToString());
    //    //    responseExists = true;
    //    //}

    //    // Receive Marker

    //}

    // Select an object from the objectList
    public void selectObject(int objectIndex)
    {
        // When a selection is made, turn the stimulus off
        stimOn = false;

        try
        {
            // Run the SPO onSelection script
            objectList[objectIndex].GetComponent<SPO>().onSelection();
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
    public virtual IEnumerator doTraining()
    {   
        // Generate the target list
        populateObjectList("tag");

        // Get number of selectable objects by counting the objects in the objectList
        int numOptions = objectList.Count;

        // Create a random non repeating array 
        int[] trainArray = new int[numTrainingSelections];
        trainArray = makeRNRA(numTrainingSelections, numOptions);
        printArray(trainArray);

        yield return new WaitForSecondsRealtime(0.001f);

        // Loop for each training target
        for (int i = 0; i < numTrainingSelections; i++)
        {
            // Get the target from the array
            trainTarget = trainArray[i];

            // 
            Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

            // Turn on train target
            objectList[trainTarget].GetComponent<SPO>().onTrainTarget();

            // Go through the training sequence
            yield return new WaitForSecondsRealtime(3f);

            stimulusOn();
            yield return new WaitForSecondsRealtime((windowLength + interWindowInterval) * (float)numTrainWindows);
            stimulusOff();

            // Turn off train target
            objectList[trainTarget].GetComponent<SPO>().offTrainTarget();

            // If sham feedback is true, then show it
            if(shamFeedback)
            {
                objectList[trainTarget].GetComponent<SPO>().onSelection();
            }

            trainTarget = 99;

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);
        }

        marker.Write("Training Complete");

    }

    // Make a random non repeating array of shuffled subarrays
    // 
    public int[] makeRNRA(int arrayLength, int numOptions)
    {
        // Make random object
        Debug.Log("Random seed is 42");
        System.Random trainRandom = new System.Random(42);

        // Initialize array
        int[] array = new int[arrayLength];

        // Create an unshuffled array of the possible options
        int[] unshuffledArray = new int[numOptions];
        for (int i = 0; i < numOptions; i++)
        {
            unshuffledArray[i] = i;
        }
        //printArray(unshuffledArray);

        // Get the number of loops required to generate a list of desired length
        int numLoops = (arrayLength / numOptions);
        int remainder = arrayLength % numOptions;

        // Set last value to something well outside the realm of possible options
        int lastValue = 999;

        // Create new shuffled list containing all selections 
        for (int i = 0; i <= numLoops; i++)
        {
            // Shuffle the array 
            int[] shuffledArray = unshuffledArray.OrderBy(x => trainRandom.Next()).ToArray();
            // Reshuffle until first val of shuffled array doesn't match last
            while (shuffledArray[0] == lastValue)
            {
                shuffledArray = unshuffledArray.OrderBy(x => trainRandom.Next()).ToArray();
            }
            //printArray(shuffledArray);

            // If this is not the last loop
            if (i < numLoops)
            {
                // Add the full shuffled array to the big array
                for (int j = 0; j < numOptions; j++)
                {
                    int ind = (i * (numOptions)) + j;
                    //print(ind.ToString());
                    array[ind] = shuffledArray[j];
                }
                lastValue = shuffledArray[numOptions - 1];
            }

            // If this is the last loop
            if (i == numLoops)
            {
                // Add the partial array to the big array
                for (int k = 0; k < remainder; k++)
                {
                    int ind = (i * (numOptions)) + k;
                    //print(ind.ToString());
                    array[ind] = shuffledArray[k];

                }
            }
        }

        //printArray(array);
        return array;
    }

    public void printArray(int[] array)
    {
        string[] strings = new string[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            strings[i] = array[i].ToString();
        }
        print(string.Join(" ",strings));
    }

    // Coroutine for the stimulus
    public virtual IEnumerator stimulus()
    {
        // Present the stimulus until it is turned off
        while(stimOn)
        {
            // What to do each frame
            for (int i = 0; i < objectList.Count; i++)
            {
                try
                {
                    objectList[i].GetComponent<SPO>().turnOn();
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
                objectList[i].GetComponent<SPO>().turnOff();
            }
            catch
            {
                Debug.Log("There is no object " + i.ToString());
            }
            
        }

        yield return new WaitForSecondsRealtime(0.001f);
    }

    // Send markers
    public virtual IEnumerator sendMarkers(int trainingIndex = 99)
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
    public IEnumerator receiveMarkers()
    {
        if (receivingMarkers == false)
        {
            //Get response stream from Python
            print("Looking for a response stream");
            response = GetComponent<LSLResponseStream>();
            int diditwork = response.ResolveResponse();
            print(diditwork.ToString());
            receivingMarkers = true;
        }

        //Set interval at which to receive markers
        float receiveInterval = 0.5f;
        float responseTimeout = 1f;

        // Receive markers continuously
        while(receivingMarkers)
        {
            // Receive markers
            // Initialize the default response string
            string[] defaultResponseStrings = { "" };
            string[] responseStrings = defaultResponseStrings;

            // Pull the python response and add it to the responseStrings array
            responseStrings = response.PullResponse(defaultResponseStrings, responseTimeout);

            // Check if there is 
            bool newResponse = !responseStrings[0].Equals(defaultResponseStrings[0]);
            if (responseStrings[0] != "")
            {
                for (int i = 0; i < responseStrings.Length; i++)
                {
                    string responseString = responseStrings[i];
                    print("WE GOT A RESPONSE");
                    print(responseString);

                    int n;
                    bool isNumeric = int.TryParse(responseString, out n);
                    if (isNumeric == true)
                    {
                        //Run on selection
                        objectList[i].GetComponent<SPO>().onSelection();
                    }
                }
            }

            if (responseStrings.Length > defaultResponseStrings.Length)
            {
                //print(responseStrings);

                //print(responseStrings.Length);

                for (int i = 0; i < responseStrings.Length; i++)
                {
                    string responseString = responseStrings[i];
                    print("WE GOT A RESPONSE");
                    print(responseString);

                    int n;
                    bool isNumeric = int.TryParse(responseString, out n);
                    if (isNumeric == true)
                    {
                        //Run on selection
                        objectList[i].GetComponent<SPO>().onSelection();
                    }

                }
            }

            // Wait for the next receive interval
            yield return new WaitForSecondsRealtime(receiveInterval);
        }

        Debug.Log("Done receiving markers");
    }
}
