using UnityEngine;
using LSL;
using BCIEssentials.LSLFramework;
using NUnit.Framework;

using static BCIEssentials.LSLFramework.LSLStreamResolver;

namespace BCIEssentials.Tests.Utilities.LSLFramework
{
    public class LSLStreamWriterTestRunner<T>:
        PlayModeTestRunnerBase where T: LSLStreamWriter
    {
        protected T _outStream;
        private StreamInlet _inlet;

        [SetUp]
        public virtual void Setup()
        {
            _outStream = BuildTestSpecificStreamWriter();
            _inlet = ConnectTestInlet();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Destroy(_outStream);
            _inlet.Dispose();
        }

        public void AssertPulledSample(string expectedSampleValue)
        {
            // updates some state in Unity or otherwise
            // delays execution long enough for the sample to get through lsl
            Debug.Log($"Testing for marker: {expectedSampleValue}");

            var sampleBuffer = new string[1];
            _inlet.pull_sample(sampleBuffer, 0);
            Assert.AreEqual(expectedSampleValue, sampleBuffer[0]);
        }


        protected T BuildTestSpecificStreamWriter()
        => AddComponent<T> (
            outStream => {
                outStream.StreamName = $"UnityTestingOutletFor:{CurrentTestName}";
                outStream.StreamType = "StreamWriterTestMarkers";
                outStream.OpenStream();
            }
        );

        protected StreamInlet ConnectTestInlet()
        {
            var resolvedStreams = LSL.LSL.resolve_stream("name", _outStream.StreamName, 1, 1);
            Assert.NotZero(resolvedStreams.Length);
            
            StreamInlet inlet = new(resolvedStreams[0]);
            inlet.open_stream(0.1);
            return inlet;
        }

        protected void AssertConnectable()
        => Assert.IsTrue(TryResolveByName(_outStream.StreamName, out _));
        protected void AssertNotConnectable()
        => Assert.IsFalse(TryResolveByName(_outStream.StreamName, out _));
    }
}