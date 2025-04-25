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
        [TestCase(2, 1, 1.5f, "mi,2,2,1.50")]
        [TestCase(2, -1, 1.5f, "mi,2,-1,1.50")]
        [TestCase(2, 4, 2.5f, "mi,2,-1,2.50")]
        [TestCase(2, -2, 2.5f, "mi,2,-1,2.50")]
        public void PushMIMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int trainingTarget,
            float epochLength, string expectedSampleValue
        )
        {
            OutStream.PushMITrainingMarker
            (
                objectCount, trainingTarget, epochLength
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(2, 1, 1.5f, "switch,2,2,1.50")]
        public void PushSwitchMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int trainingTarget,
            float epochLength, string expectedSampleValue
        )
        {
            OutStream.PushSwitchTrainingMarker
            (
                objectCount, trainingTarget, epochLength
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(4, 2, 1.5f, new[] {12.5f,18.7f,24.4f,30.1f}, "ssvep,4,3,1.50,12.5,18.7,24.4,30.1")]
        public void PushSSVEPMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int trainingTarget, float epochLength,
            float[] frequencies, string expectedSampleValue
        )
        {
            OutStream.PushSSVEPTrainingMarker
            (
                objectCount, trainingTarget,
                epochLength, frequencies
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(6, 2, 1.5f, new[] {15f}, "tvep,6,3,1.50,15")]
        public void PushTVEPMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int trainingTarget, float epochLength,
            float[] frequencies, string expectedSampleValue
        )
        {
            OutStream.PushTVEPTrainingMarker
            (
                objectCount, trainingTarget,
                epochLength, frequencies
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(8, 3, 1, "p300,s,8,4,2")]
        public void PushSingleFlashP300Marker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int trainingTarget,
            int activeObject, string expectedSampleValue
        )
        {
            OutStream.PushSingleFlashP300TrainingMarker
            (
                objectCount, trainingTarget, activeObject
            );
            AssertPulledSample(expectedSampleValue);
        }

        [Test]
        [TestCase(8, 3, new[] {1,3,5,7}, "p300,m,8,4,2,4,6,8")]
        public void PushMultiFlashP300Marker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, int trainingTarget,
            int[] activeObjects, string expectedSampleValue
        )
        {
            OutStream.PushMultiFlashP300TrainingMarker
            (
                objectCount, trainingTarget, activeObjects
            );
            AssertPulledSample(expectedSampleValue);
        }
    }
}