using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System;

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
            responseObject.RawSampleValues = (string[])sampleValues.Clone();

            return responseObject;
        }

        protected static LSLResponse CreateUnparsedMessage<T>
        (
            string warningBody
        )
        where T: LSLResponse, new()
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
    }

    public class EmptyLSLResponse: LSLResponse {}


    public class SingleChannelLSLResponse: LSLResponse
    {
        private static readonly char[] TrimmedCharacters
            = new[] {'[', ']'};
        private static readonly Regex PredictionRegex
            = new(@"^(\d+)$");
        private static readonly Regex MarkerReceiptRegex
            = new(@"^marker received\s*:\s*(.+)$");

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
        => sampleValue.Trim(TrimmedCharacters).Trim() switch
        {
            "ping"
                => new LSLPing()
            ,
            string trimmedSample when
                TryMatchRegex(trimmedSample, PredictionRegex, out string predictionBody)
                => BuildPartialResponseFromBody<LSLPredictionResponse>(predictionBody)
            ,
            string trimmedSample when
                TryMatchRegex(trimmedSample, MarkerReceiptRegex, out string markerBody)
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
                catch (Exception ex)
                {
                    throw new FormatException
                    (
                        $"Body of {typeof(T).Name} was in unexpected format: {capturedBody}"
                    , ex);
                }
            }

            return responseObject;
        }

        protected virtual void ParseBody(string body) {}
    }

    public class LSLPing: SingleChannelLSLResponse {}

    /// <summary>
    /// Prediction/Selection response from bci-essentials python back end.
    /// <br/><i>(0-indexed)</i>
    /// </summary>
    public class LSLPredictionResponse: SingleChannelLSLResponse
    {
        /// <summary>
        /// Index of object or class to select <i>(0-indexed)</i>
        /// </summary>
        public int Value {get; protected set;}

        protected override void ParseBody(string body)
        {
            Value = int.Parse(body);
            if (Value > 0) Value--;
        }
    }
}