using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLStreamWriterTests: LSLStreamWriterTestRunner
    {
        [Test]
        public void WhenStreamWriterCreated_ThenOpened()
        => TestStreamWriter();

        [Test]
        public void CloseStream_WhenStreamOpen_ThenClosed()
        => TestStreamWriter
        (
            outStream => {
                outStream.CloseStream();
                AssertNotConnectable(outStream);
            }
        );

        [Test]
        public void PushString_WhenStringPushed_ThenSamplePulled()
        => TestStreamWriteAgainstPulledSample
        (
            outStream => outStream.PushString("test"), "test"
        );
    }
}