using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

namespace BCIEssentials.LSLFramework
{
    public class LSLResponse
    {
        public double CaptureTime {get; protected set;}
        public string[] RawSampleValues {get; protected set;}

        public LSLResponse() {}

        /// <summary>
        /// Build typed response object from LSL sample.
        /// </summary>
        public static LSLResponse BuildResponse
        (
            string[] sampleValues, double captureTime
        )
        {
            LSLResponse responseObject = sampleValues switch
            {
                _ when sampleValues.All(string.IsNullOrEmpty)
                    => new EmptyLSLResponse()
                ,
                { Length: 1 } _
                    => SingleChannelLSLResponse.Parse(sampleValues[0])
                ,
                _ => CreateUnparsedMessage<LSLResponse>(string.Join(" | ", sampleValues))
            };

            responseObject.CaptureTime = captureTime;
            responseObject.RawSampleValues = sampleValues;

            return responseObject;
        }

        protected static LSLResponse CreateUnparsedMessage<T>
        (
            string warningBody
        )
        where T: LSLResponse, new()
        {
            Debug.LogWarning($"Failed to parse {nameof(T)} into meaningful type: {warningBody}");
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
    }

    public class EmptyLSLResponse: LSLResponse {}


    public class SingleChannelLSLResponse: LSLResponse
    {
        private static readonly Regex PredictionRegex
            = new(@"^\[?(\d+)\]?$");
        private static readonly Regex MarkerReceiptRegex
            = new(@"^marker received : (.+)$");

        /// <summary>
        /// Build typed response object from singular
        /// channel value of an LSL sample.
        /// </summary>
        public static LSLResponse BuildResponse
        (
            string sampleValue, double captureTime
        )
        => BuildResponse(new[] {sampleValue}, captureTime);

        /// <summary>
        /// Parse sample into a skeleton response object
        /// </summary>
        public static LSLResponse Parse
        (
            string sampleValue
        )
        => sampleValue switch
        {
            "ping"
                => new LSLPing()
            ,
            _ when TryMatchRegex(sampleValue, PredictionRegex, out string predictionBody)
                => BuildPartialResponseFromBody<LSLPredictionResponse>(predictionBody)
            ,
            _ when TryMatchRegex(sampleValue, MarkerReceiptRegex, out string markerBody)
                => MarkerReceipt.Parse(markerBody)
            ,
            _ => CreateUnparsedMessage<SingleChannelLSLResponse>(sampleValue)
        };


        protected static SingleChannelLSLResponse BuildPartialResponseFromBody<T>
        (
            string capturedBody = ""
        )
        where T : SingleChannelLSLResponse, new()
        {
            T responseObject = new T();
            
            if (capturedBody is not "")
            {
                try
                {
                    responseObject.ParseBody(capturedBody);
                }
                catch
                {
                    Debug.LogWarning($"Failed to parse body of {nameof(T)}");
                }
            }

            return responseObject;
        }

        protected virtual void ParseBody(string body) {}
    }

    public class LSLPing: SingleChannelLSLResponse {}

    /// <summary>
    /// Prediction/Selection response from bci-essentials python back end
    /// </summary>
    public class LSLPredictionResponse: SingleChannelLSLResponse
    {
        public int Value {get; protected set;}

        protected override void ParseBody(string body)
        {
            Value = int.Parse(body);
        }
    }
}