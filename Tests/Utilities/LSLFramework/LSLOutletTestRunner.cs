using LSL;
using UnityEngine;
using NUnit.Framework;

using static System.Diagnostics.Process;

namespace BCIEssentials.Tests.Utilities.LSLFramework
{
    public class LSLOutletTestRunner: PersistentScenePlayModeTestRunner
    {
        protected static string OutletName
        => $"UnityTestingOutletFor:{CurrentTestName}";
        protected static string OutletType
        => $"TestMarkersFor:{CurrentTestName}";

        protected StreamOutlet Outlet;

        [SetUp]
        public virtual void SetUp()
        {
            Outlet = BuildOutlet();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Outlet.Dispose();
        }


        protected static StreamOutlet BuildOutlet()
        => BuildTypedOutlet(OutletType);

        protected static StreamOutlet BuildTypedOutlet(string type)
        {
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            int processID = GetCurrentProcess().Id;
            var streamInfo = new StreamInfo
            (
                OutletName, type,
                channel_format: channel_format_t.cf_string,
                source_id: $"{deviceID}-Unity-{processID}-{CurrentTestName}"
            );
            return new StreamOutlet(streamInfo);
        }


        protected void PushStringThroughOutlet(string sampleValue)
        {
            Outlet.push_sample(new[] {sampleValue});
            // required to make tests work in sequence,
            // updates some state in Unity or otherwise delays
            // execution long enough for the sample to get through lsl
            Debug.Log($"Writing test marker: {sampleValue}");
        }
    }
}