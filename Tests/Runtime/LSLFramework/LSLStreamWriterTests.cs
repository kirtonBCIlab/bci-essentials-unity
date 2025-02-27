using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;

using static BCIEssentials.LSLFramework.LSLStreamResolver;
using LSL;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLStreamWriterTests: PlayModeTestRunnerBase
    {
        [Test]
        public void WhenStreamWriterCreated_ThenOpened()
        => TestStreamWriter();

        [Test]
        public void CloseStream_WhenStreamOpen_ThenClosed()
        => TestStreamWriter(outStream => {
            outStream.CloseStream();
            AssertNotConnectable(outStream);
        });

        [Test]
        public void PushString_WhenStringPushed_ThenSamplePulled()
        => TestStreamWriter(outStream => {
            TryResolveByName(outStream.StreamName, out var streamInfo);
            StreamInlet inlet = new(streamInfo);
            inlet.open_stream(0);
            
            string testMarkerString = "test";
            outStream.PushString(testMarkerString);
            Assert.AreEqual(1, inlet.samples_available());

            var sampleBuffer = new string[1];
            inlet.pull_sample(sampleBuffer, 0);
            Assert.AreEqual(testMarkerString, sampleBuffer[0]);

            inlet.Dispose();
        });


        public IEnumerator TestStreamWriter
        (
            Action<LSLStreamWriter> testMethod = null
        )
        {
            var outStream = BuildTestSpecificStreamWriter();
            yield return new WaitForEndOfFrame();

            AssertConnectable(outStream);
            testMethod?.Invoke(outStream);
            
            Destroy(outStream);
        }

        private LSLStreamWriter BuildTestSpecificStreamWriter()
        {
            return AddComponent<LSLStreamWriter> (
                outStream => {
                    outStream.StreamName = $"UnityTestingOutletFor:{CurrentTestName}";
                    outStream.StreamType = "StreamWriterTestMarkers";
                }
            );
        }

        private void AssertConnectable(LSLStreamWriter outStream)
        => Assert.IsTrue(TryResolveByName(outStream.StreamName, out _));
        private void AssertNotConnectable(LSLStreamWriter outStream)
        => Assert.IsFalse(TryResolveByName(outStream.StreamName, out _));
    }
}