using System.Collections.Generic;

namespace BCIEssentials.LSLFramework
{
    public class LSLMarkerWriter: LSLStreamWriter
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
            int objectCount, float windowLength,
            int trainingTarget = -1
        )
        => PushMarker(
            new MIEventMarker
            (
                objectCount, windowLength, trainingTarget
            )
        );

        public void PushSwitchMarker
        (
            int objectCount, float windowLength,
            int trainingTarget = -1
        )
        => PushMarker(
            new SwitchEventMarker
            (
                objectCount, windowLength, trainingTarget
            )
        );

        public void PushSSVEPMarker
        (
            int objectCount, float windowLength,
            IEnumerable<float> frequencies,
            int trainingTarget = -1
        )
        => PushMarker(
            new SSVEPEventMarker
            (
                objectCount, windowLength,
                frequencies, trainingTarget
            )
        );

        public void PushTVEPMarker
        (
            int objectCount, float windowLength,
            IEnumerable<float> frequencies,
            int trainingTarget = -1
        )
        => PushMarker(
            new TVEPEventMarker
            (
                objectCount, windowLength,
                frequencies, trainingTarget
            )
        );

        public void PushSingleFlashP300Marker
        (
            int objectCount, int activeObject,
            int trainingTarget = -1
        )
        => PushMarker(
            new SingleFlashP300EventMarker
            (
                objectCount, activeObject,
                trainingTarget
            )
        );

        public void PushMultiFlashP300Marker
        (
            int objectCount, IEnumerable<int> activeObjects,
            int trainingTarget = -1
        )
        => PushMarker(
            new MultiFlashP300EventMarker
            (
                objectCount, activeObjects, trainingTarget
            )
        );

        public void PushMarker(ILSLMarker marker)
            => PushString(marker.MarkerString);
    }
}