using System;
using UnityEngine;
using UnityEngine.TestTools;
using LSL;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;

using static BCIEssentials.LSLFramework.LSLStreamResolver;
using static System.Diagnostics.Process;
using System.Collections;

namespace BCIEssentials.Tests
{
    public class LSLStreamResolverTests: PlayModeTestRunnerBase
    {
        private StreamOutlet _testOutlet;
        private const string OutletName = "Unity Testing Outlet";
        private const string OutletType = "Test Markers";


        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();
            _testOutlet = BuildOutlet(OutletName, OutletType);
        }

        [TearDown]
        public void TearDown()
        {
            _testOutlet.Dispose();
        }


        [Test]
        public void TryResolveByType_WhenOutletAvailable_ThenReturnsTrue()
        => Assert.IsTrue(TryResolveByType(OutletType, out _));

        [Test]
        public void TryResolveByName_WhenOutletAvailable_ThenReturnsTrue()
        => Assert.IsTrue(TryResolveByName(OutletName, out _));


        [Test]
        public void TryResolveByType_WhenOutletUnavailable_ThenReturnsFalse()
        => Assert.IsFalse(TryResolveByType("Invalid Stream Type", out _));

        [Test]
        public void TryResolveByName_WhenOutletUnavailable_ThenReturnsFalse()
        => Assert.IsFalse(TryResolveByName("Invalid Stream Name", out _));


        [UnityTest]
        public IEnumerator RunResolveByType_WhenOutletBecomesAvailable_ThenConnectsInlet()
        {
            string outletType = "Delayed Test Markers";
            StreamInfo resolvedStreamInfo = null;

            AddCoroutineRunner(
                RunResolveByType(
                    outletType,
                    streamInfo => resolvedStreamInfo = streamInfo,
                    0.02f
                )
            ).StartRun();
            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsNull(resolvedStreamInfo);
            StreamOutlet outlet = BuildOutlet(streamType: outletType);
            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsNotNull(resolvedStreamInfo);
            outlet.Dispose();
        }


        private StreamOutlet BuildOutlet
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