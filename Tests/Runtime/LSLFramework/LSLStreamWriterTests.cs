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
            OutStream.CloseStream();
            AssertNotConnectable();
        }

        [Test]
        public void PushString_WhenStringPushed_ThenSamplePulled()
        {
            OutStream.PushString("test");
            AssertPulledSample("test");
        }
    }
}