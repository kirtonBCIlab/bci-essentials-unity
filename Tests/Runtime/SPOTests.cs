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
        public void WhenOnTrainTarget_ThenEventInvoked()
        {
            var eventCalled = false;
            _testSpo.StartTrainingStimulusEvent.AddListener(()=>
            {
                eventCalled = true;
            });

            _testSpo.OnTrainTarget();

            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void WhenOffTrainTarget_ThenEventInvoked()
        {
            var eventCalled = false;
            _testSpo.StopTrainingStimulusEvent.AddListener(()=>
            {
                eventCalled = true;
            });

            _testSpo.OffTrainTarget();

            Assert.IsTrue(eventCalled);
        }

    }
}