using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

namespace BCIEssentials.LSLFramework
{
    public class Response
    {
        public double CaptureTime {get; protected set;}
        public string[] RawSampleValues {get; protected set;}

        public Response() {}

        /// <summary>
        /// Build typed response object from LSL sample.
        /// </summary>
        public static Response BuildResponse
        (
            string[] sampleValues, double captureTime
        )
        {
            Response responseObject = sampleValues switch
            {
                _ when sampleValues.All(string.IsNullOrEmpty)
                    => new EmptyResponse()
                ,
                { Length: 1 } _
                    => SingleChannelResponse.Parse(sampleValues[0])
                ,
                _ => CreateUnparsedMessage<Response>(string.Join(" | ", sampleValues))
            };

            responseObject.CaptureTime = captureTime;
            responseObject.RawSampleValues = (string[])sampleValues.Clone();

            return responseObject;
        }

        protected static Response CreateUnparsedMessage<T>
        (
            string warningBody
        )
        where T: Response, new()
        {
            Debug.LogWarning($"Failed to parse {typeof(T).Name} into meaningful type: {warningBody}");
            return new T();
        }


        public override string ToString()
        => GetType().Name + ": {"
            + "capture time: " + CaptureTime + ","
            + "values: [" + string.Join(',', RawSampleValues) + "]"
            + "}";


        public static bool TryMatchRegex
        (
            string input, Regex pattern,
            out string capturedGroup
        )
        {
            Match match = pattern.Match(input);
            capturedGroup = match.Success? match.Groups[1].Value: "";
            return match.Success;
        }

        public static bool TryMatchRegex
        (
            string input, Regex pattern,
            out string[][] capturedGroups
        )
        {
            MatchCollection matches = pattern.Matches(input);
            capturedGroups = matches.Select(
                match => match.Groups.Select(
                    group => group.Value
                ).ToArray()[1..]
            ).ToArray();
            return matches.Count > 0;
        }
    }

    public class EmptyResponse: Response {}


    public class SingleChannelResponse: Response
    {
        private static readonly Regex PredictionRegex
            = new(@"(\d+)\s*:\s*\[([\d\. ]+)\]");
        private static readonly Regex MarkerReceiptRegex
            = new(@"^marker received\s*:\s*(.+)$");

        /// <summary>
        /// Build typed response object from singular
        /// channel value of an LSL sample.
        /// </summary>
        public static Response BuildResponse
        (
            string sampleValue, double captureTime
        )
        => BuildResponse(new[] {sampleValue}, captureTime);

        /// <summary>
        /// Parse sample into a skeleton response object
        /// </summary>
        public static Response Parse
        (
            string sampleValue
        )
        => sampleValue.Trim() switch
        {
            "ping"
                => new Ping()
            ,
            string trimmedSample when
                TryMatchRegex(trimmedSample, PredictionRegex, out string[][] predictionSegments)
                => Prediction.Parse(predictionSegments)
            ,
            string trimmedSample when
                TryMatchRegex(trimmedSample, MarkerReceiptRegex, out string markerBody)
                => MarkerReceipt.Parse(markerBody)
            ,
            _ => CreateUnparsedMessage<SingleChannelResponse>(sampleValue)
        };
    }

    public class Ping : SingleChannelResponse { }

    public class MarkerReceipt : SingleChannelResponse
    {
        public string MarkerBody { get; protected set; }

        public new static Response Parse(string body)
        => new MarkerReceipt {MarkerBody = body};
    }
}