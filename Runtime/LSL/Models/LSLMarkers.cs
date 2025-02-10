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
    }


    public abstract class WindowedEventMarker: EventMarker
    {
        public float WindowLength;

        public override string MarkerString
        => $"{base.MarkerString},{WindowLength.ToString("f2")}";

        public WindowedEventMarker
        (
            int spoCount, float windowLength,
            int trainingTarget = -1
        )
        {
            SpoCount = spoCount;
            TrainingTarget = trainingTarget;
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
            {Length: 0} => "",
            _ => $",{string.Join(",", Frequencies)}"
        };

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
    /// "p300,[sm],{spo count},{train target (-1 if n/a)},{active spo}"
    /// <br/>
    /// <i>where 's' or 'm' indicated single or multi flash</i>
    /// </summary>
    public abstract class P300EventMarker: EventMarker
    {
        public int ActiveSPO;

        public override string MarkerString
        => $"{base.MarkerString},{ActiveSPO}";

        public P300EventMarker
        (
            int spoCount, int activeSpo,
            int trainingTarget = -1
        )
        {
            SpoCount = spoCount;
            ActiveSPO = activeSpo;
            TrainingTarget = trainingTarget;
        }
    }

    public class SingleFlashP300EventMarker: P300EventMarker
    {
        public override string MarkerString
        => $"p300,s,{base.MarkerString}";

        public SingleFlashP300EventMarker
        (
            int spoCount, int activeSpo,
            int trainingTarget = -1
        )
        : base(spoCount, activeSpo, trainingTarget)
        {}
    }

    public class MultiFlashP300EventMarker: P300EventMarker
    {
        public override string MarkerString
        => $"p300,m,{base.MarkerString}";

        public MultiFlashP300EventMarker
        (
            int spoCount, int activeSpo,
            int trainingTarget = -1
        )
        : base(spoCount, activeSpo, trainingTarget)
        {}
    }
}