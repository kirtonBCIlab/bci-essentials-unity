namespace BCIEssentials.LSLFramework
{
    public class LSLMarkerStreamWriter: LSLStreamWriter
    {
        public void PushTrialStartedMarker()
            => PushCommandMarker<TrialStartedMarker>();
        public void PushTrialEndsMarker()
            => PushCommandMarker<TrialEndsMarker>();
        public void PushTrainingCompleteMarker()
            => PushCommandMarker<TrainingCompleteMarker>();
        public void PushUpdateClassifierMarker()
            => PushCommandMarker<UpdateClassifierMarker>();
        public void PushCommandMarker<T>()
            where T: ICommandMarker, new()
            => PushMarker(new T());

        public void PushMIMarker
        (
            int spoCount, float windowLength,
            int trainingTarget = -1
        )
        => PushMarker(
            new MIEventMarker
            (
                spoCount, windowLength, trainingTarget
            )
        );

        public void PushSSVEPMarker
        (
            int spoCount, float windowLength,
            float[] frequencies,
            int trainingTarget = -1
        )
        => PushMarker(
            new SSVEPEventMarker
            (
                spoCount, windowLength, 
                frequencies, trainingTarget
            )
        );

        public void PushSingleFlashP300EventMarker
        (
            int spoCount, int activeSPO,
            int trainingTarget = -1
        )
        => PushMarker(
            new SingleFlashP300EventMarker
            (
                spoCount, activeSPO,
                trainingTarget
            )
        );

        public void PushMultiFlashP300EventMarker
        (
            int spoCount, int[] activeSPOs,
            int trainingTarget = -1
        )
        => PushMarker(
            new MultiFlashP300EventMarker
            (
                spoCount, activeSPOs,
                trainingTarget
            )
        );

        public void PushMarker(ILSLMarker marker)
            => PushString(marker.MarkerString);
    }
}