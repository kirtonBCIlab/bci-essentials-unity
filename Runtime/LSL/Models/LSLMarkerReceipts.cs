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
                => CreateMessage<TrialStartedMarkerReceipt>(captureTime, sampleValue)
            ,
            "Trial Ends"
                => CreateMessage<TrialEndsMarkerReceipt>(captureTime, sampleValue)
            ,
            "Training Complete"
                => CreateMessage<TrainingCompleteMarkerReceipt>(captureTime, sampleValue)
            ,
            "Update Classifier"
                => CreateMessage<UpdateClassifierMarkerReceipt>(captureTime, sampleValue)
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
                => CreateMessage<P300EventMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            "ssvep"
                => CreateMessage<SSVEPEventMarkerReceipt>(captureTime, sampleValue, markerBody)
            ,
            _ => CreateUnparsedMessage<EventMarkerReceipt>(captureTime, sampleValue, markerBody)
        };

        public static bool TryMatchParadigm(string markerBody, out string paradigm)
        => TryMatchRegex(markerBody, @"^(mi|p300|ssvep),", out paradigm);
    }


// TODO: write parsing for event marker receipts,
// account for whitespace between commas
    public class MIEventMarkerReceipt: EventMarkerReceipt
    {
        // "mi,{spo count},{train target (-1 if n/a)},{window length}"
    }

    public class P300EventMarkerReceipt: EventMarkerReceipt
    {
        // single or multiflash
        // "p300,[sm],{spo count},{train target (-1 if n/a)},{active spo}"
    }

    public class SSVEPEventMarkerReceipt: EventMarkerReceipt
    {
        // "ssvep",{spo count},{training target (-1 if n/a)},{window length},{...frequencies}
    }
}