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
    public bool setupRequired;  //Determines if setupSSVEP needs to be run or if there are already objects with BCI tag
    public bool setupOccured;   //Boolean shows if the setup has occured
    public int refreshRate;     //Refresh rate of the Screen
    public float freqHz;        //Frequency in Hz
    public float dutyCycle;     //Previously 'flashLength'. Determines how long an object will remain "on" during a flash. 
    public int numFlashes;      //Previously 'numSamples'. Determines number of times a single object will flash in the series.
    //public double startX;       //Initial position of X for drawing in the objects
    //public double startY;       //Initial position of Y for drawing in the objects
    //public float startZ;        //Initial position of Z for drawing in the objects
    //public double distanceX;    //Distance between objects in X-plane
    //public double distanceY;    //Distance between objects in Y-Plane
    public GameObject myObject; //Previously 'myCube'. Object type that will be flashing. Default is a cube.
    public Resolution[] resol;  //Resolution of the screen
    //public int numRows;         //Initial number of rows to use
    //public int numColumns;      //Initial number of columns to use
    public Color onColour;      //Color during the 'flash' of the object.
    public Color offColour;     //Color when not flashing of the object.
    public int TargetObjectID;  //This can be used to select a 'target' object for individuals to focus on, using the given int ID.
    public int trainingLength;  //Number of training selections to complete
    public float trainBreak;    //Time in seconds between training trials


    public bool listExists = false; //whether the list of objects exists or is tbd
    public bool trialOn = false;    //is a trial occuring
    //Variables for the Boxes
    /* Grid is mapped out as follows:

        c00     c10     c20

        c01     c11     c21

        c02     c12     c22

     */

    /* Variables shared with LSL Inlet (to be accessed to flash correct cube) */
    //public List<GameObject> object_list = new List<GameObject>();  //Previously 'cube_list'. List of objects that will be flashing, shared with the LSL inlet.

    /* Private Variables */
    private GameObject[,] object_matrix;
    public GameObject[] objectList;
    private int s_trials;
    private Dictionary<KeyCode, bool> keyLocks = new Dictionary<KeyCode, bool>();


    private GameObject current_object;
    private bool locked_keys = false;

    /* LSL Variables */
    private LSLMarkerStream marker;
    //private Inlet_P300 inletP300;

    //Other Scripts to Connect
    [SerializeField] Matrix_Setup setup;
    [SerializeField] P300_SingleFlash singleFlash;
    //[SerializeField] RunPython runPython;

    private void Awake()
    {
        setup = GetComponent<Matrix_Setup>();
        singleFlash = GetComponent<P300_SingleFlash>();
        //runPython = GetComponent<RunPython>();

        marker = GetComponent<LSLMarkerStream>();
        Application.targetFrameRate = refreshRate;

        print(marker);
    }

    private void Start()
    {
        //Get the screen refresh rate, so that the colours can be set appropriately
        resol = Screen.resolutions;
        
        refreshRate = resol[3].refreshRate;
        //Set up LSL Marker Streams (Outlet & Inlet)
        //marker = FindObjectOfType<LSLMarkerStream>();
        //inletP300 = FindObjectOfType<Inlet_P300>();

        //Setting up Keys, to lock other keys when one simulation is being run
        keyLocks.Add(KeyCode.S, false);
        keyLocks.Add(KeyCode.D, false);
        keyLocks.Add(KeyCode.T, false);
        locked_keys = false;
        
        // Add statement to check if 
        if (setupRequired == true)
        {
            SetupP300();
        }

        //Run Python
        //runPython.RunP300Python();
    }


    private void Update()
    { 
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
        setup.SetUpMatrix();
    }

    public void RunSingleFlash()
    {
        singleFlash.SetUpSingle();
        singleFlash.SingleFlashes();
    }

    private void RedrawSingleFlash()
    {
        setup.DestroyMatrix();
        SetupP300();
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

    // Turn ON
    public void turnON(GameObject objectToTurnOn)
    {
        objectToTurnOn.GetComponent<Renderer>().material.color = onColour;
    }
    
    // Turn OFF
    public void turnOFF(GameObject objectToTurnOff)
    {
        objectToTurnOff.GetComponent<Renderer>().material.color = offColour;
    }

    // TODO: On Selection

    // Do Training
    public IEnumerator DoTraining()
    {
        System.Random trainRandom = new System.Random();

        //populate the list
        PopulateList("tag");

        GameObject trainingCube;

        // Run this once for each training target
        for (int i = 0; i < trainingLength; i++)
        {
            // Select random cube to train on
            int trainingIndex = trainRandom.Next(((int)objectList.Length - 1));

            print("Running training session " + i.ToString() + " on cube " + trainingIndex.ToString());

            trainingCube = objectList[trainingIndex];

            GameObject trainTarget = Instantiate(myObject);
            trainTarget.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            trainTarget.transform.position = new Vector3(0, 0, 2) + trainingCube.transform.position;

            //
            TargetObjectID = trainingIndex;


            // Run SingleFlash
            print("starting singleflash");
            RunSingleFlash();
            yield return new WaitForSecondsRealtime(trainBreak);

            // Wait for response saying that singleflash is complete
            float timeToTrain = objectList.Length * numFlashes * (1f / freqHz);// + trainBreak???

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

    public void PopulateList(string pMethod)
    {
        //Collect objects with the BCI tag
        if (pMethod == "tag")
        {
            try
            {
                //Add game objects to list by tag "BCI"
                //GameObject[] objectList = GameObject.FindGameObjectsWithTag("BCI");
                objectList = GameObject.FindGameObjectsWithTag("BCI");

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
            // Set cubes to default colour 
            ResetCubeColour();
        }
        else
        {
            print("No object list exists");
        }

    }

    private void ResetCubeColour()
    {
        for (int i = 0; i < objectList.Length; i++)
        {
            turnOFF(objectList[i]);
            //objectList[i].GetComponent<Renderer>().material.color = Color.grey;
        }
    }

}
