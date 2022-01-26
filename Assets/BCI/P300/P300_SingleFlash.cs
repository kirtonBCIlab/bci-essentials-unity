using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P300_SingleFlash : MonoBehaviour
{
    //Main connection to the P300 controller.
    //[SerializeField] P300_Controller p300_controller;
    public P300_Controller p300_controller;
    public P300_Events p300_events;
    public Matrix_Setup setup;
    

    public bool startFlashes;   //Whether to automatically start the flashes on awake.
    

    //Variables associated with counting the flashes

    //private int counter = 0;
    private List<int> flash_counter = new List<int>();
    private List<int> s_indexes = new List<int>();
    //private int numTrials = 0; //CAUTION- THIS MAY BE WHAT IS CAUSING AN ISSUE IF NOT RESET APPROPRIATELY.



    private void Awake()
    {
        p300_controller = GetComponent<P300_Controller>();
        p300_events = GetComponent<P300_Events>();
        setup = GetComponent<Matrix_Setup>();
    }


    /* Configures array and simulation values for Single flashes */
    public void SetUpSingle()
    {
        /*
                Grid                  Number
        c00     c10     c20         0   1   2

        c01     c11     c21     ->  3   4   5

        c02     c12     c22         6   7   8
        */

        //int numRows = setup.numRows;
        //int numCols = setup.numColumns;

        int listLength = p300_controller.objectList.Length;
        int numSamples = p300_controller.numFlashes;


        //Setting counters for each cube
        for (int i = 0; i < (listLength); i++)
        {
            flash_counter.Add(numSamples);
        }

        //Set up test single indices
        for (int i = 0; i < (listLength); i++)
        {
            s_indexes.Add(i);
        }

        print("---------- SINGLE FLASH DETAILS ----------");
        //print("Number of Trials will be: " + numTrials);
        print("Number of flashes for each cell: " + numSamples);
        print("--------------------------------------");
        TurnOffSingle();
        startFlashes = !startFlashes;
    }

    //Simple call to run or stop the coroutines based on if the start_flashes call is true or not
    public void SingleFlashes()
    {
        if (startFlashes)
        {
            StartCoroutine("SingleFlashCor");
        }
        else
        {
            StopSingleFlashes();
        }

    }

    public void StopSingleFlashes()
    {
        //Turn off the flash boolean and stop the coroutine.
        startFlashes = false;
        StopCoroutine("SingleFlashCor");
        p300_controller.WriteMarker("P300 SingleFlash Ends");
        ResetSingleCounters();
        print("Counters Reset! Hit S again to run P300 SingleFlash");
    }

    /* Single Flash Operation */
    public IEnumerator SingleFlashCor()
    {
        //Write that this coroutine has started
        p300_controller.WriteMarker("P300 SingleFlash Started");
        // if we are going to send additional details about the flashing, I think this would be a good time

        int targetId = p300_controller.TargetObjectID;

        // Get the timeOn and timeOff from the flash frequncy and duty cycle
        float timeOn = (1f / p300_controller.freqHz) * p300_controller.dutyCycle;
        float timeOff = (1f / p300_controller.freqHz) * (1f - p300_controller.dutyCycle);
        
        int randomCube;
        int lastRandomCube = 99999;         //Makes sure that we don't flash same cube twice in a row
        string selectionString = "";        // string of selections for debuging
        string markerData;                  // markerData to be printed
        System.Random flashRandom = new System.Random();
        while (startFlashes)
        {
            //Turn off the cubes to give the flashing image
            TurnOffSingle();

            // If there are no more cubes to select, then quit
            if (s_indexes.Count < 1)
            {
                // Print the single flash ends to console
                print("Done P300 Single Flash Trials");
                // 
                p300_controller.WriteMarker("P300 SingleFlash Ends");
                break;
            }
            // If there is only one cube to select, you must select that one
            else if (s_indexes.Count == 1)
            {
                randomCube = s_indexes[0];
            }
            // Otherwise get select a random cube to flash
            else
            {
                randomCube = GetRandomFromList(s_indexes, flashRandom);
            }
            // If it is the same cube that just flashed, select a different random cube
            if (randomCube == lastRandomCube && s_indexes.Count != 1)
            {
                while (randomCube == lastRandomCube)
                {
                    randomCube = GetRandomFromList(s_indexes, flashRandom);
                }
            }
            // Reset the most recently selected cube
            lastRandomCube = randomCube;

            // debug
            selectionString = selectionString + randomCube.ToString();

            //If the counter is non-zero, then flash that cube and decrement the flash counter
            if (flash_counter[randomCube] > 0)
            {
                // Wait timeoff before turning on the stim
                yield return new WaitForSecondsRealtime(timeOff);

                // Wait timeOff seconds before turning on
                print(randomCube.ToString());
                p300_controller.turnON(p300_controller.objectList[randomCube]);
                //p300_controller.objectList[randomCube].TurnOn();

                //p300_controller.turnON

                //Handle events if this is the target cube or not //NEW!
                if (randomCube == p300_controller.TargetObjectID)
                {
                    OnTargetFlash();
                }
                else
                {
                    OnNonTargetFlash();
                }

                flash_counter[randomCube]--;
                
                //print("OBJECT: " + randomCube.ToString());
                // Get marker data
                markerData = "s," + randomCube.ToString() + "," + targetId.ToString();
                
                // Write the selected cube to the console
                Debug.Log(markerData);
                // Write the selected cube to the LSL Outlet stream
                p300_controller.WriteMarker(markerData);
            }
            if (flash_counter[randomCube] == 0)
            {
                s_indexes.Remove(randomCube);
            }

            // Wait timeOn seconds before turning off
            yield return new WaitForSecondsRealtime(timeOn);

        }
        //print(selectionString);

        ResetSingleCounters();

        // Reset all cubes to OFF state
        p300_controller.ResetCubeColour();


        //Write to LSL stream to indicate end of P300 SingleFlash
        //This is all things to do on the P300 controller.
        p300_controller.WriteMarker("P300 SingleFlash Ends");//marker.Write("P300 SingleFlash Ends");
        startFlashes = !startFlashes;
        p300_controller.LockKeysToggle(KeyCode.S);//keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];

    }

    //Turn off all object values
    public void TurnOffSingle()
    {
        for (int i = 0; i < p300_controller.objectList.Length; i++)
        {
            p300_controller.turnOFF(p300_controller.objectList[i]);
            //p300_controller.objectList[i].TurnOff();
        }
    }

    /* Resets all counters and clear arrays */
    public void ResetSingleCounters()
    {
        //counter = 0;
        flash_counter.Clear();
        s_indexes.Clear();
        //numTrials = 0;
    }


    //TODO: Add back in Redraw capabilities for rapid changes.

    //public void Redraw()
    //{
    //    print("Redrawing Matrix");
    //    TurnOffSingle();
    //    ResetSingleCounters();
    //    p300_controller.objectList.Clear();
    //    SetUpSingle();

    //}

    // Get a random value from a list, input the list and a random object
    private int GetRandomFromList(List<int> list, System.Random thisRandom)
    {
        int randomIndex = thisRandom.Next(list.Count);
        int randomValue = list[randomIndex];
        return randomValue;
    }


    //Dealing with events
    private void OnTargetFlash()
    {
        p300_events.TargetFlashEvent();
    }

    private void OnNonTargetFlash()
    {
        p300_events.NonTargetFlashEvent();
    }
}