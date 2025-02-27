using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using LSL;

using static BCIEssentials.LSLFramework.LSLStreamResolver;

namespace BCIEssentials.Tests
{
    public class LSLStreamReaderTests: LSLOutletTestRunner
    {
        [Test]
        public void OpenStream_WhenOutletExists_ThenConnects()
        {
            var inStream = BuildAndOpenStreamReader(PersistentOutletType);
            AssertConnected(inStream);
            Destroy(inStream);
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
            var inStream = BuildAndOpenTestSpecificStreamReader();
            yield return new WaitForSecondsRealtime(0.15f);

            AssertNotConnected(inStream);
            var outlet = BuildTestSpecificOutlet();
            yield return new WaitForSecondsRealtime(0.1f);

            AssertConnected(inStream);
            outlet.Dispose();
            Destroy(inStream);
        }

        [Test]
        public void CloseStream_WhenConnected_ThenDisconnects()
        {
            var inStream = BuildAndOpenStreamReader(PersistentOutletType);
            AssertConnected(inStream);
            inStream.CloseStream();
            AssertNotConnected(inStream);
            Destroy(inStream);
        }

        [Test]
        public void WhenSamplePushed_ThenSamplesAvailable()
        => TestStreamReaderWithPushedSample
        (
            "ping", inStream => {
                Assert.AreEqual(1, inStream.SamplesAvailable);
            }
        );

        [Test]
        public void PullResponses_WhenSamplePushed_ThenSamplePulled()
        => TestStreamReaderWithPushedSample
        (
            "test", inStream =>
            {
                var responses = inStream.PullAllResponses();
                Assert.AreEqual(1, responses.Length);
                Assert.AreEqual("test", responses[0].RawSampleValues[0]);
            }
        );

        [Test]
        public void PullResponses_WhenPredictionSamplePushed_ThenParsedPredictionPulled()
        => TestStreamReaderWithPushedSample
        (
            "1", inStream => {
                var responses = inStream.PullAllResponses();
                Assert.AreEqual(1, responses.Length);
                var response = responses[0];
                Assert.IsInstanceOf<LSLPredictionResponse>(response);
                Assert.AreEqual(1, (response as LSLPredictionResponse).Value);
            }
        );


        private IEnumerator TestStreamReaderWithPushedSample
        (
            string sampleString, Action<LSLStreamReader> testMethod
        )
        {
            yield return new WaitForEndOfFrame();
            
            var outlet = BuildTestSpecificOutlet();
            var inStream = BuildAndOpenTestSpecificStreamReader();
            
            AssertConnected(inStream);
            outlet.push_sample(new[] {sampleString});

            testMethod(inStream);

            outlet.Dispose();
            Destroy(inStream);
        }


        private LSLStreamReader BuildAndOpenTestSpecificStreamReader()
        => BuildAndOpenStreamReader(TestSpecificOutletType);
        
        private LSLStreamReader BuildAndOpenStreamReader
        (
            string streamType = PersistentOutletType
        )
        {
            var inStream = AddComponent<LSLStreamReader>();
            inStream.StreamType = streamType;
            inStream.OpenStream();
            return inStream;
        }

        private void AssertConnected(LSLStreamReader inStream)
        => Assert.IsTrue(inStream.HasLiveInlet);
        private void AssertNotConnected(LSLStreamReader inStream)
        => Assert.IsFalse(inStream.HasLiveInlet);
    }
}