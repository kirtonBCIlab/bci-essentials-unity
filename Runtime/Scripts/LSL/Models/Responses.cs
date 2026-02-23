using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System;

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


    /// <summary>
    /// Prediction/Selection response from bci-essentials python back end.
    /// <br/><i>(0-indexed)</i>
    /// </summary>
    public class Prediction : SingleChannelResponse
    {
        /// <summary>
        /// Index of object or class to select <i>(0-indexed)</i>
        /// </summary>
        public int Index { get; protected set; }
        /// <summary>
        /// Confidence ratio for each possible class or stimulus item
        /// </summary>
        public float[] Probabilities { get; protected set; }


        public static Prediction Parse(string[][] predictionSegments)
        => predictionSegments switch
        {
            { Length: 1 } => ParseValues(predictionSegments[0]),
            _ => CompositePrediction.Parse(predictionSegments)
        };

        public static Prediction ParseValues(string[] valueStrings)
        {
            try
            {
                return new Prediction()
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
                    $"Body segments of {typeof(Prediction).Name}"
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
    public class CompositePrediction : Prediction
    {
        public Prediction[] Parts { get; protected set; }

        public new static CompositePrediction Parse(string[][] predictionSegments)
        {
            Prediction[] parts = predictionSegments.Select(
                valueStrings => ParseValues(valueStrings)
            ).ToArray();
            Prediction latest = parts[^1];

            return new()
            {
                Index = latest.Index,
                Probabilities = latest.Probabilities,
                Parts = parts
            };
        }
    }
}