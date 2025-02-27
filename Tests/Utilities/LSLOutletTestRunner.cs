using LSL;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

using static System.Diagnostics.Process;
using NUnit.Framework;

namespace BCIEssentials.Tests.Utilities
{
    public class LSLOutletTestRunner: PlayModeTestRunnerBase
    {
        protected StreamOutlet PersistentOutlet;
        protected const string PersistentOutletName = "UnityTestingOutlet";
        protected const string PersistentOutletType = "TestMarkers";
        protected static string TestScopeOutletType
        => $"TestMarkersFor:{CurrentTestName}";

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();
            PersistentOutlet = BuildOutlet();
        }

        [TearDown]
        public void TearDown()
        {
            PersistentOutlet.Dispose();
        }


        public static StreamOutlet BuildTestScopedOutlet()
        => BuildOutlet(streamType: TestScopeOutletType);

        public static StreamOutlet BuildOutlet
        (
            string streamName = PersistentOutletName,
            string streamType = PersistentOutletType
        )
        {
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            int processID = GetCurrentProcess().Id;
            var streamInfo = new StreamInfo
            (
                streamName, streamType,
                channel_format: channel_format_t.cf_string,
                source_id: $"{deviceID}-Unity-{processID}-{CurrentTestName}"
            );
            return new StreamOutlet(streamInfo);
        }
    }
}