namespace BCIEssentials.Extensions
{
    using LSLFramework;

    public static class P300MarkerWriterExtensions
    {
        public static void SendSingleFlashMarker
        (
            this MarkerWriter markerWriter,
            int stimulusIndex, int presenterCount,
            int? trainingTarget = null
        )
        {
            if (trainingTarget.HasValue)
            {
                markerWriter.PushSingleFlashP300TrainingMarker
                (presenterCount, trainingTarget.Value, stimulusIndex);
            }
            else
            {
                markerWriter.PushSingleFlashP300ClassificationMarker
                (presenterCount, stimulusIndex);
            }
        }
        
        public static void SendMultiFlashMarker
        (
            this MarkerWriter markerWriter,
            int[] stimulusIndices, int presenterCount,
            int? trainingTarget = null
        )
        {
            if (trainingTarget.HasValue)
            {
                markerWriter.PushMultiFlashP300TrainingMarker
                (presenterCount, trainingTarget.Value, stimulusIndices);
            }
            else
            {
                markerWriter.PushMultiFlashP300ClassificationMarker
                (presenterCount, stimulusIndices);
            }
        }
    }
}