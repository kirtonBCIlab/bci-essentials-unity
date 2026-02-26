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


        const float resolutionDelay = 0.05f;
        static readonly WaitForSecondsRealtime waitForResolutionDelay = new(resolutionDelay);
        private class DummyBehaviour : MonoBehaviour { }

        [UnityTest]
        public IEnumerator RunResolveByType_WhenOutletBecomesAvailable_ThenResolves()
        {
            StreamInfo resolvedStreamInfo = null;
            string streamType = OutletType + "-Delayed";

            DummyBehaviour resolutionHost = AddComponent<DummyBehaviour>();
            resolutionHost.StartCoroutine(
                RunResolveByType(
                    streamType,
                    streamInfo => resolvedStreamInfo = streamInfo,
                    0.02f
                )
            );
            yield return waitForResolutionDelay;

            Assert.IsNull(resolvedStreamInfo);
            StreamOutlet outlet = BuildTypedOutlet(streamType);
            yield return waitForResolutionDelay;

            Assert.IsNotNull(resolvedStreamInfo);
            outlet.Dispose();
            Destroy(resolutionHost);
        }
    }
}