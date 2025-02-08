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


        public static LSLResponse Parse
        (
            string[] sampleValues, double captureTime
        )
        => sampleValues switch
        {
            _ when sampleValues.All(string.IsNullOrEmpty)
                => CreateMessage<EmptyLSLResponse>(captureTime, sampleValues)
            ,
            { Length: 1 } _
                => SingleChannelLSLResponse.Parse(sampleValues[0], captureTime)
            ,
            _ => CreateUnparsedMessage<LSLResponse>(captureTime, sampleValues, string.Join(" | ", sampleValues))
        };
        
        protected static LSLResponse CreateMessage<T>
        (
            double captureTime, string[] sampleValues
        )
        where T : LSLResponse, new()
        {
            return new T()
            {
                CaptureTime = captureTime,
                RawSampleValues = sampleValues
            };
        }

        protected static LSLResponse CreateUnparsedMessage<T>
        (
            double captureTime, string[] sampleValues, string warningBody
        )
        where T: LSLResponse, new()
        {
            Debug.LogWarning($"Failed to parse {nameof(T)} into meaningful type: {warningBody}");
            return CreateMessage<T>(captureTime, sampleValues);
        }
    }

    public class EmptyLSLResponse: LSLResponse {}


    public class SingleChannelLSLResponse: LSLResponse
    {
        public static LSLResponse Parse
        (
            string sampleValue, double captureTime
        )
        => sampleValue switch
        {
            "ping"
                => CreateMessage<LSLPing>(captureTime, sampleValue)
            ,
            _ when TryMatchRegex(sampleValue, @"^\[?(\d+)\]?$", out string predictionBody)
                => CreateMessage<PredictionResponse>(captureTime, sampleValue, predictionBody)
            ,
            _ when TryMatchRegex(sampleValue, @"^marker received : (.+)$", out string markerBody)
                => LSLMarkerReceipt.Parse(markerBody, sampleValue, captureTime)
            ,
            _ => CreateUnparsedMessage<SingleChannelLSLResponse>(captureTime, sampleValue, sampleValue)
        };

        protected static SingleChannelLSLResponse CreateMessage<T>
        (
            double captureTime, string sampleValue,
            string capturedBody = ""
        )
        where T : SingleChannelLSLResponse, new()
        {
            T newMessage = CreateMessage<T>(captureTime, new[] {sampleValue}) as T;
            if (capturedBody is not "")
            {
                try
                {
                    newMessage.ParseBody(capturedBody);
                }
                catch
                {
                    Debug.LogWarning($"Failed to parse body of {nameof(T)}");
                }
            }
            return newMessage;
        }

        protected static LSLResponse CreateUnparsedMessage<T>
        (
            double captureTime, string sampleValue, string warningBody
        )
        where T: SingleChannelLSLResponse, new()
        {
            return CreateUnparsedMessage<T>(captureTime, new[] {sampleValue}, warningBody);
        }


        protected virtual void ParseBody(string body) {}


        public static bool TryMatchRegex
        (
            string input, string pattern,
            out string capturedGroup
        )
        {
            Match match = Regex.Match(input, pattern);
            capturedGroup = match.Success? match.Groups[1].Value: "";
            return match.Success;
        }
    }

    public class LSLPing: SingleChannelLSLResponse {}

    public class PredictionResponse: SingleChannelLSLResponse
    {
        public int Value {get; protected set;}

        protected override void ParseBody(string body)
        {
            Value = int.Parse(body);
        }
    }
}