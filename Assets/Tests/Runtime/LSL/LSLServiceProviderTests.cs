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

namespace BCIEssentials.Tests.LSLService
{
    public class LSLServiceProviderTests : PlayModeTestRunnerBase
    {
        private const string k_TestStreamName = "_astreamname";
        
        private LSLServiceProvider _testServiceProvider;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();
            _testServiceProvider = AddComponent<LSLServiceProvider>();
        }

        [Test]
        public void WhenRegisterMarkerReceiver_ThenRegistered()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiver = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            
            var wasRegistered = _testServiceProvider.RegisterMarkerReceiver(markerReceiver);
            
            Assert.True(wasRegistered);
        }

        [Test]
        public void WhenRegisterMarkerReceiverAndAlreadyRegistered_ThenNotRegistered()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiverA = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            var markerReceiverB = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            
            _testServiceProvider.RegisterMarkerReceiver(markerReceiverA);
            LogAssert.ExpectAnyContains(LogType.Error, "already registered");
            
            var wasRegistered = _testServiceProvider.RegisterMarkerReceiver(markerReceiverB);
            
            Assert.False(wasRegistered);
        }

        [Test]
        public void WhenRegisterMarkerReceiverAndAlreadyRegisteredIsNull_ThenRegistered()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiverA = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            var markerReceiverB = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            _testServiceProvider.RegisterMarkerReceiver(markerReceiverA);
            Object.DestroyImmediate(markerReceiverA.gameObject);
            
            var wasRegistered = _testServiceProvider.RegisterMarkerReceiver(markerReceiverB);
            
            Assert.True(wasRegistered);
        }
        
        [Test]
        public void WhenGetMarkerReceiverByUIDAndRegistered_ThenReturnsRegisteredMarker()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiver = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            _testServiceProvider.RegisterMarkerReceiver(markerReceiver);

            var retrievedMarker = _testServiceProvider.GetMarkerReceiverByUID(markerReceiver.UID);

            UnityEngine.Assertions.Assert.AreEqual(retrievedMarker, markerReceiver);
        }
        
        [Test]
        public void WhenGetMarkerReceiverByUIDAndHasStream_ThenReturnsCreatedMarker()
        {
            var streamUID = CreateMarkerStream(k_TestStreamName).StreamUID;
            
            var retrievedMarker = _testServiceProvider.GetMarkerReceiverByUID(streamUID);

            Assert.NotNull(retrievedMarker);
            Assert.AreEqual(retrievedMarker.UID, streamUID);
        }

        [Test]
        public void WhenHasRegisteredMarkerReceiverAndHasRegistered_ThenReturnsTrue()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiver = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            _testServiceProvider.RegisterMarkerReceiver(markerReceiver);

            bool isRegistered = _testServiceProvider.HasRegisteredMarkerReceiver(markerReceiver);
            Assert.True(isRegistered);
        }
        
        [Test]
        public void WhenHasRegisteredMarkerReceiverAndHasNoneRegistered_ThenReturnsFalse()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiver = CreateMarkerReceiver($"name='{k_TestStreamName}'");

            bool isRegistered = _testServiceProvider.HasRegisteredMarkerReceiver(markerReceiver);
            Assert.False(isRegistered);
        }

        [Test]
        public void WhenGetMarkerReceiverByName_ThenReturnsMarker()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiver = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            _testServiceProvider.RegisterMarkerReceiver(markerReceiver);

            var retrievedMarker = _testServiceProvider.GetMarkerReceiverByName(k_TestStreamName);

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
            _testServiceProvider.RegisterMarkerReceiver(expectedReceiver);

            var foundReceiver = _testServiceProvider.GetMarkerReceiverByPredicate(predicateValue);
            
            Assert.IsNotNull(foundReceiver);
            UnityEngine.Assertions.Assert.AreEqual(expectedReceiver, foundReceiver);
        }

        [Test]
        public void WhenServiceCreatesMarkerReceiver_ThenMarkerCreatedWithSettings()
        {
            var markerUID = CreateMarkerStream(k_TestStreamName).StreamUID;
            var testSettings = new LSLMarkerReceiverSettings
            {
                PollingFrequency = 555,
            };
            ReflectionHelpers.SetField(_testServiceProvider, "_responseStreamSettings", testSettings);

            var markerReceiver = _testServiceProvider.GetMarkerReceiverByUID(markerUID);
            
            Assert.AreEqual(testSettings.PollingFrequency, markerReceiver.PollingFrequency);
        }

        [Test]
        public void WhenMultipleGetRequestsForSameMarker_ThenReturnsSingleMarkerReceiver()
        {
            CreateMarkerStream(k_TestStreamName);
            var markerReceiver = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            _testServiceProvider.RegisterMarkerReceiver(markerReceiver);

            var markerReceiverA = _testServiceProvider.GetMarkerReceiverByPredicate($"name='{k_TestStreamName}'");
            var markerReceiverB = _testServiceProvider.GetMarkerReceiverByPredicate($"name='{k_TestStreamName}'");

            UnityEngine.Assertions.Assert.AreEqual(markerReceiverA, markerReceiverB);
        }

        [Test]
        public void WhenUnregisterMarkerReceiver_ThenUnregistered()
        {
            var markerStream = CreateMarkerStream(k_TestStreamName);
            var markerReceiver = CreateMarkerReceiver($"name='{k_TestStreamName}'");
            _testServiceProvider.RegisterMarkerReceiver(markerReceiver);
            
            markerStream.EndStream(); //Close stream so a new marker receiver is not created by the service provider
            _testServiceProvider.UnregisterMarkerReceiver(markerReceiver);

            var registeredReceiver = _testServiceProvider.GetMarkerReceiverByUID(markerReceiver.UID);
            Assert.IsNull(registeredReceiver);
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