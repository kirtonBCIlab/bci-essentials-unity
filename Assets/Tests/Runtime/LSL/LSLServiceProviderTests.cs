using System.Collections;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.TestResources;
using BCIEssentials.Tests.Utilities;
using LSL;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LogAssert = BCIEssentials.Tests.TestResources.LogAssert;
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
        public void WhenRegisterMarkerReceiver_ThenRegistered()
        {
            CreateMarkerStream("astreamname");
            var markerReceiver = CreateMarkerReceiver("name='astreamname'");
            
            var wasRegistered = _testProvider.RegisterMarkerReceiver(markerReceiver);
            
            Assert.True(wasRegistered);
        }

        [Test]
        public void WhenRegisterMarkerReceiverAndAlreadyRegistered_ThenNotRegistered()
        {
            CreateMarkerStream("astreamname");
            var markerReceiverA = CreateMarkerReceiver("name='astreamname'");
            var markerReceiverB = CreateMarkerReceiver("name='astreamname'");
            
            _testProvider.RegisterMarkerReceiver(markerReceiverA);
            LogAssert.ExpectAnyContains(LogType.Error, "already registered");
            
            var wasRegistered = _testProvider.RegisterMarkerReceiver(markerReceiverB);
            
            Assert.False(wasRegistered);
        }

        [Test]
        public void WhenRegisterMarkerReceiverAndAlreadyRegisteredIsNull_ThenRegistered()
        {
            CreateMarkerStream("astreamname");
            var markerReceiverA = CreateMarkerReceiver("name='astreamname'");
            var markerReceiverB = CreateMarkerReceiver("name='astreamname'");
            _testProvider.RegisterMarkerReceiver(markerReceiverA);
            Object.DestroyImmediate(markerReceiverA.gameObject);
            
            var wasRegistered = _testProvider.RegisterMarkerReceiver(markerReceiverB);
            
            Assert.True(wasRegistered);
        }
        
        [Test]
        public void WhenGetMarkerReceiverByUIDAndRegistered_ThenReturnsRegisteredMarker()
        {
            CreateMarkerStream("astreamname");
            var markerReceiver = CreateMarkerReceiver("name='astreamname'");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            var retrievedMarker = _testProvider.GetMarkerReceiverByUID(markerReceiver.UID);

            UnityEngine.Assertions.Assert.AreEqual(retrievedMarker, markerReceiver);
        }
        
        [Test]
        public void WhenGetMarkerReceiverByUIDAndHasStream_ThenReturnsCreatedMarker()
        {
            var streamUID = CreateMarkerStream("astreamname").StreamOutlet.info().uid();
            
            var retrievedMarker = _testProvider.GetMarkerReceiverByUID(streamUID);

            Assert.NotNull(retrievedMarker);
            Assert.AreEqual(retrievedMarker.UID, streamUID);
        }
        
        [Test]
        public void WhenGetMarkerReceiverBySourceId_ThenReturnsMarker()
        {
            CreateMarkerStream(id:"asourceid");
            var markerReceiver = CreateMarkerReceiver("source_id='asourceid'");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            var retrievedMarker = _testProvider.GetMarkerReceiverBySourceId("asourceid");

            UnityEngine.Assertions.Assert.AreEqual(retrievedMarker, markerReceiver);
        }
        
        /// <summary>
        /// See <a href="https://en.wikipedia.org/wiki/XPath">XPath 1.0</a> for predicate formatting.
        /// </summary>
        [Test]
        [TestCase("type='anewtype'", "astream", "anid", "anewtype")]
        [TestCase("starts-with(source_id,'anidbut')", "astream", "anidbutlonger")]
        [TestCase("contains(source_id,'longer')", "astream", "anidbutlonger")]
        public void WhenGetMarkerReceiverByPredicate_ThenReturnsMarker(string predicateValue, string streamName = "astream", string streamId = "anid", string streamType = "atype")
        {
            CreateMarkerStream(streamName, streamId, streamType);
            var expectedReceiver = CreateMarkerReceiver($"name='{streamName}'");
            _testProvider.RegisterMarkerReceiver(expectedReceiver);

            var foundReceiver = _testProvider.GetMarkerReceiverByPredicate(predicateValue);
            
            Assert.IsNotNull(foundReceiver);
            UnityEngine.Assertions.Assert.AreEqual(expectedReceiver, foundReceiver);
        }

        [Test]
        public void WhenServiceCreatesMarkerReceiver_ThenMarkerCreatedWithSettings()
        {
            var markerUID = CreateMarkerStream("astreamname").StreamUID;
            var testSettings = new LSLMarkerReceiverSettings
            {
                PollingFrequency = 555,
            };
            ReflectionHelpers.SetField(_testProvider, "_responseStreamSettings", testSettings);

            var markerReceiver = _testProvider.GetMarkerReceiverByUID(markerUID);
            
            Assert.AreEqual(testSettings.PollingFrequency, markerReceiver.PollingFrequency);
        }

        [Test]
        public void WhenMultipleGetRequestsForSameMarker_ThenReturnsSingleMarkerReceiver()
        {
            CreateMarkerStream("astreamname");
            var markerReceiver = CreateMarkerReceiver("name='astreamname'");
            _testProvider.RegisterMarkerReceiver(markerReceiver);

            var markerReceiverA = _testProvider.GetMarkerReceiverByPredicate("name='astreamname'");
            var markerReceiverB = _testProvider.GetMarkerReceiverByPredicate("name='astreamname'");

            UnityEngine.Assertions.Assert.AreEqual(markerReceiverA, markerReceiverB);
        }
        
        private static LSLMarkerStream CreateMarkerStream(string name = null, string id = null, string type = null)
        {
            var hasName = !string.IsNullOrEmpty(name);
            var hasId = !string.IsNullOrEmpty(id);
            var hasType = !string.IsNullOrEmpty(type);

            if (!hasName && !hasId && !hasType)
            {
                Assert.Fail("No identifying values provided.");
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

            Debug.Log($"Created marker stream with uid: {lslStreamOutlet.StreamUID}");
            return lslStreamOutlet;
        }
        
        private static LSLMarkerReceiver CreateMarkerReceiver(string predicate)
        {
            var resolvedStreams = LSL.LSL.resolve_stream(predicate, 0, 0);
            if (resolvedStreams.Length == 0)
            {
                Assert.Fail($"No stream found for predicate: {predicate}");
            }

            var streamInfo = resolvedStreams[0];
            
            if (streamInfo == null)
            {
                Assert.Fail("Failed to create Marker Receiver");
                return null;
            }

            var receiver = AddComponent<LSLMarkerReceiver>()
                .Initialize(streamInfo, null);
            
            resolvedStreams.DisposeArray();
            
            Debug.Log($"Created marker receiver with uid: {receiver.UID}");
            return receiver;
        }
    }
}