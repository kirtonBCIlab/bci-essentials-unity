using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLResponseStream : MonoBehaviour
{
    //The predicate by which to recognize the python response stream
    public string responsePredicate = "name='PythonResponse'";
    //private string[] responseStrings = {""};

    public StreamInfo[] responseInfo;
    public StreamInlet responseInlet;
    //public responseInlet  liblsl.StreamInlet(responseInfo[0]);
    //public liblsl.StreamInlet(responseInfo) responseInlet;

    // responseInlet.open_stream();
    public string value = "PythonResponse";
    public int pyRespIndex;


    public int ResolveResponse()
    {
        // Resolve stream not working, crashes unity, use resolve streams instead and then find a way to pick the right one
        responseInfo = LSL.LSL.resolve_streams();

        for (int i = 0; i < responseInfo.Length; i++)
        {
            Debug.Log("Response info " + i.ToString() + ":\n");
            Debug.Log(responseInfo[i].name());

            if (responseInfo[i].name() == value)
            {
                pyRespIndex = i;
                Debug.Log("Got Python Response");
                responseInlet = new StreamInlet(responseInfo[i]);
                Debug.Log("Created the inlet");

                //responseInlet.open_stream();
                //print("Opened the stream");

                // Try to open the stream, timeout after 2 seconds
                try
                {
                    double timeout = 2.0;
                    responseInlet.open_stream(timeout);
                    Debug.Log("Opened the stream successfully");

                    // If we are successful in opening the python stream then we do not need to look further
                    i = 99;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }

        // Tried moving it down here so that it only opens the last Python Response stream, and it still crashes
        // Aparently this is unnecessary anyway because if the stream is not opened it will be opened implicitly 
        //double timeout = 2.0;
        //responseInlet.open_stream(timeout);
        //print("Opened the stream");

        return 1;

    }

    //void Start()
    //{
    //    StreamInfo streamInfo = new StreamInfo(StreamName, StreamType, 1, liblsl.IRREGULAR_RATE, LSL.channel_format_t.cf_float32);

    //    intlet = new StreamOutlet(streamInfo);
    //}

    public string[] PullResponse(string[] responseStrings, double timeout)
    {
        // Try to pull sample
        try
        {
            //double timeout = 0.1;
            double result = responseInlet.pull_sample(responseStrings, timeout);
            if (result != 0)
            {
                //print(result);
                for (int i = 0; i < responseStrings.Length; i++)
                {
                    //print(responseStrings[i]);
                }
            }

        }
        catch //(Exception e)
        {
            //print(e.Message);

        }
        return responseStrings;
    }

}