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
    
    This is a refactoring of the original P300_Flashes.cs code to update the P300 Dynamic Cubes tool.

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
    o	Off Colour (Colour): Colour of cube object when not flashing //TODO - Do we want this parameter still?
    
    Inputs:
    o   'S' key: Single Flash Start/Stop
    o   'D' key: Redraw Matrix
    o   'Q' key: Quit Program

    NOTE:
    o   Must press Q before closing the application, this will ensure that the LSL Outlet is properly destroyed
    
 */

public class SSVEP_Controller : MonoBehaviour
{
    /* Public Variables */
    public bool setupRequired;          //Determines if setupSSVEP needs to be run or if there are already objects with BCI tag
    public double startX;               //Initial position of X for drawing in the objects
    public double startY;               //Initial position of Y for drawing in the objects
    public float startZ;                //Initial position of Z for drawing in the objects
    public double distanceX;            //Distance between objects in X-plane
    public double distanceY;            //Distance between objects in Y-Plane
    public GameObject myObject;         //Previously 'myCube'. Object type that will be flashing. Default is a cube.
    public Resolution[] resol;          //Resolution of the screen
    public int numRows;                 //Initial number of rows to use
    public int numColumns;              //Initial number of columns to use
    public Color onColour;              //Color during the 'flash' of the object.
    public Color offColour;             //Color when not flashing of the object.
    public bool SendLiveInfo;           //This determines whether or not to send live information about the set-up to LSL.
    public int TargetObjectID;          //This can be used to select a 'target' object for individuals to focus on, using the given int ID.
    public int numTrainingSelections;   //Number of training selections to complete
    public int numTrainingWindows;      //Number of markers to send per selection
    public float windowLength;          //Length of training windows
    //public float windowOverlapRatio;  //Ratio of overlap in the signal, 0 is non overlapping, 1 is 
    public float trainBreak;            //Time in seconds between training trials
    public float[] setFreqFlash;        //frequency of flashes (in Hz) set by the user
    public float[] realFreqFlash;       //frequency of flashes (in Hz) based on the closest approximation possible with given display settings
 


    private string realFreqFlashString; //Nearest possible frequencies as determined by the screen refresh rate
    private bool flashing;              //Flashing On/Off
    private bool training = false;      //Has training occured
    private string markerString;        //The marker string to be sent out

    /* Variables shared with LSL Inlet (to be accessed to flash correct cube) */
    public List<GameObject> objectList = new List<GameObject>();  //Previously 'cube_list'. List of objects that will be flashing, shared with the LSL inlet.

    /* Private Variables */
    private GameObject[,] object_matrix;
    private int s_trials;
    private Dictionary<KeyCode, bool> keyLocks = new Dictionary<KeyCode, bool>();

    //Variables used for checking redraw
    private double current_startx;
    private double current_starty;
    private float current_startz;
    private double current_dx;
    private double current_dy;
    private int current_numrow;
    private int current_numcol;
    private GameObject current_object;
    private bool locked_keys = false;

    // SSVEP vars
    public int refreshRate = 100;
    //public int stim_freq = 10;
    
    //public int markersPerSelection = 5;
    //public int trainLength;
    private int trainLabel;
    private float period;
    private int ISI_count = 0;
    private int[] frames_on = new int[99];          // tells if the flash is on or not
    private int[] frame_count = new int[99];        // number of frames ellapsed
    private int[] frame_on_count = new int[99];     // how many frames does the flash stay on for
    private int[] frame_off_count = new int[99];    // how many frames does the flash stay off for
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

    //Other Scripts to Connect
    //[SerializeField] SSVEP_Setup setup;
    //[SerializeField] LSLMarkerStream marker;

    public SSVEP_Setup setup;
    //public LSLMarkerStream marker;


    private void Awake()
    {
        setup = GetComponent<SSVEP_Setup>();
        marker = GetComponent<LSLMarkerStream>();
        //response = GetComponent<LSLResponseStream>();
        Application.targetFrameRate = refreshRate;

        print(marker);
    }

    private void Start()
    {
        //Get the screen refresh rate, so that the colours can be set appropriately
        resol = Screen.resolutions;

        //Set up LSL Marker Streams (Outlet & Inlet)
        //marker = FindObjectOfType<LSLMarkerStream>();

        //outletSSVEP = FindObjectOfType<Outlet_SSVEP>();

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
        if (CheckEmpty())
        {
            print("Values must be non-zero and non-negative, please re-enter values and press 'D' to redraw...");
            locked_keys = true;
            return;
        }
        //Initialize Matrix
        if (setupRequired == true)
        {
            SetupSSVEP();
        }

        //Add game objects to list

        GameObject[] objectList = GameObject.FindGameObjectsWithTag("BCI");
        //GameObject trainingCube;

        realFreqFlash = new float[objectList.Length];

        //Set all frames to be off, get ready for SSVEP flashing
        for (int i = 0; i < objectList.Length; i++)
        {
            frames_on[i] = 0;
            frame_count[i] = 0;
            period = (float)refreshRate / (float)setFreqFlash[i];
            // could add duty cycle selection here, but for now we will just get a duty cycle as close to 0.5 as possible
            frame_off_count[i] = (int)Math.Ceiling(period / 2);
            frame_on_count[i] = (int)Math.Floor(period / 2);
            realFreqFlash[i] = (refreshRate / (frame_off_count[i] + frame_on_count[i]));
            print("frequency " + (i+1).ToString() + " : " + realFreqFlash[i].ToString());
        }

        // cut the end off of setFlashFreqs
        
        // get a string of the flash_freqs
        realFreqFlashString = string.Join(",", realFreqFlash);

        // Turn flashing off for now
        flashing = false;

        // Set cubes to default colour 
        ResetCubeColour();

        //Run Python
        //runPython.RunP300Python();


    }


    private void Update()
    {
        ISI_count++;
        // Add duty cycle
        // Generate the flashing
        for (int i = 0; i < objectList.Count; i++)
        {
            if (flashing == true)
            {
                frame_count[i]++;
                if (frames_on[i] == 1)
                {
                    if (frame_count[i] >= frame_on_count[i])
                    {
                        // turn the cube off
                        objectList[i].GetComponent<Renderer>().material.color = Color.green;
                        frames_on[i] = 0;
                        frame_count[i] = 0;
                    }
                }
                else
                {
                    if (frame_count[i] >= frame_off_count[i])
                    {
                        // turn the cube on
                        objectList[i].GetComponent<Renderer>().material.color = Color.blue;
                        frames_on[i] = 1;
                        frame_count[i] = 0;
                    }
                }
            }
        }

        // so jank, but this sends the markers 
        if (ISI_count >= refreshRate * windowLength)
        {
            if (flashing == true)
            {
                if (training == true)
                {
                    markerString = windowLength.ToString() + "," + trainLabel.ToString() + "," + realFreqFlashString;
                }
                else
                {
                    markerString = windowLength.ToString() + "," + realFreqFlashString;
                }
                print(markerString);
                marker.Write(markerString);
            }
            ISI_count = 0;
        }

        // Check for a response
        //print(responseExists.ToString());
        //if (responseExists == false && trainingComplete == true)
        //{
        //    print("Looking for a response stream");
        //    response = GetComponent<LSLResponseStream>();
        //    response.ResolveResponse();
        //    responseExists = true;
        //}
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
            if (flashing == false)
            {
                marker.Write("Trial Started");
            }
            else
            {
                marker.Write("Trial Ends");
                ResetCubeColour();
            }

            flashing = !flashing;
        }
        
        // Do Training
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(trainSSVEP());
        }

        // Make a selection, there must be a better way
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartCoroutine(onSelection(0, objectList[0]));
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(onSelection(1, objectList[1]));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(onSelection(2, objectList[2]));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(onSelection(3, objectList[3]));
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(onSelection(4, objectList[4]));
        }

        //Resolve new streams - This is just if you need to refresh the streams.
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    inletP300.ResolveOnRequest();
        //}

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
    private void SetupSSVEP()
    {
        object_matrix = setup.SetUpMatrix(objectList);
        setup.Recolour(objectList, offColour);
    }

    public IEnumerator trainSSVEP()
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

            GameObject trainTarget = Instantiate(myObject);
            trainTarget.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            trainTarget.transform.position = new Vector3(0, 0, 2) + trainingCube.transform.position;

            //
            TargetObjectID = trainingIndex;


            // Run SingleFlash
            print("starting ");
            //RunSingleFlash();
            yield return new WaitForSecondsRealtime(trainBreak);

            trainLabel = trainingIndex;
            flashing = true;

            // Wait for response saying that singleflash is complete
            float timeToTrain = (float)numTrainingWindows * windowLength;// + trainBreak???

            marker.Write("Trial Started");
            yield return new WaitForSecondsRealtime(timeToTrain);
            marker.Write("Trial Ends");

            // Turn off flashing
            flashing = false;
            ResetCubeColour();

            // Destroy the train target
            Destroy(trainTarget);


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

        //bool newResponse = !responseStrings[0].Equals(defaultResponseStrings[0]);

        if (responseStrings.Length > defaultResponseStrings.Length)
        {
            print(responseStrings);
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
        // This is free form, do whatever you want on selection

        print("Obejct " + selectedInd.ToString() + " selected");

        for (int i = 0; i < 3; i++)
        {
            selectedObject.GetComponent<Renderer>().material.color = Color.red;
            yield return new WaitForSecondsRealtime(0.3F);
            selectedObject.GetComponent<Renderer>().material.color = Color.grey;
            yield return new WaitForSecondsRealtime(0.3F);

        }
    }

    /* Checks to see if given values are valid */
    public bool CheckEmpty()
    {
        if (myObject == null || distanceX <= 0 || distanceY <= 0 || numRows <= 0 || numColumns <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Reset all of the SSVEP objects to their default colour
    private void ResetCubeColour()
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            objectList[i].GetComponent<Renderer>().material.color = Color.grey;
        }
    }

    //Write any marker you want!
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
