using System.Collections.Generic;
using System.Linq;

namespace BCIEssentials.LSLFramework
{
    public interface ILSLMarker
    {
        public string MarkerString {get;}
    }


    public interface ICommandMarker: ILSLMarker {}

    public struct TrialStartedMarker: ICommandMarker
    {
        public string MarkerString => "Trial Started";
    }
    public struct TrialEndsMarker: ICommandMarker 
    {
        public string MarkerString => "Trial Ends";
    }
    public struct TrainingCompleteMarker: ICommandMarker
    {
        public string MarkerString => "Training Complete";
    }
    public struct UpdateClassifierMarker: ICommandMarker
    {
        public string MarkerString => "Update Classifier";
    }


    public abstract class EventMarker: ILSLMarker
    {
        public int SpoCount;
        public int TrainingTarget;

        public virtual string MarkerString
        => $"{SpoCount},{TrainingTarget}";

        public EventMarker
        (
            int spoCount, int trainingTarget
        )
        {
            SpoCount = spoCount;
            TrainingTarget = trainingTarget;
            if (TrainingTarget > spoCount || trainingTarget < 0)
                TrainingTarget = -1;
        }
    }


    public abstract class WindowedEventMarker: EventMarker
    {
        public float WindowLength;

        public override string MarkerString
        => $"{base.MarkerString},{WindowLength.ToString("f2")}";

        public WindowedEventMarker
        (
            int spoCount, float windowLength,
            int trainingTarget
        ): base(spoCount, trainingTarget)
        {
            WindowLength = windowLength;
        }
    }

    /// <summary>
    /// Motor Imagery event marker in the format:
    /// <br/><br/>
    /// "mi,{spo count},{train target (-1 if n/a)},{window length}"
    /// </summary>
    public class MIEventMarker: WindowedEventMarker
    {
        public override string MarkerString
        => $"mi,{base.MarkerString}";

        public MIEventMarker
        (
            int spoCount, float windowLength,
            int trainingTarget = -1
        )
        : base(spoCount, windowLength, trainingTarget)
        {}
    }

    /// <summary>
    /// SSVEP event marker in the format:
    /// <br/><br/>
    /// "ssvep,{spo count},{training target (-1 if n/a)},{window length},{...frequencies}"
    /// </summary>
    public class SSVEPEventMarker: WindowedEventMarker
    {
        public float[] Frequencies;

        public override string MarkerString
        => $"ssvep,{base.MarkerString}{FrequenciesString}";

        protected string FrequenciesString
        => Frequencies switch {
            null or {Length: 0} => "",
            _ => $",{string.Join(",", Frequencies)}"
        };

        public SSVEPEventMarker
        (
            int spoCount, float windowLength,
            IEnumerable<float> frequencies,
            int trainingTarget = -1
        )
        : this(spoCount, windowLength, frequencies.ToArray(), trainingTarget)
        {}

        public SSVEPEventMarker
        (
            int spoCount, float windowLength,
            float[] frequencies,
            int trainingTarget = -1
        )
        : base(spoCount, windowLength, trainingTarget)
        {
            Frequencies = frequencies;
        }
    }


    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,[sm],{spo count},{train target (-1 if n/a)}..."
    /// <br/>
    /// <i>where 's' or 'm' indicates single or multi flash</i>
    /// </summary>
    public abstract class P300EventMarker: EventMarker
    {
        public P300EventMarker
        (
            int spoCount, int trainingTarget
        ): base(spoCount, trainingTarget)
        {}
    }

    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,s,{spo count},{train target (-1 if n/a)},{active spo}"
    /// </summary>
    public class SingleFlashP300EventMarker: P300EventMarker
    {
        public int ActiveSPO;

        public override string MarkerString
        => $"p300,s,{base.MarkerString}, {ActiveSPO}";

        public SingleFlashP300EventMarker
        (
            int spoCount, int activeSpo,
            int trainingTarget = -1
        ): base(spoCount, trainingTarget)
        {
            ActiveSPO = activeSpo;
        }
    }

    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,m,{spo count},{train target (-1 if n/a)},{...active spos}"
    /// </summary>
    public class MultiFlashP300EventMarker: P300EventMarker
    {
        public int[] ActiveSPOs;

        public override string MarkerString
        => $"p300,m,{base.MarkerString}{ActiveSpoString}";

        protected string ActiveSpoString
        => ActiveSPOs switch {
            null or {Length: 0} => "",
            _ => $",{string.Join(',', ActiveSPOs)}"
        };

        public MultiFlashP300EventMarker
        (
            int spoCount, IEnumerable<int> activeSpos,
            int trainingTarget = -1
        )
        : this(spoCount, activeSpos.ToArray(), trainingTarget)
        {}

        public MultiFlashP300EventMarker
        (
            int spoCount, int[] activeSpos,
            int trainingTarget = -1
        ): base(spoCount, trainingTarget)
        {
            ActiveSPOs = activeSpos;
        }
    }
}