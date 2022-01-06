using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Assets.LSL4Unity.Scripts;

/*
P300 Flashes Program
Author: Eli Kinney-Lang
Adapted from: Shaheed Murji "P300_Flashes.cs"
    
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

public class P300_Controller : MonoBehaviour
{
    /* Public Variables */
    public int refreshRate;     //Refresh rate of the Screen
    public float freqHz;        //Frequency in Hz
    public float dutyCycle;     //Previously 'flashLength'. Determines how long an object will remain "on" during a flash. 
    public int numFlashes;      //Previously 'numSamples'. Determines number of times a single object will flash in the series.
    public double startX;       //Initial position of X for drawing in the objects
    public double startY;       //Initial position of Y for drawing in the objects
    public float startZ;        //Initial position of Z for drawing in the objects
    public double distanceX;    //Distance between objects in X-plane
    public double distanceY;    //Distance between objects in Y-Plane
    public GameObject myObject; //Previously 'myCube'. Object type that will be flashing. Default is a cube.
    public Resolution[] resol;  //Resolution of the screen
    public int numRows;         //Initial number of rows to use
    public int numColumns;      //Initial number of columns to use
    public Color onColour;      //Color during the 'flash' of the object.
    public Color offColour;     //Color when not flashing of the object.
    public bool SendLiveInfo;   //This determines whether or not to send live information about the set-up to LSL.
    public int TargetObjectID;  //This can be used to select a 'target' object for individuals to focus on, using the given int ID.
    public int trainingLength;  //Number of training selections to complete
    public float trainBreak;    //Time in seconds between training trials

    //Variables for the Boxes
    /* Grid is mapped out as follows:

        c00     c10     c20

        c01     c11     c21

        c02     c12     c22

     */

    /* Variables shared with LSL Inlet (to be accessed to flash correct cube) */
    public List<GameObject> object_list = new List<GameObject>();  //Previously 'cube_list'. List of objects that will be flashing, shared with the LSL inlet.

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

    /* LSL Variables */
    private LSLMarkerStream marker;
    //private Inlet_P300 inletP300;

    //Other Scripts to Connect
    [SerializeField] P300_Setup setup;
    [SerializeField] P300_SingleFlash singleFlash;
    //[SerializeField] RunPython runPython;

    private void Awake()
    {
        setup = GetComponent<P300_Setup>();
        singleFlash = GetComponent<P300_SingleFlash>();
        //runPython = GetComponent<RunPython>();
    }

    private void Start()
    {
        //Get the screen refresh rate, so that the colours can be set appropriately
        resol = Screen.resolutions;
        refreshRate = resol[3].refreshRate;
        //Set up LSL Marker Streams (Outlet & Inlet)
        marker = FindObjectOfType<LSLMarkerStream>();
        //inletP300 = FindObjectOfType<Inlet_P300>();

        //Setting up Keys, to lock other keys when one simulation is being run
        keyLocks.Add(KeyCode.S, false);
        keyLocks.Add(KeyCode.D, false);
        keyLocks.Add(KeyCode.T, false);
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
        SetupP300();
        //SetUpMatrix();
        //SetUpSingle();
        //SetUpRC();

        SaveCurrentInfo();
        //Set the colour of the box to the given offColour
        //TurnOff();
        //System.Threading.Thread.Sleep(2000);
        SendInfo();

        //Run Python
        //runPython.RunP300Python();
    }


    private void Update()
    {
        //Don't think we need this anymore.

        //if(Input.GetKeyDown(KeyCode.I))
        //{
        //    SetupP300();
        //    Debug.Log("Initialization worked!");
        //}

        if(Input.GetKeyDown(KeyCode.S))
        {
            RunSingleFlash();
            Debug.Log("Single Flash worked!");
        }

        // Do Training
        if(Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(DoTraining());
        }

        ////Resolve new streams - This is just if you need to refresh the streams.
        //if (Input.GetKeyDown(KeyCode.F4))
        //{
        //    inletP300.ResolveOnRequest();
        //}


        ////TODO: Redraw Matrix
        ////Select this after changing parameters
        //if (Input.GetKeyDown(KeyCode.D) && keyLocks[KeyCode.S] == false)
        //{
        //    //Check if values are empty
        //    if (CheckEmpty())
        //    {
        //        print("Values must be non-zero and non-negative, please re-enter values and try again...");
        //        locked_keys = true;
        //        return;
        //    }
        //    keyLocks[KeyCode.D] = true;
        //    //Run our Redraw function.
        //    
        //    RedrawSingleFlash();
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
    private void SetupP300()
    {
        object_matrix = setup.SetUpMatrix(object_list);
        setup.Recolour(object_list,offColour);
    }

    public void RunSingleFlash()
    {
        //print("running singleflash training on target " + targetCube.ToString());
        singleFlash.SetUpSingle();
        singleFlash.SingleFlashes();
    }

    private void RedrawSingleFlash()
    {
        setup.DestroyMatrix();
        SetupP300();
        //singleFlash.Redraw();
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

    /* Write information to LSL for initializations */
    //TODO: Make this more intuitive and flexible for send/receive. JSONify.
    public void SendInfo()
    {
        marker.Write(numRows.ToString());
        marker.Write(numColumns.ToString());
        marker.Write(numFlashes.ToString());
        marker.Write(s_trials.ToString());
    }

    //Send Current information about the current P300 setup
    public void SendCurrentInfo()
    {
        marker.Write("Current rows : "  + numRows.ToString());
        marker.Write("Current cols: "   + numColumns.ToString());
        marker.Write("Num flashes: "    + numFlashes.ToString());
        marker.Write("Target Object: "  + TargetObjectID.ToString());
    }

    /* Save current states into variables for OnValidate to check */
    public void SaveCurrentInfo()
    {
        current_object  = myObject;
        current_startx  = startX;
        current_starty  = startY;
        current_startz  = startZ;
        current_dx      = distanceX;
        current_dy      = distanceY;
        current_numrow  = numRows;
        current_numcol  = numColumns;
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

    // Do Training
    public IEnumerator  DoTraining()
    {
        System.Random trainRandom = new System.Random();
        GameObject[,] objectList = setup.SetUpMatrix(object_list);
        GameObject trainingCube;

        // Get singleflash ready
        //RunSingleFlash();

        
        for (int i = 0; i < trainingLength; i++)
        {
            // Select random cube to train on
            int trainingIndex = trainRandom.Next((numRows * numColumns));

            print("Running training session " + i.ToString() + " on cube " + trainingIndex.ToString());
            // Training goes here

            // Put a slightly larger cube just behind the training cube as a target
            int x = trainingIndex % numColumns;
            int y = (trainingIndex - x) / numColumns;
            trainingCube = objectList[x,y];

            GameObject trainTarget = Instantiate(myObject);
            trainTarget.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            trainTarget.transform.position = new Vector3(0, 0, 2) + trainingCube.transform.position;

            //
            TargetObjectID = trainingIndex;


            // Run SingleFlash
            print("starting singleflash");
            RunSingleFlash();
            yield return new WaitForSecondsRealtime(trainBreak);
            RunSingleFlash();
            //singleFlash.startFlashes = true;
            //singleFlash.SingleFlashes();

            // RunSingleFlash(trainingIndex);

            // Wait for response saying that singleflash is complete
            float timeToTrain = numRows *  numColumns * numFlashes * (1f / freqHz);// + trainBreak???

            yield return new WaitForSecondsRealtime(timeToTrain);
            
            // Destroy the train target
            Destroy(trainTarget);


            print("Training session " + i.ToString() + " complete");

            // 
            //StopSingleFlashes();

        }
        print("Training complete");
        WriteMarker("Training Complete");
    }
    

}
