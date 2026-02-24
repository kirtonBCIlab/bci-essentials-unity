using System.Collections.Generic;
using System.Linq;

namespace BCIEssentials.LSLFramework
{
    public interface IMarker
    {
        public string MarkerString {get;}
    }


    public interface IStatusMarker: IMarker {}

    public struct TrialStartedMarker: IStatusMarker
    {
        public string MarkerString => "Trial Started";
    }
    public struct TrialEndsMarker: IStatusMarker 
    {
        public string MarkerString => "Trial Ends";
    }
    public struct TrainingCompleteMarker: IStatusMarker
    {
        public string MarkerString => "Training Complete";
    }
    public struct TrainClassifierMarker: IStatusMarker
    {
        public string MarkerString => "Train Classifier";
    }
    public struct UpdateClassifierMarker: IStatusMarker
    {
        public string MarkerString => "Update Classifier";
    }
    public struct DoneWithRestingStateCollectionMarker: IStatusMarker
    {
        public string MarkerString => "Done with all RS collection";
    }


    public abstract class EventMarker: IMarker
    {
        /// <summary>
        /// Number of classes or stimulus presenters in the trial
        /// </summary>
        public int CaseCount;
        /// <summary>
        /// Index of object, frequency, or class targetted for selection <i>(0-indexed)</i>
        /// </summary>
        public int TrainingTargetIndex;

        public virtual string MarkerString
        => $"{CaseCount},{(TrainingTargetIndex < 0? -1: TrainingTargetIndex + 1)}";

        public EventMarker
        (
            int caseCount, int trainingTargetIndex
        )
        {
            CaseCount = caseCount;
            TrainingTargetIndex = trainingTargetIndex;
            if (TrainingTargetIndex > caseCount || trainingTargetIndex < 0)
                TrainingTargetIndex = -1;
        }
    }


    public abstract class EpochEventMarker: EventMarker
    {
        /// <summary>
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant for training</b>
        /// </summary>
        public float EpochLength;

        public override string MarkerString
        => $"{base.MarkerString},{EpochLength.ToString("f2")}";

        public EpochEventMarker
        (
            int caseCount, int trainingTargetIndex,
            float epochLength
        ): base(caseCount, trainingTargetIndex)
        {
            EpochLength = epochLength;
        }
    }

    /// <summary>
    /// Motor Imagery event marker in the format:
    /// <br/><br/>
    /// "mi,{object count},{train target (-1 if n/a)},{epoch length}"
    /// </summary>
    public class MIEventMarker: EpochEventMarker
    {
        public override string MarkerString
        => $"mi,{base.MarkerString}";

        /// <param name="caseCount">
        /// Number of target states
        /// </param>
        /// <param name="trainingTargetIndex">
        /// Index of state targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant for training</b>
        /// </param>
        public MIEventMarker
        (
            int caseCount, int trainingTargetIndex,
            float epochLength
        )
        : base(caseCount, trainingTargetIndex, epochLength)
        {}
    }


    /// <summary>
    /// SSVEP event marker in the format:
    /// <br/><br/>
    /// "ssvep,{object count},{train target (-1 if n/a)},{epoch length},{...frequencies}"
    /// </summary>
    public class SSVEPEventMarker: EpochEventMarker
    {
        /// <summary>
        /// Flashing frequencies used by stimulus objects
        /// </summary>
        public float[] Frequencies;

        public override string MarkerString
        => $"ssvep,{base.MarkerString}{FrequenciesString}";

        protected string FrequenciesString
        => Frequencies switch {
            null or {Length: 0} => "",
            _ => $",{string.Join(",", Frequencies)}"
        };

        /// <param name="caseCount">
        /// Number of classes (frequencies) in the trial
        /// </param>
        /// <param name="trainingTargetIndex">
        /// Index of class (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="epochLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant for training</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        public SSVEPEventMarker
        (
            int caseCount, int trainingTargetIndex,
            float epochLength,  IEnumerable<float> frequencies
        ): base(caseCount, trainingTargetIndex, epochLength)
        {
            Frequencies = frequencies.ToArray();
        }
    }


    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,[sm],{object count},{train target (-1 if n/a)}..."
    /// <br/>
    /// <i>where 's' or 'm' indicates single or multi flash</i>
    /// </summary>
    public abstract class P300EventMarker: EventMarker
    {
        public P300EventMarker
        (
            int caseCount, int trainingTargetIndex
        ): base(caseCount, trainingTargetIndex)
        {}
    }

    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,s,{object count},{train target (-1 if n/a)},{active object}"
    /// </summary>
    public class SingleFlashP300EventMarker: P300EventMarker
    {
        /// <summary>
        /// Index of stimulus presenter triggered
        /// </summary>
        public int StimulusIndex;

        public override string MarkerString
        => $"p300,s,{base.MarkerString},{StimulusIndex + 1}";

        /// <param name="caseCount">Number of objects in the trial</param>
        /// <param name="stimulusIndex">
        /// Index of stimulus presenter triggered <i>(0-indexed)</i>
        /// </param>
        /// <param name="trainingTargetIndex">
        /// Index of stimulus presenter targetted for training <i>(0-indexed)</i>
        /// </param>
        public SingleFlashP300EventMarker
        (
            int caseCount, int trainingTargetIndex,
            int stimulusIndex
        ): base(caseCount, trainingTargetIndex)
        {
            StimulusIndex = stimulusIndex;
        }
    }

    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,m,{object count},{train target (-1 if n/a)},{...active objects}"
    /// </summary>
    public class MultiFlashP300EventMarker: P300EventMarker
    {
        /// <summary>
        /// Collection of stimulus presenter indices triggered
        /// </summary>
        public int[] StimulusIndices;

        public override string MarkerString
        => $"p300,m,{base.MarkerString}{StimulusIndicesString}";

        protected string StimulusIndicesString
        => StimulusIndices switch {
            null or {Length: 0} => "",
            _ => $",{string.Join(',', StimulusIndices.Select(x => ++x))}"
        };

        /// <param name="caseCount">Number of objects in the trial</param>
        /// <param name="trainingTargetIndex">
        /// Index of stimulus presenter targetted for training <i>(0-indexed)</i>
        /// </param>
        /// <param name="stimulusIndices">
        /// Collection of stimulus presenter indices triggered <i>(0-indexed)</i>
        /// </param>
        public MultiFlashP300EventMarker
        (
            int caseCount, int trainingTargetIndex,
            IEnumerable<int> stimulusIndices
        )
        : base(caseCount, trainingTargetIndex)
        {
            StimulusIndices = stimulusIndices.ToArray();
        }
    }
}