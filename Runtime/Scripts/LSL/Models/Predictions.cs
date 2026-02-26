using UnityEngine;
using System.Linq;
using System;

namespace BCIEssentials.LSLFramework
{

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
                int label = int.Parse(valueStrings[0]);
                int index = label - 1;
                if (label == 0)
                {
                    index = 0;
                    Debug.LogWarning("Received unexpected prediction label of 0");
                }

                float[] probabilities = valueStrings[1].Split(" ")
                .Select(s => float.Parse(s)).ToArray();

                return new Prediction()
                { Index = index, Probabilities = probabilities };
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