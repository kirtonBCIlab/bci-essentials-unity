using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLStreamReaderTests: LSLOutletTestRunner
    {
        LSLStreamReader InStream;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            InStream = BuildAndOpenStreamReader(OutletType);
        }

        [TearDown]
        public override void TearDown()
        {
            Destroy(InStream);
            base.TearDown();
        }


        [Test]
        public void OpenStream_WhenOutletExists_ThenConnects()
        {
            AssertConnected(InStream);
        }

        [Test]
        public void OpenStream_WhenNoOutletExists_ThenNotConnected()
        {
            var inStream = BuildAndOpenStreamReader("Invalid Stream Type");
            AssertNotConnected(inStream);
            Destroy(inStream);
        }

        [UnityTest]
        public IEnumerator OpenStream_WhenOutletBecomesAvailable_ThenConnects()
        {
            string streamType = OutletType + "-Delayed";
            var inStream = BuildAndOpenStreamReader(streamType);
            yield return new WaitForSecondsRealtime(0.05f);

            AssertNotConnected(inStream);
            var outlet = BuildTypedOutlet(streamType);
            yield return new WaitForSecondsRealtime(0.05f);

            AssertConnected(inStream);
            outlet.Dispose();
            Destroy(inStream);
        }

        [Test]
        public void CloseStream_WhenConnected_ThenDisconnects()
        {
            AssertConnected(InStream);
            InStream.CloseStream();
            AssertNotConnected(InStream);
        }

        [Test]
        public void WhenSamplePushed_ThenSamplesAvailable()
        {
            PushStringThroughOutlet("ping");
            Assert.AreEqual(1, InStream.SamplesAvailable);
        }

        [Test]
        public void PullResponses_WhenSamplePushed_ThenSamplePulled()
        {
            PushStringThroughOutlet("test");
            var responses = InStream.PullAllResponses();
            Assert.AreEqual(1, responses.Length);
            Assert.AreEqual("test", responses[0].RawSampleValues[0]);
        }

        [Test]
        public void PullResponses_WhenPredictionSamplePushed_ThenParsedPredictionPulled()
        {
            PushStringThroughOutlet("1");
            var responses = InStream.PullAllResponses();
            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOf<LSLPredictionResponse>(responses[0]);
            var prediction = responses[0] as LSLPredictionResponse;
            Assert.AreEqual(1, prediction.Value);
        }

        
        private LSLStreamReader BuildAndOpenStreamReader
        (
            string streamType
        )
        {
            var inStream = AddComponent<LSLStreamReader>();
            inStream.StreamType = streamType;
            inStream.OpenStream(0.02f);
            return inStream;
        }

        private void AssertConnected(LSLStreamReader inStream)
        => Assert.IsTrue(inStream.HasLiveInlet);
        private void AssertNotConnected(LSLStreamReader inStream)
        => Assert.IsFalse(inStream.HasLiveInlet);
    }
}