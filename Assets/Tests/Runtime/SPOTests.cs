using System.Collections;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests
{
    internal class SPOTests : PlayModeTestRunnerBase
    {
        private SPO _testSpo;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _testSpo = new GameObject().AddComponent<SPO>();
        }

        [Test]
        public void WhenStartStimulus_ThenReturnsTime()
        {
            var expectedResult = Time.time;

            var result = _testSpo.StartStimulus();

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void WhenStartStimulus_ThenEventInvoked()
        {
            var eventCalled = false;
            _testSpo.StartStimulusEvent.AddListener(() =>
            {
                eventCalled = true;
            });

            _testSpo.StartStimulus();

            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void WhenSelect_ThenEventInvoked()
        {
            var eventCalled = false;
            _testSpo.OnSelectedEvent.AddListener(() =>
            {
                eventCalled = true;
            });

            _testSpo.Select();

            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void WhenStopStimulus_ThenEventInvoked()
        {
            var eventCalled = false;
            _testSpo.StopStimulusEvent.AddListener(() =>
            {
                eventCalled = true;
            });

            _testSpo.StopStimulus();

            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void WhenOnTrainTarget_ThenLocalScaleIncreased()
        {
            _testSpo.transform.localScale = new Vector3(5, 5, 5);
            var expectedScale = Vector3.one * 5 * 1.4f; //1.4 is a magic number used by the SPO

            _testSpo.OnTrainTarget();

            Assert.AreEqual(expectedScale, _testSpo.transform.localScale);
        }

        [Test]
        public void WhenOnTrainTarget_ThenLocalScaleDecreased()
        {
            _testSpo.transform.localScale = new Vector3(5, 5, 5);
            var expectedScale = Vector3.one * 5 / 1.4f; //1.4 is a magic number used by the SPO

            _testSpo.OffTrainTarget();

            Assert.AreEqual(expectedScale, _testSpo.transform.localScale);
        }
    }
}