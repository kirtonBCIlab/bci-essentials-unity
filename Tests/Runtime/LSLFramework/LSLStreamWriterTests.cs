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
        => TestStreamWriter(outStream => {
            outStream.CloseStream();
            AssertNotConnectable(outStream);
        });

        [Test]
        public void PushString_WhenStringPushed_ThenSamplePulled()
        => TestStreamWriterWithInlet((outStream, inlet) => {
            string testMarkerString = "test";
            outStream.PushString(testMarkerString);
            Assert.AreEqual(1, inlet.samples_available());

            var sampleBuffer = new string[1];
            inlet.pull_sample(sampleBuffer, 0);
            Assert.AreEqual(testMarkerString, sampleBuffer[0]);
        });
    }
}