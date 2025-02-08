namespace BCIEssentials.LSLFramework
{
    public class LSLMarkerReceipt : SingleChannelLSLResponse
    {
        public string MarkerBody {get; protected set;}

        public static LSLResponse Parse
        (
            string markerBody, string sampleValue, double captureTime
        )
        => markerBody switch
        {
            _ when !markerBody.Contains(',')
                => CommandMarkerReceipt.Parse(markerBody, sampleValue, captureTime)
            ,
            _ when EventMarkerReceipt.TryMatchParadigm(markerBody, out string paradigm)
                => EventMarkerReceipt.Parse(paradigm, markerBody, sampleValue, captureTime)
            ,
            _ => CreateUnparsedMessage<LSLMarkerReceipt>(captureTime, sampleValue, markerBody)
        };

        protected override void ParseBody(string body)
        {
            MarkerBody = body;
        }
    }

    public class CommandMarkerReceipt: LSLMarkerReceipt
    {
        public new static LSLResponse Parse
        (
            string markerBody, string sampleValue, double captureTime
        )
        => markerBody switch
        {
            "Trial Started"
                => CreateMessage<TrialStartedMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            "Trial Ends"
                => CreateMessage<TrialEndsMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            "Training Complete"
                => CreateMessage<TrainingCompleteMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            "Update Classifier"
                => CreateMessage<UpdateClassifierMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            _ => CreateUnparsedMessage<CommandMarkerReceipt>(captureTime, sampleValue, markerBody)
        };
    }

    public class TrialStartedMarkerReceipt: CommandMarkerReceipt {}
    public class TrialEndsMarkerReceipt: CommandMarkerReceipt {}
    public class TrainingCompleteMarkerReceipt: CommandMarkerReceipt {}
    public class UpdateClassifierMarkerReceipt: CommandMarkerReceipt {}


    public class EventMarkerReceipt: LSLMarkerReceipt
    {
        public int SpoCount {get; protected set;}
        public int TrainingTarget {get; protected set;}

        public static LSLResponse Parse
        (
            string paradigmFlag, string markerBody,
            string sampleValue, double captureTime
        )
        => paradigmFlag switch
        {
            "mi"
                => CreateMessage<MIEventMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            "p300"
                => P300EventMarkerReceipt.Parse(markerBody, sampleValue, captureTime)
            ,
            "ssvep"
                => CreateMessage<SSVEPEventMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            _ => CreateUnparsedMessage<EventMarkerReceipt>(captureTime, sampleValue, markerBody)
        };

        public static bool TryMatchParadigm(string markerBody, out string paradigm)
        => TryMatchRegex(markerBody, @"^(mi|p300|ssvep),", out paradigm);


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
        public int ActiveSPO {get; protected set;}

        // single or multiflash
        // "p300,[sm],{spo count},{train target (-1 if n/a)},{active spo}"
        public new static LSLResponse Parse
        (
            string markerBody,
            string sampleValue, double captureTime
        )
        {
            TryMatchRegex(markerBody, @"^\w+,\s*(\w)", out string flashType);
            return flashType switch {
                "s"
                    => CreateMessage<SingleFlashP300EventMarkerReceipt>(captureTime, sampleValue, markerBody)
                ,
                "m"
                    => CreateMessage<MultiFlashP300EventMarkerReceipt>(captureTime, sampleValue, markerBody)
                ,
                _ => CreateUnparsedMessage<P300EventMarkerReceipt>(captureTime, sampleValue, markerBody)
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