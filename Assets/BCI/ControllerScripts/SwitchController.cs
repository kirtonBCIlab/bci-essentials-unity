using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/*An extension of the controller class to add Switch functionality
 *

*/
public class SwitchController : Controller
{
    // Variables related to Iterative training
    public int numSelectionsBeforeTraining = 3;        // How many selections to make before creating the classifier
    public int numSelectionsBetweenTraining = 3;       // How many selections to make before updating the classifier


    protected int selectionCounter = 0;
    protected int updateCounter = 0;

    // Populate object list is a little different here because it starts from 1
    public override void PopulateObjectList(string popMethod)
    {
        // Remove everything from the existing list
        objectList.Clear();

        // Add neutral object
        GameObject neutralObject = GameObject.FindGameObjectWithTag("Neutral");
        objectList.Add(neutralObject);


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

                //The object list exists
                listExists = true;
            }
            catch
            {
                //the list does not exist
                print("Unable to create a list based on 'BCI' object tag");

                // Clear the list
                objectList.Clear();

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
    }

    // Coroutine for the stimulus, wait there is no stimulus
    public override IEnumerator Stimulus()
    {
        yield return 0;
    }

    public override IEnumerator DoTraining()
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
                objectList[trainTarget].GetComponent<SPO>().OnSelection();
            }

            trainTarget = 99;

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);
        }

        marker.Write("Training Complete");
    }

    public override IEnumerator SendMarkers(int trainingIndex = 99)
    {
        // Make the marker string, this will change based on the paradigm
        while (stimOn)
        {
            // Desired format is: [switch, number of switches, window length, training labels (or -1 if n/a)] 
            string trainingString;
            if (trainingIndex <= objectList.Count)
            {
                trainingString = trainingIndex.ToString();
            }
            else
            {
                trainingString = "-1";
            }

            string markerString = "switch," + objectList.Count.ToString() + "," + trainingString + "," + windowLength.ToString();

            //string markerString = "switch," + objectList.Count.ToString() + "," +  windowLength.ToString() +  "," + trainingString;

            // Send the marker
            marker.Write(markerString);

            // Wait the window length + the inter-window interval
            yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);


        }
    }

    // Coroutine to continuously receive markers
    public override IEnumerator ReceiveMarkers()
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
        float receiveInterval = 1 / refreshRate;
        float responseTimeout = 0f;

        //Ping count
        int pingCount = 0;

        // Receive markers continuously
        while (receivingMarkers)
        {
            // Receive markers
            // Initialize the default response string
            string[] defaultResponseStrings = { "" };
            string[] responseStrings = defaultResponseStrings;

            // Pull the python response and add it to the responseStrings array
            responseStrings = response.PullResponse(defaultResponseStrings, responseTimeout);

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


                    // want the response in format float1, float2, ..., floatN
                    try
                    {
                        string[] activationStrings = responseString.Split(',');
                        int n = 0;
                        foreach (var activationString in activationStrings)
                        {
                            float activationFloat;
                            bool didItWork = float.TryParse(activationString, out activationFloat);


                            objectList[n].GetComponent<SwitchSPO>().OnActivation(activationFloat);

                            n++;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            // Wait for the next receive interval
            yield return new WaitForSecondsRealtime(receiveInterval);
        }

        Debug.Log("Done receiving markers");
    }
}