using System;
using System.Collections.Generic;

namespace BCIEssentials.LSLFramework
{
    [Serializable]
    public class MarkerWriter : LSLStreamWriter
    {
        private float? _lastEpochLength;

        public void PushTrialStartedMarker()
            => PushStatusMarker<TrialStartedMarker>();
        public void PushTrialEndsMarker()
            => PushStatusMarker<TrialEndsMarker>();
        public void PushTrainingCompleteMarker()
            => PushStatusMarker<TrainingCompleteMarker>();
        public void PushTrainClassificationMarker()
            => PushStatusMarker<TrainClassifierMarker>();
        public void PushUpdateClassifierMarker()
            => PushStatusMarker<UpdateClassifierMarker>();
        public void PushDoneWithRestingStateCollectionMarker()
            => PushStatusMarker<DoneWithRestingStateCollectionMarker>();
        public void PushStatusMarker<T>()
            where T : IStatusMarker, new()
            => PushMarker(new T());


        /// <summary>
        /// Create and send a training marker for the Motor Imagery paradigm
        /// </summary>
        /// <param name="stateCount">
        /// Number of target states
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant</b>
        /// </param>
        /// <param name="trainingTargetIndex">
        /// Index of state targetted for training <i>(0-indexed)</i>
        /// </param>
        public void PushMITrainingMarker
        (
            int stateCount,
            int trainingTargetIndex,
            float epochLength
        )
        => PushEpochMarker(new MIEventMarker
            (stateCount, trainingTargetIndex, epochLength)
        );

        /// <summary>
        /// Create and send a classification marker for the Motor Imagery paradigm
        /// </summary>
        /// <remarks>
        /// Will trigger a prediction in <paramref name="epochLength"/> seconds
        /// </remarks>
        /// <param name="stateCount">
        /// Number target states
        /// </param>
        /// <param name="epochLength">
        /// Arbitrary length of the processing Epoch <br/>
        /// Ideally at least 1-2 seconds
        /// </param>
        public void PushMIClassificationMarker
        (
            int stateCount, float epochLength
        )
        => PushEpochMarker(new MIEventMarker
            (stateCount, -1, epochLength)
        );


        /// <summary>
        /// Create and send a training marker for the SSVEP paradigm
        /// </summary>
        /// <param name="trainingTargetIndex">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus presenters <br/>
        /// </param>
        public void PushSSVEPTrainingMarker
        (
            int trainingTargetIndex,
            float epochLength,
            IEnumerable<float> frequencies
        )
        => PushEpochMarker(new SSVEPEventMarker
            (trainingTargetIndex, epochLength, frequencies)
        );

        /// <summary>
        /// Create and send a classification marker for the SSVEP paradigm
        /// </summary>
        /// <remarks>
        /// Will trigger a prediction in <paramref name="epochLength"/> seconds
        /// <br/>or at the end of the trial
        /// <br/>depending on python configuration
        /// </remarks>
        /// <param name="epochLength">
        /// Arbitrary length of the processing Epoch <br/>
        /// Ideally at least 1-2 seconds
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus presenters
        /// </param>
        /// <exception cref="EpochLengthException">
        /// Thrown when a different Epoch Length is used
        /// </exception>
        public void PushSSVEPClassificationMarker
        (
            float epochLength,
            IEnumerable<float> frequencies
        )
        => PushEpochMarker(new SSVEPEventMarker
            (-1, epochLength, frequencies)
        );


        /// <summary>
        /// Create and send a single flash training marker for the P300 paradigm
        /// </summary>
        /// <param name="presenterCount">Number of presenters in the trial</param>
        /// <param name="trainingTargetIndex">
        /// Index of stimulus presenter targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="activeObject">
        /// Index of stimulus presenter triggered <i>(0-indexed)</i>
        /// </param>
        public void PushSingleFlashP300TrainingMarker
        (
            int presenterCount,
            int trainingTargetIndex,
            int activeObject
        )
        => PushMarker(new SingleFlashP300EventMarker
            (presenterCount, trainingTargetIndex, activeObject)
        );

        /// <summary>
        /// Create and send a single flash classification marker for the P300 paradigm
        /// </summary>
        /// <remarks>
        /// Will trigger a prediction at the end of the trial
        /// </remarks>
        /// <param name="presenterCount">Number of presenters in the trial</param>
        /// <param name="stimulusIndex">
        /// Index of stimulus presenter triggered <i>(0-indexed)</i>
        /// </param>
        public void PushSingleFlashP300ClassificationMarker
        (
            int presenterCount, int stimulusIndex
        )
        => PushMarker(new SingleFlashP300EventMarker
            (presenterCount, -1, stimulusIndex)
        );

        /// <summary>
        /// Create and send a multi-flash training marker for the P300 paradigm
        /// </summary>
        /// <param name="presenterCount">Number of presenters in the trial</param>
        /// <param name="trainingTargetIndex">
        /// Index of stimulus presenter targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="stimulusIndices">
        /// Collection of stimulus presenter indices triggered <i>(0-indexed)</i>
        /// </param>
        public void PushMultiFlashP300TrainingMarker
        (
            int presenterCount,
            int trainingTargetIndex,
            IEnumerable<int> stimulusIndices
        )
        => PushMarker(new MultiFlashP300EventMarker
            (presenterCount, trainingTargetIndex, stimulusIndices)
        );

        /// <summary>
        /// Create and send a multi-flash classification marker for the P300 paradigm
        /// </summary>
        /// <remarks>
        /// Will trigger a prediction at the end of the trial
        /// </remarks>
        /// <param name="presenterCount">Number of presenters in the trial</param>
        /// <param name="stimulusIndices">
        /// Collection of stimulus presenter indices triggered <i>(0-indexed)</i>
        /// </param>
        public void PushMultiFlashP300ClassificationMarker
        (
            int presenterCount, IEnumerable<int> stimulusIndices
        )
        => PushMarker(new MultiFlashP300EventMarker
            (presenterCount, -1, stimulusIndices)
        );


        public virtual void PushMarker(IMarker marker)
            => PushString(marker.MarkerString);


        public void PushEpochMarker(EpochEventMarker marker)
        {
            if (!_lastEpochLength.HasValue)
            {
                _lastEpochLength = marker.EpochLength;
            }
            else if (_lastEpochLength.Value != marker.EpochLength)
            {
                throw new EpochLengthException(_lastEpochLength.Value);
            }
            PushMarker(marker);
        }
    }
}