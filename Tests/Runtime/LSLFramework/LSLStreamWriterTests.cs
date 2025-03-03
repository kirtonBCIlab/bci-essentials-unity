using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLStreamWriterTests: LSLStreamWriterTestRunner<LSLStreamWriter>
    {
        [Test]
        public void WhenStreamWriterCreated_ThenOpened()
        {
            AssertConnectable();
        }

        [Test]
        public void CloseStream_WhenStreamOpen_ThenClosed()
        {
            _outStream.CloseStream();
            AssertNotConnectable();
        }

        [Test]
        public void PushString_WhenStringPushed_ThenSamplePulled()
        {
            _outStream.PushString("test");
            AssertPulledSample("test");
        }
    }
}