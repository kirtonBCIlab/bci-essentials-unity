using System;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class LSLMarkerWriterTests: LSLStreamWriterTestRunner
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
        => TestStreamWriteAgainstPulledSample<LSLMarkerWriter>
        (
            outStream => {
                var marker = (ICommandMarker)Activator.CreateInstance(markerType);
                outStream.PushMarker(marker);
            }, expectedSampleValue
        );

        [Test]
        [TestCase(2, 1.5f, 1, "mi,2,1,1.50")]
        [TestCase(2, 1.5f, -1, "mi,2,-1,1.50")]
        [TestCase(2, 2.5f, 4, "mi,2,-1,2.50")]
        [TestCase(2, 2.5f, -2, "mi,2,-1,2.50")]
        public void PushMIMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, float windowLength,
            int trainingTarget, string expectedSampleValue
        )
        => TestStreamWriteAgainstPulledSample
        (
            (LSLMarkerWriter outStream) => outStream.PushMIMarker
            (
                objectCount, windowLength, trainingTarget
            ), expectedSampleValue
        );
    }
}