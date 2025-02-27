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
        public IEnumerator TestStreamWriter<T>
        (
            Action<T> testMethod = null
        ) where T: LSLStreamWriter
        {
            var outStream = BuildTestSpecificStreamWriter<T>();
            yield return new WaitForEndOfFrame();

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
        => TestStreamWriter<T>(outStream => {
            TryResolveByName(outStream.StreamName, out var streamInfo);
            StreamInlet inlet = new(streamInfo);
            inlet.open_stream(0);

            testMethod?.Invoke(outStream, inlet);

            inlet.Dispose();
        });


        protected T BuildTestSpecificStreamWriter<T>()
        where T: LSLStreamWriter
        => AddComponent<T> (
            outStream => {
                outStream.StreamName = $"UnityTestingOutletFor:{CurrentTestName}";
                outStream.StreamType = "StreamWriterTestMarkers";
            }
        );

        protected void AssertConnectable(LSLStreamWriter outStream)
        => Assert.IsTrue(TryResolveByName(outStream.StreamName, out _));
        protected void AssertNotConnectable(LSLStreamWriter outStream)
        => Assert.IsFalse(TryResolveByName(outStream.StreamName, out _));
    }
}