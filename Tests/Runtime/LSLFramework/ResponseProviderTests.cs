using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class ResponseProviderTests: LSLOutletTestRunner
    {
        protected virtual ResponseProvider InStream => _provider;
        ResponseProvider _provider;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _provider = new()
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

        protected virtual IEnumerator StepProvider()
        {
            _provider.ProcessResponseProvision();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Subscribe_WhenSubscribedAndPushMarker_ThenResponseProvided()
        {
            Response response = null;
            InStream.SubscribeAll(r => response = r);
            PushStringThroughOutlet("ping");
            yield return StepProvider();
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

            yield return StepProvider();
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

            yield return StepProvider();
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

            yield return StepProvider();
            Assert.AreEqual(1, unitySubscriber.responseCount);
            Assert.AreEqual(1, responseCount);

            Destroy(unitySubscriber);
            PushStringThroughOutlet("ping");

            yield return StepProvider();
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