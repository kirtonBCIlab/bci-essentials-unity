using System.Collections;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests
{
    internal class MITrainingSPOTests : PlayModeTestRunnerBase
    {
        private MITrainingSPO _testSpo;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _testSpo = new GameObject().AddComponent<MITrainingSPO>();
        }

        [Test]
        public void WhenTurnOff_ThenOriginalPositionApplied()
        {
            _testSpo.transform.position = new Vector3(5, 5, 5);
            
            _testSpo.TurnOff();
            
            Assert.AreEqual(Vector3.zero, _testSpo.transform.position);
        }

        [Test]
        public void WhenTurnOn_ThenOriginalPositionSet()
        {
            var expectedPosition = new Vector3(5, 5, 5);
            _testSpo.transform.position = expectedPosition;

            _testSpo.TurnOn();
            _testSpo.transform.position = Vector3.zero;
            _testSpo.TurnOff();
            
            Assert.AreEqual(expectedPosition, _testSpo.transform.position);
        }
    }
}