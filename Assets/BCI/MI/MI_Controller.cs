using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using LSL4Unity;


/*
SSVEP Controller Script
Author: Brian Irvine
Adapted from: Eli Kinney-Lang & Shaheed Murji "P300_Flashes.cs"

    Parameters here have been updated, and the master script has been broken out to better support modulatrity.

    //Updated Descriptions
    User configured matrix which flashes cubes either in a single or row/column fashion. User defined parameters which allow 
    a variety of different use cases.

    Parameters: 
    o	Freq Hz (Hz): this is how frequent the object will flash
    o	Duty Cycle (s): how long an object will remain "on" during a flash. This is equivalent to the idea of "Duty Cycle" in LEDs.
    o	Num flashes (#): number of times a single object will flash in the series
    o	Distance X (units): distance between each cube on the x-axis
    o	Distance Y (units): distance between each cube on the y-axis
    o	My Cube (prefab): a prefab of a cube object that will be instantiated at runtime //TODO - Change name here?
    o	Num Rows (#): number of rows
    o	Num Columns (#): number of columns
    o   Target (#): cube which is designated as the 'target' cube for an Auditory run or bookkeeping purposes
    o   Target_SFX (.mp4/.wav): sound to be played when target object flashes.
    o   NonTarget_SFX (.mp4/.wav): sound to be played when non-target objects flash.
    o	On Colour (Colour): Colour of cube object when flashed //TODO - Do we want to have this parameter still?
    o	Off Colour (Colour): Colour of cube object when not trialOn //TODO - Do we want this parameter still?
    
    Inputs:
    o   'S' key: Single Flash Start/Stop
    o   'D' key: Redraw Matrix
    o   'Q' key: Quit Program

    NOTE:
    o   Must press Q before closing the application, this will ensure that the LSL Outlet is properly destroyed
    
 */

public class MI_Controller : MonoBehaviour
{
    /* Public Variables */
    public bool setupRequired;          //Determines if MI_Setup needs to be run or if there are already objects with BCI tag
    public Color onColour;              //Color during the 'flash' of the object.
    public Color offColour;             //Color when not flashing of the object.
    public bool SendLiveInfo;           //This determines whether or not to send live information about the set-up to LSL.
    public int TargetObjectID;          //This can be used to select a 'target' object for individuals to focus on, using the given int ID.
    public int numTrainingSelections;   //Number of training selections to complete
    public int numTrainingWindows;      //Number of markers to send per selection
    public float windowLength;          //Length of training windows
    //public float windowOverlapRatio;  //Ratio of overlap in the signal, 0 is non overlapping, 1 is 
    public float trainBreak;            //Time in seconds between training trials

    // Sham feedback during training
    public bool shamFeedback = false;
    public float shamAccuracy = 0.9f;

    private bool training = false;      //Has training occured
    private string markerString;        //The marker string to be sent out

    /* Variables shared with LSL Inlet (to be accessed to flash correct cube) */
    public GameObject[] objectList;  //Previously 'cube_list'. List of objects that will be flashing, shared with the LSL inlet.
    public bool listExists = false;

    //TRAINING
    public GameObject targetCube;

    /* Private Variables */
    private GameObject[,] object_matrix;
    private int s_trials;
    private Dictionary<KeyCode, bool> keyLocks = new Dictionary<KeyCode, bool>();

    //Variables used for checking redraw
    //private double current_startx;
    //private double current_starty;
    //private float current_startz;
    //private double current_dx;
    //private double current_dy;
    //private int current_numrow;
    //private int current_numcol;
    private GameObject current_object;
    private bool locked_keys = false;

    // 
    public int refreshRate = 100;
    private int ISI_count = 0;
    //public int stim_freq = 10;

    //public int markersPerSelection = 5;
    //public int trainLength;
    private int trainLabel;
    public GameObject cube;

    /* LSL Variables */
    public LSLMarkerStream marker;
    public LSLResponseStream response;
    public bool trainingComplete = false;
    public bool responseExists = false;
    private double responseTimeout = 0.008;
    private double responseDelay = 0.2;             // Time in seconds to wait between checking for a python response
    private double responseDelayFrames;
    private int responseDelayFrameCount = 0;
    //private Unity_SSVEP unitySSVEP;

    // Tells if the trial is running
    public bool trialOn = false;

    //Other Scripts to Connect
    //[SerializeField] SSVEP_Setup setup;
    //[SerializeField] LSLMarkerStream marker;

    //public MI_Setup setup;
    private Matrix_Setup setup;
    //public LSLMarkerStream marker;

    // MI Specific
    public GameObject cursor;



    private void Awake()
    {
        setup = GetComponent<Matrix_Setup>();
        marker = GetComponent<LSLMarkerStream>();
        //response = GetComponent<LSLResponseStream>();
        Application.targetFrameRate = refreshRate;

        print(marker);
    }

    private void Start()
    {
        //Get the screen refresh rate, so that the colours can be set appropriately
        //resol = Screen.resolutions;

        //Setting up Keys, to lock other keys when one simulation is being run
        keyLocks.Add(KeyCode.S, false);
        keyLocks.Add(KeyCode.D, false);
        keyLocks.Add(KeyCode.T, false);
        keyLocks.Add(KeyCode.P, false);
        locked_keys = false;

        //Starting with sending the live information as false.
        SendLiveInfo = false;

        //Check to see if inputs are valid, if not, then don't draw matrix and prompt user to redraw with the
        //correct inputs
        //if (CheckEmpty())
        //{
        //    print("Values must be non-zero and non-negative, please re-enter values and press 'D' to redraw...");
        //    locked_keys = true;
        //    return;
        //}

        //Initialize Matrix
        if (setupRequired == true)
        {
            SetupMI();
        }

        //Populate the list (REMOVE THIS LATER)
        PopulateList("tag");

    }


    private void Update()
    {
        // Get the object list if it is not defined
        if (listExists == true)
        {
            ISI_count++;
            // so jank, but this sends the markers 
            // put into a coroutine
            if (ISI_count >= refreshRate * windowLength)
            {
                if (trialOn == true)
                {
                    if (training == true)
                    {
                        markerString = windowLength.ToString() + "," + trainLabel.ToString();
                    }
                    else
                    {
                        markerString = windowLength.ToString();
                    }
                    print(markerString);
                    marker.Write(markerString);
                }
                ISI_count = 0;
            }

            if (responseExists == true)
            {
                // Pull a response
                //Debug.Log("pulling python response");
                // Check if enough frames have elapsed to pull another python response
                responseDelayFrames = responseDelay * (double)refreshRate;
                if (responseDelayFrameCount > (int)responseDelayFrames)
                {
                    StartCoroutine(pullPythonResponse());
                    responseDelayFrameCount = 0;
                }
                responseDelayFrameCount++;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                //RunSingleFlash();
                // Debug.Log("Single Flash worked!");
                //StartSSVEP(freqFlash);
                if (trialOn == false)
                {
                    marker.Write("Trial Started");
                }
                else
                {
                    marker.Write("Trial Ends");
                    ResetCubeColour();
                }

                trialOn = !trialOn;
            }

            // Do Training
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartCoroutine(trainMI());
            }

            // Make a selection
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                StartCoroutine(onSelection(0, objectList[0]));
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(onSelection(1, objectList[1]));
            }

        }

        //Quit Program 
        if (Input.GetKeyDown(KeyCode.Q))
        {
            print("Quitting Program...");
            marker.Write("Quit");
            marker = null;
            Application.Quit();
        }
    }

    //Setting up the scene:
    private void SetupMI()
    {
        setup.SetUpMatrix();
        //setup.Recolour(objectList, offColour);
    }

    //Populating the SSVEP object list
    public void PopulateList(string pMethod)
    {
        //Collect objects with the BCI tag
        if (pMethod == "tag")
        {
            try
            {
                //Add game objects to list by tag "BCI"
                GameObject[] objectList = GameObject.FindGameObjectsWithTag("BCI");

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
        else if (pMethod == "predefined")
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

        if (listExists == true)
        {
            ////Initialize the real flash frequency list
            ////realFreqFlash = new float[objectList.Length];
            //realFreqFlash = new float[objectList.Count];

            //Set all frames to be off, get ready for SSVEP trialOn
            //for (int i = 0; i < objectList.Length; i++)
            //for (int i = 0; i < objectList.Count; i++)
            //{
            //    //frames_on[i] = 0;
            //    //frame_count[i] = 0;
            //    //period = (float)refreshRate / (float)setFreqFlash[i];
            //    // could add duty cycle selection here, but for now we will just get a duty cycle as close to 0.5 as possible
            //    frame_off_count[i] = (int)Math.Ceiling(period / 2);
            //    frame_on_count[i] = (int)Math.Floor(period / 2);
            //    realFreqFlash[i] = (refreshRate / (frame_off_count[i] + frame_on_count[i]));
            //    print("frequency " + (i + 1).ToString() + " : " + realFreqFlash[i].ToString());
            //}

            // cut the end off of setFlashFreqs

            // get a string of the flash_freqs
            //realFreqFlashString = string.Join(",", realFreqFlash);

            // Turn trialOn off for now
            trialOn = false;

            // Set cubes to default colour 
            ResetCubeColour();
        }
        else
        {
            print("No object list exists");
        }

    }

    //Training
    public IEnumerator trainMI()
    {
        if (responseExists == false)
        {
            //Get response stream from Python
            print("Looking for a response stream");
            response = GetComponent<LSLResponseStream>();
            int diditwork = response.ResolveResponse();
            print(diditwork.ToString());
            responseExists = true;
        }

        //Set up the random generator
        System.Random trainRandom = new System.Random();
        GameObject[] objectList = GameObject.FindGameObjectsWithTag("BCI");
        GameObject trainingCube;

        // set training to true
        training = true;
        int trainingIndex;

        // Create training array
        int[] unshuffledTrainArray = new int[numTrainingSelections];
        for (int k = 0; k < numTrainingSelections; k++)
        {
            if (k >= objectList.Count())
            {
                unshuffledTrainArray[k] = k % objectList.Count();
            }
            else
            {
                unshuffledTrainArray[k] = k;
            }
        }

        System.Random randomShuffle = new System.Random();
        int[] shuffledTrainArray = unshuffledTrainArray.OrderBy(x => randomShuffle.Next()).ToArray();

        for (int i = 0; i < numTrainingSelections; i++)
        {
            trainingIndex = shuffledTrainArray[i];

            print("Running training session " + i.ToString() + " on cube " + trainingIndex.ToString());

            trainingCube = objectList[trainingIndex];

            GameObject trainTarget = Instantiate(targetCube);
            trainTarget.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            trainTarget.transform.position = new Vector3(0, 0, 2) + trainingCube.transform.position;

            //
            TargetObjectID = trainingIndex;


            // Run SingleFlash
            print("starting ");

            // Replace cursor to center
            cursor.transform.position = new Vector3(2, 0, 0);



            //RunSingleFlash();
            yield return new WaitForSecondsRealtime(trainBreak);

            trainLabel = trainingIndex;
            trialOn = true;

            // Wait for response saying that singleflash is complete
            float timeToTrain = (float)numTrainingWindows * windowLength;// + trainBreak???

            marker.Write("Trial Started");
            yield return new WaitForSecondsRealtime(timeToTrain);
            marker.Write("Trial Ends");



            //Deliver sham feedback
            if (shamFeedback == true)
            {
                StartCoroutine(onSelection(trainLabel, objectList[trainLabel]));
            }

            //Destroy the training target
            Destroy(trainTarget);

            // Pause before next trial
            yield return new WaitForSecondsRealtime(trainBreak);


            // Turn off trialOn
            trialOn = false;
            ResetCubeColour();

            // Destroy the train target
            


            print("Training session " + i.ToString() + " complete");

        }
        print("Training complete");
        WriteMarker("Training Complete");

        training = false;
        trainingComplete = true;

    }

    public IEnumerator pullPythonResponse()
    {
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
                    StartCoroutine(onSelection(n, objectList[n]));
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
                    StartCoroutine(onSelection(n, objectList[n]));
                }

            }
        }
        //if (responseStrings.Count() > 0)
        //{
        //    for (int i = 1; i < responseStrings.Count(); i++)
        //    {
        //        string responseString = responseStrings[i];
        //        print("WE GOT A RESPONSE");
        //        print(responseString);

        //        int n;
        //        bool isNumeric = int.TryParse(responseString, out n);
        //        if (isNumeric == true)
        //        {
        //            //Run on selection
        //            StartCoroutine(onSelection(n, objectList[n]));
        //        }

        //    }
        //}
        yield return new WaitForSeconds(0.001f);
    }

    public IEnumerator onSelection(int selectedInd, GameObject selectedObject)
    {
        //Turn off the trialOn and reset
        trialOn = false;
        ResetCubeColour();

        print("Object " + selectedInd.ToString() + " selected");

        //LERP the cursor

        // LERP over 0.5 seconds
        int interpFrameCount = (int)(refreshRate / 2);


        float lerpTime = 0f;
        float lerpDuration = 1f;
        Vector3 startPosition = cursor.transform.position;
        Vector3 targetPosition = selectedObject.transform.position;

        while (lerpTime < lerpDuration)
        {
            cursor.transform.position = Vector3.Lerp(startPosition, targetPosition, lerpTime / lerpDuration);
            lerpTime += Time.deltaTime;
            yield return null;
        }
        cursor.transform.position = targetPosition;



        // Flash the target
        //for (int i = 0; i < 3; i++)
        //{
        //    selectedObject.GetComponent<Renderer>().material.color = Color.red;
        //    yield return new WaitForSecondsRealtime(0.3F);
        //    selectedObject.GetComponent<Renderer>().material.color = Color.grey;
        //    yield return new WaitForSecondsRealtime(0.3F);

        //}
    }

    /* Checks to see if given values are valid */
    // Do I need this????
    //public bool CheckEmpty()
    //{
    //    if (myObject == null || distanceX <= 0 || distanceY <= 0 || numRows <= 0 || numColumns <= 0)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    // Reset all of the SSVEP objects to their default colour
    private void ResetCubeColour()
    {
        for (int i = 0; i < objectList.Count(); i++)
        {
            objectList[i].GetComponent<Renderer>().material.color = Color.grey;
        }
    }

    //Write any marker you want!
    // Do I need this if it is only one line???
    public void WriteMarker(string markerString)
    {
        marker.Write(markerString);
    }

    //Toggle key locks on/off
    public void LockKeysToggle(KeyCode key)
    {
        keyLocks[key] = keyLocks[key];
        locked_keys = !locked_keys;
    }
}