using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLResponseProviderTests: LSLOutletTestRunner
    {
        ResponseProvider InStream;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            InStream = new()
            {
                StreamType = OutletType,
                PollingPeriod = 0.02f
            };
        }

        [TearDown]
        public override void TearDown()
        {
            InStream.CloseStream();
            base.TearDown();
        }


        [Test]
        public void WhenCreated_ThenNotPolling()
        {
            Assert.IsFalse(InStream.IsPolling);
        }

        [Test]
        public void Subscribe_WhenNotPolling_ThenStartsPolling()
        {
            Assert.IsFalse(InStream.IsPolling);
            InStream.SubscribeAll(r => {});
            Assert.IsTrue(InStream.IsPolling);
        }

        [Test]
        public void Unsubscribe_WhenPolling_ThenNotPolling()
        {
            static void dummySubscriber(Response r) { }
            InStream.SubscribeAll(dummySubscriber);
            Assert.IsTrue(InStream.IsPolling);
            InStream.UnsubscribeAll(dummySubscriber);
            Assert.IsFalse(InStream.IsPolling);
        }

        [UnityTest]
        public IEnumerator Subscribe_WhenSubscribedAndPushMarker_ThenResponseProvided()
        {
            Response response = null;
            InStream.SubscribeAll(r => response = r);
            PushStringThroughOutlet("ping");
            yield return new WaitForSecondsRealtime(0.05f);
            Assert.NotNull(response);
        }

        [UnityTest]
        public IEnumerator Subscribe_WhenSubscribePredictions_ThenOnlyPredictionsReceived()
        {
            Prediction prediction = null;
            InStream.SubscribePredictions(p => prediction = p);

            PushStringThroughOutlet("ping");
            PushStringThroughOutlet("2:[0.39 0.61]");
            PushStringThroughOutlet("ping");

            yield return new WaitForSecondsRealtime(0.05f);
            Assert.NotNull(prediction);
            Assert.AreEqual(1, prediction.Index);
        }

        [UnityTest]
        public IEnumerator Subscribe_WhenSubscribeByType_ThenResponsesFiltered()
        {
            int responseCount = 0;
            int predictionCount = 0;
            int pingCount = 0;

            InStream.SubscribeAll(_ => responseCount++);
            InStream.SubscribePredictions(_ => predictionCount++);
            InStream.Subscribe<BCIEssentials.LSLFramework.Ping>(_ => pingCount++);

            PushStringThroughOutlet("ping");
            PushStringThroughOutlet("2:[0.39 0.61]");
            PushStringThroughOutlet("ping");

            yield return new WaitForSecondsRealtime(0.05f);
            Assert.AreEqual(3, responseCount);
            Assert.AreEqual(1, predictionCount);
            Assert.AreEqual(2, pingCount);
        }

        [UnityTest]
        public IEnumerator Subscribe_WhenSubscriberDestroyed_ThenPruned()
        {
            var unitySubscriber = AddComponent<DummySubscriber>();
            var responseCount = 0;

            InStream.SubscribeAll(unitySubscriber.ReceiveLSLResponse);
            InStream.SubscribeAll(_ => responseCount++);

            PushStringThroughOutlet("ping");

            yield return new WaitForSecondsRealtime(0.05f);
            Assert.AreEqual(1, unitySubscriber.responseCount);
            Assert.AreEqual(1, responseCount);

            Destroy(unitySubscriber);
            PushStringThroughOutlet("ping");

            yield return new WaitForSecondsRealtime(0.05f);
            Assert.AreEqual(2, responseCount);
        }

        [Test]
        public void PullAllResponses_WhenResponsesPulledManually_ThenSubscribersNotified()
        {
            InStream.PollingPeriod = 10;

            BCIEssentials.LSLFramework.Ping ping = null;
            InStream.Subscribe<BCIEssentials.LSLFramework.Ping>(p => ping = p);

            PushStringThroughOutlet("ping");
            InStream.PullAllResponses();

            Assert.NotNull(ping);
        }


        private class DummySubscriber: MonoBehaviour
        {
            public int responseCount = 0;

            public void ReceiveLSLResponse(Response r)
            {
                responseCount++;
            }
        }
    }
}