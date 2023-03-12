using System;
using System.Collections;
using BCIEssentials.LSL;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using Tests.Resources.Scripts;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests
{
    public class LSLServiceProviderTests : PlayModeTestRunnerBase
    {
        private LSLServiceProvider _testProvider;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();
            _testProvider = AddComponent<LSLServiceProvider>();
        }

        [Test]
        public void WhenGetMarkerReceiver_ThenReturnsMarker()
        {
            InitializeTestStream("astreamname");
            var markerReceiver = AddComponent<LSLMarkerReceiver>().InitializeWithStreamName("astreamname");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            var retrievedMarker = _testProvider.GetMarkerReceiver(markerReceiver.Id);

            UnityEngine.Assertions.Assert.AreEqual(retrievedMarker, markerReceiver);
        }

        [Test]
        public void WhenGetMarkerReceiverAndMarkerReceiverDestroyed_ThenReturnsNull()
        {
            InitializeTestStream("astreamname");
            var markerReceiver = AddComponent<LSLMarkerReceiver>().InitializeWithStreamName("astreamname");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            Object.DestroyImmediate(markerReceiver);

            var registeredMarker = _testProvider.GetMarkerReceiver("astreamname");
            Assert.IsNull(registeredMarker); //object null vs Unity Object null
        }

        [Test]
        public void WhenTryGetMarkerByNameAndHasRegistered_ThenReturnsMarkerReceiver()
        {
            InitializeTestStream("astreamname");
            var markerReceiver = AddComponent<LSLMarkerReceiver>().InitializeWithStreamName("astreamname");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            var markerFound = _testProvider.TryGetMarkerReceiverByStreamName("astreamname", out var registeredMarker);

            Assert.True(markerFound);
            UnityEngine.Assertions.Assert.AreEqual(markerFound, registeredMarker);
        }

        [Test]
        public void WhenHasRegisteredAndMultipleGetRequests_ThenReturnsSingleMarkerReceiver()
        {
            InitializeTestStream("astreamname");
            var markerReceiver = AddComponent<LSLMarkerReceiver>().InitializeWithStreamName("astreamname");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            _testProvider.TryGetMarkerReceiverByStreamName("astreamname", out var markerRequestA);
            _testProvider.TryGetMarkerReceiverByStreamName("astreamname", out var markerRequestB);

            UnityEngine.Assertions.Assert.AreEqual(markerRequestA, markerRequestB);
        }

        [Test]
        public void WhenTryGetMarkerByNameAndHasStream_ThenReturnsMarkerReceiver()
        {
            var streamName = "aname";
            InitializeTestStream(streamName);

            var markerFound = _testProvider.TryGetMarkerReceiverByStreamName(streamName, out var registeredMarker);

            Assert.True(markerFound);
            Assert.NotNull(registeredMarker);
            Assert.AreEqual(streamName, registeredMarker.StreamInfo.name());
        }

        [Test]
        public void WhenGetMarkerReceiverByNameAndMarkerReceiverDestroyed_ThenReturnsNull()
        {
            InitializeTestStream("astreamname");
            var markerReceiver = AddComponent<LSLMarkerReceiver>().InitializeWithStreamName("astreamname");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            Object.DestroyImmediate(markerReceiver);

            _testProvider.TryGetMarkerReceiverByStreamName("aname", out var registeredMarker);
            Assert.IsNull(registeredMarker); //object null vs Unity Object null
        }


        [Test]
        public void WhenTryGetMarkerByStreamIdAndHasRegistered_ThenReturnsMarkerReceiver()
        {
            InitializeTestStream("astreamname", "anid");
            var markerReceiver = AddComponent<LSLMarkerReceiver>().InitializeWithStreamName("astreamname");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            var markerFound = _testProvider.TryGetMarkerReceiverByStreamId("anid", out var registeredMarker);

            Assert.True(markerFound);
            UnityEngine.Assertions.Assert.AreEqual(markerFound, registeredMarker);
        }

        [Test]
        public void WhenTryGetMarkerByStreamIdAndHasStream_ThenReturnsMarkerReceiver()
        {
            var streamId = $"TestStream_{DateTime.Now.Ticks}";
            InitializeTestStream(id: streamId);

            var markerFound = _testProvider.TryGetMarkerReceiverByStreamId(streamId, out var registeredMarker);

            Assert.True(markerFound);
            Assert.NotNull(registeredMarker);
            Assert.AreEqual(streamId, registeredMarker.StreamInfo.source_id());
        }

        [Test]
        public void WhenTryGetMarkerReceiverByStreamIdAndMarkerReceiverDestroyed_ThenReturnsNull()
        {
            InitializeTestStream("astreamname");
            var markerReceiver = AddComponent<LSLMarkerReceiver>().InitializeWithStreamName("astreamname");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            Object.DestroyImmediate(markerReceiver);

            _testProvider.TryGetMarkerReceiverByStreamId("anid", out var registeredMarker);
            Assert.IsNull(registeredMarker); //object null vs Unity Object null
        }

        [Test]
        public void WhenInitializeMarkerReceiver_ThenInitializedWithSettings()
        {
            InitializeTestStream("aname");
            var testSettings = new LSLMarkerReceiverSettings
            {
                PollingFrequency = 555,
            };
            SetField(_testProvider, "_responseStreamSettings", testSettings);

            _testProvider.TryGetMarkerReceiverByStreamName("aname", out var markerReceiver);
            
            Assert.AreEqual(testSettings.PollingFrequency, markerReceiver.PollingFrequency);
        }

        [Test]
        [TestCase("source_id='newid'", "astream", "newid")]
        [TestCase("type='anewtype'", "astream", "anid", "anewtype")]
        [TestCase("starts-with(source_id,'anidbut')", "astream", "anidbutlonger")]
        [TestCase("contains(source_id,'longer')", "astream", "anidbutlonger")]
        //For a full list of predicate options see:
        //https://en.wikipedia.org/w/index.php?title=XPath_1.0&oldid=474981951#Node_set_functions
        public void WhenTryFindStreamWithPredicateValues_ThenPredicateUsed(string predicateValue, string streamName = "astream", string streamId = "anid", string streamType = "atype")
        {
            InitializeTestStream("astream", "anid", "atype");
            var expectedReceiver = InitializeTestStream(streamName, streamId, streamType);
            SetField(_testProvider, "_additionalResolvePredicateValues", new[]{predicateValue});

            var exists = _testProvider.TryGetMarkerReceiverByStreamName("astream", out var foundReceiver);
            
            Assert.IsTrue(exists);
            Assert.AreEqual(expectedReceiver.StreamOutlet.info().uid(), foundReceiver.StreamInfo.uid());
        }

        private static LSLMarkerStream InitializeTestStream(string name = null, string id = null, string type = null)
        {
            var hasName = !string.IsNullOrEmpty(name);
            var hasId = !string.IsNullOrEmpty(id);
            var hasType = !string.IsNullOrEmpty(type);

            if (!hasName && !hasId && !hasType)
            {
                Assert.Fail();
            }
            
            var lslStreamOutlet = AddComponent<LSLMarkerStream>();

            if (hasName)
            {
                lslStreamOutlet.StreamName = name;
            }
            
            if (hasId)
            {
                lslStreamOutlet.StreamId = id;
            }
            
            if (hasType)
            {
                lslStreamOutlet.StreamType = type;
            }
            
            lslStreamOutlet.InitializeStream();

            Debug.Log($"Created marker stream with uid: {lslStreamOutlet.StreamOutlet.info().uid()}");
            return lslStreamOutlet;
        }
    }
}