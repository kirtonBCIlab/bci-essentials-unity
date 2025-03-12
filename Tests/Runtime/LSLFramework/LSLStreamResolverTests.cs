using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using LSL;
using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

using static BCIEssentials.LSLFramework.LSLStreamResolver;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLStreamResolverTests: LSLOutletTestRunner
    {
        [Test]
        public void TryResolveByType_WhenOutletAvailable_ThenReturnsTrue()
        => Assert.IsTrue(TryResolveByType(OutletType, out _));

        [Test]
        public void TryResolveByName_WhenOutletAvailable_ThenReturnsTrue()
        => Assert.IsTrue(TryResolveByName(OutletName, out _));


        [Test]
        public void TryResolveByType_WhenOutletUnavailable_ThenReturnsFalse()
        => Assert.IsFalse(TryResolveByType("Invalid Stream Type", out _));

        [Test]
        public void TryResolveByName_WhenOutletUnavailable_ThenReturnsFalse()
        => Assert.IsFalse(TryResolveByName("Invalid Stream Name", out _));


        [UnityTest]
        public IEnumerator RunResolveByType_WhenOutletBecomesAvailable_ThenResolves()
        {
            StreamInfo resolvedStreamInfo = null;
            string streamType = OutletType + "-Delayed";

            AddCoroutineRunner(
                RunResolveByType(
                    streamType,
                    streamInfo => resolvedStreamInfo = streamInfo,
                    0.02f
                )
            ).StartRun();
            yield return new WaitForSecondsRealtime(0.05f);

            Assert.IsNull(resolvedStreamInfo);
            StreamOutlet outlet = BuildTypedOutlet(streamType);
            yield return new WaitForSecondsRealtime(0.05f);

            Assert.IsNotNull(resolvedStreamInfo);
            outlet.Dispose();
        }
    }
}