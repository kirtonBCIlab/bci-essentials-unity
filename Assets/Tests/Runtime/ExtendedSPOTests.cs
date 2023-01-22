using System.Collections;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests
{
    internal class ExtendedSPOTests : PlayModeTestRunnerBase
    {
        private ExtendedSPO _testSpo;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _testSpo = new GameObject().AddComponent<ExtendedSPO>();
        }

        [Test]
        public void WhenTurnOn_ThenScaleIncreased()
        {
            _testSpo.transform.localScale = new Vector3(5, 5, 5);
            var expectedScale = Vector3.one * 5 * 1.4f; //1.4 is a magic number used by the SPO

            _testSpo.StartStimulus();

            Assert.AreEqual(expectedScale, _testSpo.transform.localScale);
        }

        [Test]
        public void WhenTurnOff_ThenScaleDecreased()
        {
            _testSpo.transform.localScale = new Vector3(5, 5, 5);
            var expectedScale = Vector3.one * 5 / 1.4f; //1.4 is a magic number used by the SPO

            _testSpo.StopStimulus();

            Assert.AreEqual(expectedScale, _testSpo.transform.localScale);
        }

        [UnityTest]
        public IEnumerator WhenOnSelected_ThenGameObjectDestroyed()
        {
            _testSpo.Select();
            yield return new WaitForEndOfFrame();
            
            UnityEngine.Assertions.Assert.IsNull(_testSpo);
        }
    }
}