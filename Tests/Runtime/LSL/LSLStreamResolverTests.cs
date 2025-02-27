using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using LSL;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;

using static BCIEssentials.LSLFramework.LSLStreamResolver;

namespace BCIEssentials.Tests
{
    public class LSLStreamResolverTests: LSLOutletTestRunner
    {
        [Test]
        public void TryResolveByType_WhenOutletAvailable_ThenReturnsTrue()
        => Assert.IsTrue(TryResolveByType(PersistentOutletType, out _));

        [Test]
        public void TryResolveByName_WhenOutletAvailable_ThenReturnsTrue()
        => Assert.IsTrue(TryResolveByName(PersistentOutletName, out _));


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

            AddCoroutineRunner(
                RunResolveByType(
                    TestSpecificOutletType,
                    streamInfo => resolvedStreamInfo = streamInfo,
                    0.02f
                )
            ).StartRun();
            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsNull(resolvedStreamInfo);
            StreamOutlet outlet = BuildTestSpecificOutlet();
            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsNotNull(resolvedStreamInfo);
            outlet.Dispose();
        }
    }
}