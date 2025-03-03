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
        [TestCase(2, 1.5f, 1, "mi,2,1,1.50")]
        [TestCase(2, 1.5f, -1, "mi,2,-1,1.50")]
        [TestCase(2, 2.5f, 4, "mi,2,-1,2.50")]
        [TestCase(2, 2.5f, -2, "mi,2,-1,2.50")]
        public void PushMIMarker_WhenMarkerPushed_ThenPulledWithCorrectFormat
        (
            int objectCount, float windowLength,
            int trainingTarget, string expectedSampleValue
        )
        {
            OutStream.PushMIMarker(objectCount, windowLength, trainingTarget);
            AssertPulledSample(expectedSampleValue);
        }
    }
}