using UnityEngine;
using System.Collections;
using LSL;
using System.Diagnostics;
using Assets;

namespace Assets.LSL4Unity.Scripts
{
    //public enum MomentForSampling { Update, FixedUpdate, LateUpdate }


    public class LSL_P300 : MonoBehaviour
    {
        private const string unique_source_id_suffix = "63CE5B03731944F6AC30DBB04B451A94";

        private string unique_source_id;

        private liblsl.StreamOutlet outlet;
        private liblsl.StreamInfo streamInfo;
        private float[] currentSample;

        public string StreamName = "Unity.ExampleStream";
        public string StreamType = "Unity.FixedUpdateTime";
        public int ChannelCount = 1; 

        Stopwatch watch;
        /// <summary>
        /// Due to an instable framerate we assume a irregular data rate.
        /// </summary>
        private const double dataRate = liblsl.IRREGULAR_RATE;

        void Awake(){
            //Assigning a unique source id as a combination of the instance ID for the case
            //that multiple LSLP300 are used and a guide identifying the script itself
            unique_source_id = string.Format("{0}_{1}", GetInstanceID(), unique_source_id_suffix);
        }

        // Use this for initialization
        void Start()
        {
            watch = new Stopwatch();

            watch.Start();

            currentSample = new float[ChannelCount];

            streamInfo = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, dataRate, 
                                            liblsl.channel_format_t.cf_float32, unique_source_id);

            outlet = new liblsl.StreamOutlet(streamInfo);
        }

        public void FixedUpdate()
        {
            if (watch == null || outlet == null)
                return;

            watch.Stop();

            currentSample[0] = watch.ElapsedMilliseconds;

            watch.Reset();
            watch.Start();

            outlet.push_sample(currentSample, liblsl.local_clock());
        }
    }
}