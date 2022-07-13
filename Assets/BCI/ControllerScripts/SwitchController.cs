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

    public bool trainingComplete;

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
                    objectArray[i].GetComponent<SwitchSPO>().myIndex = i;
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

    public override void StimulusOff()
    {
        // End thhe stimulus Coroutine
        stimOn = false;

        // Send the marker to end
        marker.Write("Trial Ends");
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

        ResetActivations();
        marker.Write("Training Complete");
    }

    public override IEnumerator DoIterativeTraining()
    {
        // Generate the target list
        PopulateObjectList("tag");

        int numOptions = objectList.Count;

        // Create a random non repeating array 
        int[] trainArray = new int[numTrainingSelections];
        trainArray = MakeRNRA(numTrainingSelections, numOptions);
        PrintArray(trainArray);

        yield return 0;


        // Loop for each training target
        for (int i = 0; i < numTrainingSelections; i++)
        {

            if (selectionCounter >= numSelectionsBeforeTraining)
            {
                if (updateCounter == 0)
                {
                    // update the classifier
                    Debug.Log("Updating the classifier after " + selectionCounter.ToString() + " selections");

                    marker.Write("Update Classifier");
                    updateCounter++;
                }
                else if (selectionCounter >= numSelectionsBeforeTraining + (updateCounter * numSelectionsBetweenTraining))
                {
                    // update the classifier
                    Debug.Log("Updating the classifier after " + selectionCounter.ToString() + " selections");

                    marker.Write("Update Classifier");
                    updateCounter++;
                }
            }

            // Get the target from the array
            trainTarget = trainArray[i];

            // 
            Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

            // Turn on train target
            objectList[trainTarget].GetComponent<SPO>().OnTrainTarget();

            // Go through the training sequence
            yield return new WaitForSecondsRealtime(pauseBeforeTraining);


            StimulusOn();
            for (int j = 0; j < (numTrainWindows - 1); j++)
            {
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
            StimulusOff();

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);

            // Turn off train target
            objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();

            // Reset objects

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);

            trainTarget = 99;
            selectionCounter++;
        }

        yield return new WaitForSecondsRealtime(3);

        // Send marker
        trainingComplete = true;
        marker.Write("Training Complete");

        ResetActivations();

        yield return 0;

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
                        int n = 1;
                        foreach (var activationString in activationStrings)
                        {
                            Debug.Log(activationString);

                            // check if activation string is a number
                            float activationFloat;
                            bool isActivation = float.TryParse(activationString, out activationFloat);

                            // if it is then pass the activation floats to OnActivation
                            if (isActivation)
                            {
                                //float activationFloat;
                                //bool didItWork = float.TryParse(activationString, out activationFloat);
                                //Debug.Log(didItWork);

                                // if it is an appropriate time, send the activation
                                if (trainingComplete)
                                {
                                    float activationDuration = (windowLength - 0.5f);
                                    objectList[n].GetComponent<SwitchSPO>().OnActivation(activationFloat, windowLength);
                                }

                                n++;
                            }
                            // otherwise wait for the next response
                            else
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                        //do nothing
                    }
                }
            }

            // Wait for the next receive interval
            yield return new WaitForSecondsRealtime(receiveInterval);
        }

        Debug.Log("Done receiving markers");
    }

    public void ResetActivations()
    {
        foreach (GameObject thisObject in objectList)
        {
            try
            {
                thisObject.GetComponent<SwitchSPO>().ResetPosition();
            }
            catch
            {

            }
        }
    }
}