using System;
using System.Collections;
using UnityEngine;
using LSL;
using BCIEssentials.LSLFramework;
using NUnit.Framework;

using static BCIEssentials.LSLFramework.LSLStreamResolver;

namespace BCIEssentials.Tests.Utilities.LSLFramework
{
    public class LSLStreamWriterTestRunner: PlayModeTestRunnerBase
    {
        public void TestStreamWriter
        (
            Action<LSLStreamWriter> testMethod = null
        )
        => TestStreamWriter<LSLStreamWriter>(testMethod);
        public void TestStreamWriter<T>
        (
            Action<T> testMethod = null
        ) where T: LSLStreamWriter
        {
            var outStream = BuildTestSpecificStreamWriter<T>();

            AssertConnectable(outStream);
            testMethod?.Invoke(outStream);
            
            Destroy(outStream);
        }

        public void TestStreamWriterWithInlet
        (
            Action<LSLStreamWriter, StreamInlet> testMethod = null
        )
        => TestStreamWriterWithInlet<LSLStreamWriter>(testMethod);
        public void TestStreamWriterWithInlet<T>
        (
            Action<T, StreamInlet> testMethod = null
        ) where T: LSLStreamWriter
        => TestStreamWriter<T>
        (
            outStream => {
                TryResolveByName(outStream.StreamName, out var streamInfo);
                StreamInlet inlet = new(streamInfo);
                inlet.open_stream(0);

                testMethod?.Invoke(outStream, inlet);

                inlet.Dispose();
            }
        );

        public void TestStreamWriteAgainstPulledSample
        (
            Action<LSLStreamWriter> pushMethod,
            string expectedSampleValue
        )
        => TestStreamWriteAgainstPulledSample<LSLStreamWriter>(pushMethod, expectedSampleValue);
        public void TestStreamWriteAgainstPulledSample<T>
        (
            Action<T> pushMethod, string expectedSampleValue
        ) where T: LSLStreamWriter
        => TestStreamWriterWithInlet<T>
        (
            (outStream, inlet) => {
                pushMethod(outStream);
                Assert.NotZero(inlet.samples_available());

                var sampleBuffer = new string[1];
                inlet.pull_sample(sampleBuffer, 0);
                Assert.AreEqual(expectedSampleValue, sampleBuffer[0]);
            }
        );


        protected T BuildTestSpecificStreamWriter<T>()
        where T: LSLStreamWriter
        => AddComponent<T> (
            outStream => {
                outStream.StreamName = $"UnityTestingOutletFor:{CurrentTestName}";
                outStream.StreamType = "StreamWriterTestMarkers";
                outStream.OpenStream();
            }
        );

        protected void AssertConnectable(LSLStreamWriter outStream)
        => Assert.IsTrue(TryResolveByName(outStream.StreamName, out _));
        protected void AssertNotConnectable(LSLStreamWriter outStream)
        => Assert.IsFalse(TryResolveByName(outStream.StreamName, out _));
    }
}