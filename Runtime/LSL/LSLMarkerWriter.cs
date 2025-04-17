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

        /// <summary>
        /// Create and send a formatted event marker for the Motor Imagery paradigm
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        public void PushMIMarker
        (
            int objectCount, float epochLength,
            int trainingTarget = -1
        )
        => PushMarker(
            new MIEventMarker
            (
                objectCount, epochLength, trainingTarget
            )
        );


        /// <summary>
        /// Create and send a formatted event marker for the Switch paradigm
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        public void PushSwitchMarker
        (
            int objectCount, float epochLength,
            int trainingTarget = -1
        )
        => PushMarker(
            new SwitchEventMarker
            (
                objectCount, epochLength, trainingTarget
            )
        );


        /// <summary>
        /// Create and send a formatted event marker for the SSVEP paradigm
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (frequencies) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        public void PushSSVEPMarker
        (
            int objectCount, float epochLength,
            IEnumerable<float> frequencies,
            int trainingTarget = -1
        )
        => PushMarker(
            new SSVEPEventMarker
            (
                objectCount, epochLength,
                frequencies, trainingTarget
            )
        );

        /// <summary>
        /// Create and send a formatted event marker for the TVEP paradigm
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (frequencies) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        public void PushTVEPMarker
        (
            int objectCount, float epochLength,
            IEnumerable<float> frequencies,
            int trainingTarget = -1
        )
        => PushMarker(
            new TVEPEventMarker
            (
                objectCount, epochLength,
                frequencies, trainingTarget
            )
        );

        /// <summary>
        /// Create and send a formatted single flash event marker for the P300 paradigm
        /// </summary>
        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="activeObject">
        /// Index of object being flashed <i>(0-indexed)</i>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object being targetted for training <i>(0-indexed)</i>
        /// </param>
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

        /// <summary>
        /// Create and send a formatted multi-flash event marker for the P300 paradigm
        /// </summary>
        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="activeObjects">
        /// Collection of object indices being flashed together <i>(0-indexed)</i>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object targetted for training <i>(0-indexed)</i>
        /// </param>
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