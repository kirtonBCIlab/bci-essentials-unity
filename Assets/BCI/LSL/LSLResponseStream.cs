using UnityEngine;
using System.Collections;
using LSL4Unity;
using LSL;

namespace LSL4Unity
{
    //[HelpURL("https://github.com/xfleckx/LSL4Unity/wiki#using-a-marker-stream")]
    public class LSLResponseStream : MonoBehaviour
    {
        //The predicate by which to recognize the python response stream
        public string responsePredicate = "name='PythonResponse'";
        private string[] responseStrings;

        public liblsl.StreamInfo[] responseInfo;
        public liblsl.StreamInlet responseInlet;
        //public responseInlet  liblsl.StreamInlet(responseInfo[0]);
        //public liblsl.StreamInlet(responseInfo) responseInlet;

        // responseInlet.open_stream();
        public string value = "PythonResponse";
        public int pyRespIndex;

        
        public int ResolveResponse()
        {
            // Resolve stream not working, crashes unity, use resolve streams instead and then find a way to pick the right one
            responseInfo = liblsl.resolve_streams();
            
            for (int i = 0; i < responseInfo.Length; i++)
            {
                print("Response info " + i.ToString() + ":\n");
                print(responseInfo[i].name());

                if(responseInfo[i].name() == value)
                {
                    pyRespIndex = i;
                    print("Got Python Response");
                    responseInlet = new liblsl.StreamInlet(responseInfo[i]);
                    print("Created the inlet");
                    responseInlet.open_stream();
                    print("Opened the stream");
                }
            }
            return 1;

        }
       

        public string[] PullResponse()
        {
            // Pull sample
            double timeout = responseInlet.pull_sample(responseStrings);
            return responseStrings;
        }
    }
}
