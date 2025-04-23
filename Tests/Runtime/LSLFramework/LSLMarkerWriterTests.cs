using System;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLMarkerWriterTests: LSLStreamWriterTestRunner<LSLMarkerWriter>
    {
        [Test]
        [TestCase(typeof(TrialStartedMarker), "Trial Started")]
        [TestCase(typeof(TrialEndsMarker), "Trial Ends")]
        [TestCase(typeof(TrainingCompleteMarker), "Training Complete")]
        [TestCase(typeof(UpdateClassifierMarker), "Update Classifier")]
        public void PushMarker_WhenTypedCommandMarkerPushed_ThenMarkerStringPulled
        (
            Type markerType, string expectedSampleValue
        )
        {
            var marker = (ICommandMarker)Activator.CreateInstance(markerType);
            OutStream.PushMarker(marker);
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(2, 1.5f, 1, "mi,2,2,1.50")]
        [TestCase(2, 1.5f, -1, "mi,2,-1,1.50")]
        [TestCase(2, 2.5f, 4, "mi,2,-1,2.50")]
        [TestCase(2, 2.5f, -2, "mi,2,-1,2.50")]
        public void PushMIMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, float windowLength,
            int trainingTarget, string expectedSampleValue
        )
        {
            OutStream.PushMIMarker
            (
                objectCount, windowLength, trainingTarget
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(2, 1.5f, 1, "switch,2,2,1.50")]
        public void PushSwitchMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, float windowLength,
            int trainingTarget, string expectedSampleValue
        )
        {
            OutStream.PushSwitchMarker
            (
                objectCount, windowLength, trainingTarget
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(4, 1.5f, new[] {12.5f,18.7f,24.4f,30.1f}, 2, "ssvep,4,3,1.50,12.5,18.7,24.4,30.1")]
        public void PushSSVEPMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, float windowLength, float[] frequencies,
            int trainingTarget, string expectedSampleValue
        )
        {
            OutStream.PushSSVEPMarker
            (
                objectCount, windowLength,
                frequencies, trainingTarget
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(6, 1.5f, new[] {15f}, 2, "tvep,6,3,1.50,15")]
        public void PushTVEPMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, float windowLength, float[] frequencies,
            int trainingTarget, string expectedSampleValue
        )
        {
            OutStream.PushTVEPMarker
            (
                objectCount, windowLength,
                frequencies, trainingTarget
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(8, 1, 3, "p300,s,8,4,2")]
        public void PushSingleFlashP300Marker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int activeObject,
            int trainingTarget, string expectedSampleValue
        )
        {
            OutStream.PushSingleFlashP300Marker
            (
                objectCount, activeObject, trainingTarget
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(8, new[] {1,3,5,7}, 3, "p300,m,8,4,2,4,6,8")]
        public void PushMultiFlashP300Marker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int[] activeObjects,
            int trainingTarget, string expectedSampleValue
        )
        {
            OutStream.PushMultiFlashP300Marker
            (
                objectCount, activeObjects, trainingTarget
            );
            AssertPulledSample(expectedSampleValue);
        }
    }
}