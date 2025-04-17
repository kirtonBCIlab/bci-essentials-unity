using System.Text.RegularExpressions;
using System.Linq;

namespace BCIEssentials.LSLFramework
{
    public class MarkerReceipt : SingleChannelLSLResponse
    {
        public string MarkerBody {get; protected set;}

        /// <summary>
        /// Parse sample into a skeleton response object
        /// </summary>
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
        /// <summary>
        /// Parse sample into a skeleton response object
        /// </summary>
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
            = new(@"^(mi|switch|p300|ssvep|tvep),");

        public int ObjectCount {get; protected set;}
        public int TrainingTarget {get; protected set;}

        /// <summary>
        /// Parse sample into a skeleton response object
        /// </summary>
        public static LSLResponse Parse
        (
            string paradigmLabel, string markerBody
        )
        => paradigmLabel switch
        {
            "mi"
                => BuildPartialResponseFromBody<MIEventMarkerReceipt>(markerBody)
            ,
            "switch"
                => BuildPartialResponseFromBody<SwitchEventMarkerReceipt>(markerBody)
            ,
            "ssvep"
                => BuildPartialResponseFromBody<SSVEPEventMarkerReceipt>(markerBody)
            ,
            "tvep"
                => BuildPartialResponseFromBody<TVEPEventMarkerReceipt>(markerBody)
            ,
            "p300"
                => P300EventMarkerReceipt.Parse(markerBody)
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
            string[] bodySegments = body.Split(",");
            bodySegments = bodySegments.Select(s => s.Trim()).ToArray();
            ParseBodySegments(bodySegments);
        }

        protected virtual void ParseBodySegments(string[] bodySegments) {}
    }


    public abstract class EpochEventMarkerReceipt: EventMarkerReceipt
    {
        public float EpochLength {get; protected set;}

        protected override void ParseBodySegments(string[] bodySegments)
        {
            ObjectCount = int.Parse(bodySegments[1]);
            TrainingTarget = int.Parse(bodySegments[2]);
            EpochLength = float.Parse(bodySegments[3]);
        }
    }

    /// <summary>
    /// Receipt for Motor Imagery event marker in the format:
    /// <br/><br/>
    /// "mi,{object count},{train target (-1 if n/a)},{epoch length}"
    /// </summary>
    public class MIEventMarkerReceipt: EpochEventMarkerReceipt {}

    /// <summary>
    /// Receipt for Switch event marker in the format:
    /// <br/><br/>
    /// "switch,{object count},{train target (-1 if n/a)},{epoch length}"
    /// </summary>
    public class SwitchEventMarkerReceipt: EpochEventMarkerReceipt {}


    public abstract class VisualEvokedPotentialEventMarkerReceipt: EpochEventMarkerReceipt
    {
        public float[] Frequencies {get; protected set;}

        protected override void ParseBodySegments(string[] bodySegments)
        {
            base.ParseBodySegments(bodySegments);

            Frequencies = new float[bodySegments.Length - 4];
            for (int i = 4; i < bodySegments.Length; i++)
            {
                Frequencies[i - 4] = float.Parse(bodySegments[i]);
            }
        }
    }

    /// <summary>
    /// Receipt for SSVEP event marker in the format:
    /// <br/><br/>
    /// "ssvep,{object count},{train target (-1 if n/a)},{epoch length},{...frequencies}"
    /// </summary>
    public class SSVEPEventMarkerReceipt: VisualEvokedPotentialEventMarkerReceipt {}
    
    /// <summary>
    /// Receipt for TVEP event marker in the format:
    /// <br/><br/>
    /// "tvep,{object count},{train target (-1 if n/a)},{epoch length},{...frequencies}"
    /// </summary>
    public class TVEPEventMarkerReceipt: VisualEvokedPotentialEventMarkerReceipt {}


    /// <summary>
    /// Receipt for P300 event marker in the format:
    /// <br/><br/>
    /// "p300,[sm],{object count},{train target (-1 if n/a)}..."
    /// <br/>
    /// <i>where 's' or 'm' indicates single or multi flash</i>
    /// </summary>
    public class P300EventMarkerReceipt: EventMarkerReceipt
    {
        private static readonly Regex FlashTypeRegex
            = new(@"^\w+,\s*(\w)");

        /// <summary>
        /// Parse sample into a skeleton response object
        /// </summary>
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
            ObjectCount = int.Parse(bodySegments[2]);
            TrainingTarget = int.Parse(bodySegments[3]);
        }
    }

    /// <summary>
    /// Receipt for P300 event marker in the format:
    /// <br/><br/>
    /// "p300,s,{object count},{train target (-1 if n/a)},{active object}"
    /// </summary>
    public class SingleFlashP300EventMarkerReceipt: P300EventMarkerReceipt
    {
        public int ActiveObject {get; protected set;}

        protected override void ParseBodySegments(string[] bodySegments)
        {
            base.ParseBodySegments(bodySegments);
            ActiveObject = int.Parse(bodySegments[4]);
        }
    }
    
    /// <summary>
    /// Receipt for P300 event marker in the format:
    /// <br/><br/>
    /// "p300,m,{object count},{train target (-1 if n/a)},{...active object}"
    /// </summary>
    public class MultiFlashP300EventMarkerReceipt: P300EventMarkerReceipt
    {
        public int[] ActiveObjects {get; protected set;}

        protected override void ParseBodySegments(string[] bodySegments)
        {
            base.ParseBodySegments(bodySegments);

            ActiveObjects = new int[bodySegments.Length - 4];
            for (int i = 4; i < bodySegments.Length; i++)
            {
                ActiveObjects[i - 4] = int.Parse(bodySegments[i]);
            }
        }
    }
}