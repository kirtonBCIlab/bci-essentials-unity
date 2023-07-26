using System;
using System.Collections;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities;
using LSL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests.LSLService
{
    public class LSLMarkerReceiverTests : PlayModeTestRunnerBase
    {
        private readonly LSLMarkerReceiverSettings _testSettings = new ()
        {
            PullSampleTimeout = 0D //Instant timeout so for synchronous tests with no stream
        };
        
       [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();
        }

        [TearDown]
        public void TestCleanup()
        {
            foreach (var openStream in LSL.LSL.resolve_streams())
            {
                openStream.Close();
            }
        }
        
        [Test]
        public void WhenInitializeAndNoStreamInfo_ThenThrows()
        {
            var markerReceiver = AddComponent<LSLMarkerReceiver>();
            Assert.Throws<ArgumentNullException>(() =>
            {
                markerReceiver.Initialize(null);
            });
        }

        [Test]
        public void WhenInitializeWithInvalidStreamInfo_ThenThrows()
        {
            var markerReceiver = AddComponent<LSLMarkerReceiver>();

            Assert.Throws<TimeoutException>(() =>
            {
                var streamInfo = new StreamInfo("aname", "atype", 111);
                markerReceiver.Initialize(streamInfo);
            });
        }

        [Test]
        public void WhenInitialize_ThenInitialized()
        {
            NewStreamOutlet(out var streamId);
            var markerReceiver = NewMarkerReceiver(streamId);

            Assert.True(markerReceiver.Initialized);
            Assert.AreNotEqual(string.Empty, markerReceiver.UID);
            Assert.True(markerReceiver.Connected);
            Assert.False(markerReceiver.Polling);
            Assert.AreEqual(0, markerReceiver.PollingFrequency);
        }

        [Test]
        public void WhenInitializeWithSettings_ThenInitialized()
        {
            NewStreamOutlet(out var streamId);
            var settings = new LSLMarkerReceiverSettings
            {
                PollingFrequency = 111
            };

            var markerReceiver = NewMarkerReceiver(streamId, settings:settings);

            Assert.True(markerReceiver.Initialized);
            Assert.AreEqual(settings.PollingFrequency, markerReceiver.PollingFrequency);
        }

        [Test]
        public void WhenInitializeAnReinitializeMarkerReceiver_ThenNotReinitialized()
        {
            TestResources.LogAssert.ExpectStartingWith(LogType.Error, "Cannot initialize");
            NewStreamOutlet(out var streamId);
            var markerReceiver = NewMarkerReceiver(streamId);
            var oldUID = string.Copy(markerReceiver.UID);
            
            markerReceiver.Initialize(new StreamInfo("astreamname", "astreamtype"));
            
            Assert.AreEqual(oldUID, markerReceiver.UID);
        }

        [Test]
        public void WhenCleanUp_ThenUnitializedAndPollingStopped()
        {
            NewStreamOutlet(out var streamId);
            var receiver = NewMarkerReceiver(streamId);
            AddSubscriber(receiver);
            
            receiver.CleanUp();
            
            Assert.AreEqual(string.Empty, receiver.UID);
            Assert.Null(receiver.StreamInfo);
            Assert.False(receiver.Initialized);
            Assert.False(receiver.Connected);
            Assert.False(receiver.Polling);
            Assert.AreEqual(0, receiver.SubscriberCount);
        }

        [Test]
        public void WhenSubscribe_ThenSubscribedAndPollingStarted()
        {
            NewStreamOutlet(out var streamId);
            var receiver = NewMarkerReceiver(streamId);
            AddSubscriber(receiver);
            
            Assert.True(receiver.Polling);
            Assert.AreEqual(1, receiver.SubscriberCount);
        }

        [Test]
        public void WhenUnsubscribeAndLastSubscriber_ThenUnsubscribedAndPollingStopped()
        {
            NewStreamOutlet(out var streamId);
            var receiver = NewMarkerReceiver(streamId);
            var subscriber = AddSubscriber(receiver);
                
            receiver.Unsubscribe(subscriber);
            
            Assert.IsFalse(receiver.Polling);
            Assert.AreEqual(0, receiver.SubscriberCount);
        }

        [Test]
        public void WhenUnsubscribeAndStillSubscribers_ThenUnsubscribedAndStillPolling()
        {
            NewStreamOutlet(out var streamId);
            var receiver = NewMarkerReceiver(streamId);
            AddSubscriber(receiver);
            var subscriber = AddSubscriber(receiver);
                
            receiver.Unsubscribe(subscriber);
            
            Assert.IsTrue(receiver.Polling);
            Assert.AreEqual(1, receiver.SubscriberCount);
        }

        [UnityTest]
        public IEnumerator WhenPollingAndPullSample_ThenSubscribersNotified()
        {
            var outlet = NewStreamOutlet(out var streamId); //Create stream outlet first
            var receiver = NewMarkerReceiver(streamId);
            var subscriber = Substitute.For<ILSLMarkerSubscriber>();
            
            receiver.Subscribe(subscriber);
            yield return new WaitForSecondsRealtime(_testSettings.PollingFrequency);
            outlet.push_sample(new[] { "amarker" }); //Send Marker after polling starts
            yield return new WaitForSecondsRealtime(_testSettings.PollingFrequency);
            
            subscriber.Received(1)
                .NewMarkersCallback(Arg.Any<LSLMarkerResponse[]>());
        }

        [UnityTest]
        public IEnumerator WhenGetResponses_ThenResponsesRetrieved()
        {
            var marker = new[] { "marker" };
            var outlet = NewStreamOutlet(out var streamId); //Create stream outlet first
            var receiver = NewMarkerReceiver(streamId);
            
            outlet.push_sample(marker); //Push marker after receiver stream open
            yield return new WaitForEndOfFrame();
            var responses = receiver.GetResponses();

            Assert.AreEqual(1, responses.Length);
            Assert.AreEqual(marker, responses[0].Value);
        }

        [UnityTest]
        public IEnumerator WhenGetLatestResponses_ThenLastPulledResponsesRetrieved()
        {
            var outlet = NewStreamOutlet(out var streamId); //Create stream outlet first
            var receiver = NewMarkerReceiver(streamId);

            var markerCount = 1;
            for (int i = 0; i < 3; i++)
            {
                outlet.push_sample(new[] { "marker" + markerCount });
                yield return new WaitForEndOfFrame();
                receiver.GetResponses(); //populate last response
                markerCount++;
            }

            var lastMarker = new[] { "marker" + markerCount };
            outlet.push_sample(lastMarker);
            var latestResponses = receiver.GetLatestResponses();

            Assert.AreEqual(1, latestResponses.Length);
            Assert.AreEqual(lastMarker, latestResponses[0].Value);
        }

        //Locate using SourceId to avoid un-disposed streams from previous tests
        private LSLMarkerReceiver NewMarkerReceiver(string streamId, LSLMarkerReceiverSettings settings = null)
        {
            var resolvedStreams = LSL.LSL.resolve_stream($"source_id='{streamId}'", 0, 0);
            if (resolvedStreams.Length == 0)
            {
                Assert.Fail($"No stream found for id: {streamId}");
            }

            var streamInfo = resolvedStreams[0];
            
            if (streamInfo == null || streamInfo.IsClosed)
            {
                Assert.Fail("Failed to create Marker Receiver");
                return null;
            }
            
            var receiver = AddComponent<LSLMarkerReceiver>()
                .Initialize(streamInfo, settings ?? _testSettings);
            
            resolvedStreams.DisposeArray();
            
            Debug.Log($"Created marker receiver with uid: {receiver.UID}");
            return receiver;
        }
        
        //Create using SourceId to avoid un-disposed streams from previous tests
        private static StreamOutlet NewStreamOutlet(out string streamId, string streamName = "astreamname")
        {
            streamId = Guid.NewGuid().ToString();
            Debug.Log($"Creating StreamOutlet with id: {streamId}");
            
            var streamInfo = new StreamInfo(streamName, "astreamtype", 1, 0.0, channel_format_t.cf_string, streamId);
            return new StreamOutlet(streamInfo);
        }

        private ILSLMarkerSubscriber AddSubscriber(LSLMarkerReceiver receiver, Action<LSLMarkerResponse[]> onMarkersReceived = null)
        {
            var subscriber = Substitute.For<ILSLMarkerSubscriber>();

            if (onMarkersReceived != null)
            {
                subscriber
                    .WhenForAnyArgs(x => x.NewMarkersCallback(Arg.Any<LSLMarkerResponse[]>()))
                    .Do(arg =>
                    {
                        var markers = arg.Arg<LSLMarkerResponse[]>();
                        onMarkersReceived.Invoke(markers);
                    });
            }

            receiver.Subscribe(subscriber);

            return subscriber;
        }
    }
}