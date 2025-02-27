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
    }
}