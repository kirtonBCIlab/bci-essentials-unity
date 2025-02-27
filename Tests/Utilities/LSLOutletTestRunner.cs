using LSL;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

using static System.Diagnostics.Process;

namespace BCIEssentials.Tests.Utilities
{
    public class LSLOutletTestRunner: PlayModeTestRunnerBase
    {
        protected StreamOutlet TestOutlet;
        protected const string TestOutletName = "Unity Testing Outlet";
        protected const string TestOutletType = "Test Markers";

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();
            TestOutlet = BuildOutlet(TestOutletName, TestOutletType);
        }

        [UnityTearDown]
        public void TearDown()
        {
            TestOutlet.Dispose();
        }

        public static StreamOutlet BuildOutlet
        (
            string streamName = "Unity Testing Outlet",
            string streamType = "Test Markers"
        )
        {
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            int processID = GetCurrentProcess().Id;
            var streamInfo = new StreamInfo
            (
                streamName, streamType,
                channel_format: channel_format_t.cf_string,
                source_id: $"{deviceID}-Unity-{processID}"
            );
            return new StreamOutlet(streamInfo);
        }
    }
}