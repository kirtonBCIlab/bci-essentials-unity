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
        /// Create and send a training marker for the Motor Imagery paradigm
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
        public void PushMITrainingMarker
        (
            int objectCount,
            int trainingTarget,
            float epochLength
        )
        => PushMarker(new MIEventMarker
            (objectCount, trainingTarget, epochLength)
        );

        /// <summary>
        /// Create and send a classification marker for the Motor Imagery paradigm
        /// <br/><b>Will trigger a prediction in <see cref="epochLength"/> seconds</b>
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Arbitrary length of the processing Epoch <br/>
        /// </param>
        public void PushMIClassificationMarker
        (
            int objectCount, float epochLength
        )
        => PushMarker(new MIEventMarker
            (objectCount, -1, epochLength)
        );


        /// <summary>
        /// Create and send a training marker for the Switch paradigm
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        public void PushSwitchTrainingMarker
        (
            int objectCount,
            int trainingTarget,
            float epochLength
        )
        => PushMarker(new SwitchEventMarker
            (objectCount, trainingTarget, epochLength)
        );

        /// <summary>
        /// Create and send a classification marker for the Switch paradigm
        /// <br/><b>Will trigger a prediction in <see cref="epochLength"/> seconds</b>
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Arbitrary length of the processing Epoch <br/>
        /// </param>
        public void PushSwitchClassificationMarker
        (
            int objectCount, float epochLength
        )
        => PushMarker(new SwitchEventMarker
            (objectCount, -1, epochLength)
        );


        /// <summary>
        /// Create and send a training marker for the SSVEP paradigm
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (frequencies) in the trial
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        public void PushSSVEPTrainingMarker
        (
            int objectCount,
            int trainingTarget,
            float epochLength,
            IEnumerable<float> frequencies
        )
        => PushMarker(new SSVEPEventMarker
            (objectCount, trainingTarget, epochLength, frequencies)
        );

        /// <summary>
        /// Create and send a classification marker for the SSVEP paradigm
        /// <br/><b>Will trigger a prediction in <see cref="epochLength"/> seconds</b>
        /// <br/>or at the end of the trial
        /// <br/>depending on python configuration
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Arbitrary length of the processing Epoch <br/>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        public void PushSSVEPClassificationMarker
        (
            int objectCount, float epochLength,
            IEnumerable<float> frequencies
        )
        => PushMarker(new SSVEPEventMarker
            (objectCount, -1, epochLength, frequencies)
        );

        /// <summary>
        /// Create and send a training marker for the TVEP paradigm
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (frequencies) in the trial
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        public void PushTVEPTrainingMarker
        (
            int objectCount,
            int trainingTarget,
            float epochLength,
            IEnumerable<float> frequencies
        )
        => PushMarker(new TVEPEventMarker
            (objectCount, trainingTarget, epochLength, frequencies)
        );

        /// <summary>
        /// Create and send a classification marker for the TVEP paradigm
        /// <br/><b>Will trigger a prediction in <see cref="epochLength"/> seconds</b>
        /// <br/>or at the end of the trial
        /// <br/>depending on python configuration
        /// </summary>
        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="epochLength">
        /// Arbitrary length of the processing Epoch <br/>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        public void PushTVEPClassificationMarker
        (
            int objectCount, float epochLength,
            IEnumerable<float> frequencies
        )
        => PushMarker(new TVEPEventMarker
            (objectCount, -1, epochLength, frequencies)
        );


        /// <summary>
        /// Create and send a single flash training marker for the P300 paradigm
        /// </summary>
        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="trainingTarget">
        /// Index of object being targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="activeObject">
        /// Index of object being flashed <i>(0-indexed)</i>
        /// </param>
        public void PushSingleFlashP300TrainingMarker
        (
            int objectCount,
            int trainingTarget,
            int activeObject
        )
        => PushMarker(new SingleFlashP300EventMarker
            (objectCount, trainingTarget, activeObject)
        );

        /// <summary>
        /// Create and send a single flash classification marker for the P300 paradigm
        /// <br/><b>Will trigger a prediction at the end of the trial</b>
        /// </summary>
        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="activeObject">
        /// Index of object being flashed <i>(0-indexed)</i>
        /// </param>
        public void PushSingleFlashP300ClassificationMarker
        (
            int objectCount, int activeObject
        )
        => PushMarker(new SingleFlashP300EventMarker
            (objectCount, -1, activeObject)
        );

        /// <summary>
        /// Create and send a multi-flash training marker for the P300 paradigm
        /// </summary>
        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="trainingTarget">
        /// Index of object targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="activeObjects">
        /// Collection of object indices being flashed together <i>(0-indexed)</i>
        /// </param>
        public void PushMultiFlashP300TrainingMarker
        (
            int objectCount,
            int trainingTarget,
            IEnumerable<int> activeObjects
        )
        => PushMarker(new MultiFlashP300EventMarker
            (objectCount, trainingTarget, activeObjects)
        );

        /// <summary>
        /// Create and send a multi-flash classification marker for the P300 paradigm
        /// <br/><b>Will trigger a prediction at the end of the trial</b>
        /// </summary>
        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="activeObjects">
        /// Collection of object indices being flashed together <i>(0-indexed)</i>
        /// </param>
        public void PushMultiFlashP300ClassificationMarker
        (
            int objectCount, IEnumerable<int> activeObjects
        )
        => PushMarker(new MultiFlashP300EventMarker
            (objectCount, -1, activeObjects)
        );


        public void PushMarker(ILSLMarker marker)
            => PushString(marker.MarkerString);
    }
}