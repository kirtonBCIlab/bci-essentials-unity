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

    public class EmptyLSLResponse: LSLResponse {}


    public class SingleChannelLSLResponse: LSLResponse
    {
        private static readonly Regex PredictionRegex
            = new(@"(\d+)\s*:\s*\[([\d\. ]+)\]");
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
        => sampleValue.Trim() switch
        {
            "ping"
                => new LSLPing()
            ,
            string trimmedSample when
                TryMatchRegex(trimmedSample, PredictionRegex, out string[][] predictionSegments)
                => LSLPrediction.Parse(predictionSegments)
            ,
            string trimmedSample when
                TryMatchRegex(trimmedSample, MarkerReceiptRegex, out string markerBody)
                => LSLMarkerReceipt.Parse(markerBody)
            ,
            _ => CreateUnparsedMessage<SingleChannelLSLResponse>(sampleValue)
        };
    }

    public class LSLPing : SingleChannelLSLResponse { }

    public class LSLMarkerReceipt : SingleChannelLSLResponse
    {
        public string MarkerBody { get; protected set; }

        public new static LSLResponse Parse(string body)
        => new LSLMarkerReceipt {MarkerBody = body};
    }


    /// <summary>
    /// Prediction/Selection response from bci-essentials python back end.
    /// <br/><i>(0-indexed)</i>
    /// </summary>
    public class LSLPrediction : SingleChannelLSLResponse
    {
        /// <summary>
        /// Index of object or class to select <i>(0-indexed)</i>
        /// </summary>
        public int Index { get; protected set; }
        /// <summary>
        /// Confidence ratio for each possible class or stimulus item
        /// </summary>
        public float[] Probabilities { get; protected set; }


        public static LSLPrediction Parse(string[][] predictionSegments)
        => predictionSegments switch
        {
            { Length: 1 } => ParseValues(predictionSegments[0]),
            _ => LSLCompositePrediction.Parse(predictionSegments)
        };

        public static LSLPrediction ParseValues(string[] valueStrings)
        {
            try
            {
                return new LSLPrediction()
                {
                    Index = int.Parse(valueStrings[0]),
                    Probabilities = valueStrings[1].Split(" ")
                        .Select(s => float.Parse(s)).ToArray()
                };
            }
            catch (Exception ex)
            {
                throw new FormatException
                (
                    $"Body segments of {typeof(LSLPrediction).Name}"
                    + $"were in unexpected format: {valueStrings}"
                    , ex
                );
            }
        }
    }

    /// <summary>
    /// Prediction/Selection response containing multiple results,
    /// <br>it is recommended to use the most recent result
    /// </summary>
    public class LSLCompositePrediction : LSLPrediction
    {
        public LSLPrediction[] Parts { get; protected set; }

        public new static LSLCompositePrediction Parse(string[][] predictionSegments)
        {
            LSLPrediction[] parts = predictionSegments.Select(
                valueStrings => ParseValues(valueStrings)
            ).ToArray();
            LSLPrediction latest = parts[^1];

            return new()
            {
                Index = latest.Index,
                Probabilities = latest.Probabilities,
                Parts = parts
            };
        }
    }
}