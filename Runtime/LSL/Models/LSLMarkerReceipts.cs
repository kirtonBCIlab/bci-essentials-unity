using System.Text.RegularExpressions;

namespace BCIEssentials.LSLFramework
{
    public class MarkerReceipt : SingleChannelLSLResponse
    {
        public string MarkerBody {get; protected set;}

        public new static LSLResponse Parse(string markerBody)
        => markerBody switch
        {
            _ when !markerBody.Contains(',')
                => CommandMarkerReceipt.Parse(markerBody)
            ,
            _ when EventMarkerReceipt.TryMatchParadigm(markerBody, out string paradigm)
                => EventMarkerReceipt.Parse(paradigm, markerBody)
            ,
            _ => CreateUnparsedMessage<MarkerReceipt>(markerBody)
        };

        protected override void ParseBody(string body)
        {
            MarkerBody = body;
        }
    }

    public class CommandMarkerReceipt: MarkerReceipt
    {
        public new static LSLResponse Parse(string markerBody)
        => markerBody switch
        {
            "Trial Started"
                => BuildPartialResponseFromBody<TrialStartedMarkerReceipt>(markerBody)
            ,
            "Trial Ends"
                => BuildPartialResponseFromBody<TrialEndsMarkerReceipt>(markerBody)
            ,
            "Training Complete"
                => BuildPartialResponseFromBody<TrainingCompleteMarkerReceipt>(markerBody)
            ,
            "Update Classifier"
                => BuildPartialResponseFromBody<UpdateClassifierMarkerReceipt>(markerBody)
            ,
            _ => CreateUnparsedMessage<CommandMarkerReceipt>(markerBody)
        };
    }

    public class TrialStartedMarkerReceipt: CommandMarkerReceipt {}
    public class TrialEndsMarkerReceipt: CommandMarkerReceipt {}
    public class TrainingCompleteMarkerReceipt: CommandMarkerReceipt {}
    public class UpdateClassifierMarkerReceipt: CommandMarkerReceipt {}


    public class EventMarkerReceipt: MarkerReceipt
    {
        private static readonly Regex ParadigmRegex
            = new(@"^(mi|p300|ssvep),");

        public int SpoCount {get; protected set;}
        public int TrainingTarget {get; protected set;}

        public static LSLResponse Parse
        (
            string paradigmLabel, string markerBody
        )
        => paradigmLabel switch
        {
            "mi"
                => BuildPartialResponseFromBody<MIEventMarkerReceipt>(markerBody)
            ,
            "p300"
                => P300EventMarkerReceipt.Parse(markerBody)
            ,
            "ssvep"
                => BuildPartialResponseFromBody<SSVEPEventMarkerReceipt>(markerBody)
            ,
            _ => CreateUnparsedMessage<EventMarkerReceipt>(markerBody)
        };

        public static bool TryMatchParadigm
        (
            string markerBody, out string paradigm
        )
        => TryMatchRegex(markerBody, ParadigmRegex, out paradigm);


        protected override void ParseBody(string body)
        {
            base.ParseBody(body);
            ParseBodySegments(body.Split(","));
        }

        protected virtual void ParseBodySegments(string[] bodySegments) {}
    }


    public abstract class WindowedEventMarkerReceipt: EventMarkerReceipt
    {
        public float WindowLength {get; protected set;}

        // "{paradigm},{spo count},{train target (-1 if n/a)},{window length}..."
        protected override void ParseBodySegments(string[] bodySegments)
        {
            SpoCount = int.Parse(bodySegments[1]);
            TrainingTarget = int.Parse(bodySegments[2]);
            WindowLength = float.Parse(bodySegments[3]);
        }
    }

    public class MIEventMarkerReceipt: WindowedEventMarkerReceipt
    {
        // "mi,{spo count},{train target (-1 if n/a)},{window length}"
    }

    public class SSVEPEventMarkerReceipt: EventMarkerReceipt
    {
        public float[] Frequencies {get; protected set;}

        // "ssvep",{spo count},{training target (-1 if n/a)},{window length},{...frequencies}
        protected override void ParseBodySegments(string[] bodySegments)
        {
            base.ParseBodySegments(bodySegments);

            Frequencies = new float[bodySegments.Length - 4];
            for (int i = 4; i < bodySegments.Length; i++)
            {
                Frequencies[i] = float.Parse(bodySegments[i]);
            }
        }
    }

    public class P300EventMarkerReceipt: EventMarkerReceipt
    {
        private static readonly Regex FlashTypeRegex
            = new(@"^\w+,\s*(\w)");

        public int ActiveSPO {get; protected set;}

        // single or multiflash
        // "p300,[sm],{spo count},{train target (-1 if n/a)},{active spo}"
        public new static LSLResponse Parse(string markerBody)
        {
            TryMatchRegex(markerBody, FlashTypeRegex, out string flashType);
            return flashType switch {
                "s"
                    => BuildPartialResponseFromBody<SingleFlashP300EventMarkerReceipt>(markerBody)
                ,
                "m"
                    => BuildPartialResponseFromBody<MultiFlashP300EventMarkerReceipt>(markerBody)
                ,
                _ => CreateUnparsedMessage<P300EventMarkerReceipt>(markerBody)
            };
        }

        protected override void ParseBodySegments(string[] bodySegments)
        {
            SpoCount = int.Parse(bodySegments[2]);
            TrainingTarget = int.Parse(bodySegments[3]);
            ActiveSPO = int.Parse(bodySegments[4]);
        }
    }

    public class SingleFlashP300EventMarkerReceipt: P300EventMarkerReceipt {}
    public class MultiFlashP300EventMarkerReceipt: P300EventMarkerReceipt {}
}