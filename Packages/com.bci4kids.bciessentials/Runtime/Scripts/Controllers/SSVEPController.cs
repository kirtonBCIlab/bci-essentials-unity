using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BCIEssentials.StimulusObjects;

public class SSVEPController : Controller
{
    public float[] setFreqFlash;
    public float[] realFreqFlash;

    private int[] frames_on = new int[99];
    //private int[] frames_off = new int[99];
    private int[] frame_count = new int[99];
    private float period;
    private int[] frame_off_count = new int[99];
    private int[] frame_on_count = new int[99];

    void Awake()
    {
        // Set vote on windows to true because we want to make exactly one decision for all windows in a given selection
        voteOnWindows = true;
    }

    public override void PopulateObjectList(string popMethod)
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
                GameObject[] objectArray = GameObject.FindGameObjectsWithTag("BCI");
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
        foreach (GameObject thisObject in objectList)
        {
            if (thisObject.GetComponent<SPO>().Selectable == false)
            {
                objectsToRemove.Add(thisObject);
            }
        }
        foreach (GameObject thisObject in objectsToRemove)
        {
            objectList.Remove(thisObject);
        }
        objectsToRemove.Clear();

        realFreqFlash = new float[objectList.Count];

        for (int i = 0; i < objectList.Count; i++)
        {
            frames_on[i] = 0;
            frame_count[i] = 0;
            period = (float)refreshRate / (float)setFreqFlash[i];
            // could add duty cycle selection here, but for now we will just get a duty cycle as close to 0.5 as possible
            frame_off_count[i] = (int)Math.Ceiling(period / 2);
            frame_on_count[i] = (int)Math.Floor(period / 2);
            realFreqFlash[i] = (refreshRate / (float)(frame_off_count[i] + frame_on_count[i]));
            print("frequency " + (i + 1).ToString() + " : " + realFreqFlash[i].ToString());
        }
    }

    public override IEnumerator SendMarkers(int trainingIndex = 99)
    {
        // Make the marker string, this will change based on the paradigm
        while (stimOn)
        {
            // Desired format is: ["ssvep", number of options, training target (-1 if n/a), window length, frequencies]
            string freqString = "";
            for (int i = 0; i < realFreqFlash.Length; i++)
            {
                freqString = freqString + "," + realFreqFlash[i].ToString();
            }

            string trainingString;
            if (trainingIndex <= objectList.Count)
            {
                trainingString = trainingIndex.ToString();
            }
            else
            {
                trainingString = "-1";
            }

            string markerString = "ssvep," + objectList.Count.ToString() + "," + trainingString + "," + windowLength.ToString() + freqString;

            // Send the marker
            marker.Write(markerString);

            // Wait the window length + the inter-window interval
            yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);


        }
    }

    public override IEnumerator Stimulus()
    {
        // Get a list of the SPO components
        SPO[] SPOList = new SPO[objectList.Count];
        for(int i=0; i<objectList.Count; i++)
        {
            SPOList[i] = objectList[i].GetComponent<SPO>();
        }

        while (stimOn)
        {
            // Generate the flashing
            for (int i = 0; i < objectList.Count; i++)
            {
                frame_count[i]++;
                if (frames_on[i] == 1)
                {
                    if (frame_count[i] >= frame_on_count[i])
                    {
                        // turn the cube off
                        SPOList[i].StopStimulus();
                        frames_on[i] = 0;
                        frame_count[i] = 0;
                    }
                }
                else
                {
                    if (frame_count[i] >= frame_off_count[i])
                    {
                        // turn the cube on
                        SPOList[i].StartStimulus();
                        frames_on[i] = 1;
                        frame_count[i] = 0;
                    }
                }
            }
            yield return 0;
        }

        // Turn them all off when we are done
        for (int i = 0; i < objectList.Count; i++)
        {
            // turn the cube off
            SPOList[i].StopStimulus();
        }
    }
}
