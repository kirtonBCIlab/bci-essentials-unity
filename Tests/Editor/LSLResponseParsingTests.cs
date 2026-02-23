using System;
using System.Linq;
using BCIEssentials.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.Editor
{
    internal class LSLResponseParsingTests
    {
        [Test]
        [TestCase("ping")]
        public void BuildResponse_WhenPing_ThenReturnsPing
        (
            string sampleString
        )
        => ParseResponseAndAssertType<Ping>(sampleString);

        [Test]
        [TestCase("1:[0.52 0.47 0.01]", 1, new float[] { 0.52f, 0.47f, 0.01f })]
        public void BuildResponse_WhenPrediction_ThenReturnsPrediction
        (
            string sampleString, int expectedIndex, float[] expectedProbabilities
        )
        {
            var prediction = ParseResponseAndAssertType<Prediction>(sampleString);
            new PredictionValues()
            {
                Index = expectedIndex,
                Probabilities = expectedProbabilities
            }.AssertEqual(prediction);
        }


        [Test]
        [
            TestCase(
                "1:[0.2853 0.7147],0:[0.8230 0.1770],1:[0.4573 0.5427],0:[0.9358 0.0642]",
                "constituent prediction"
            )
        ]
        public void BuildResponse_WhenCompositePrediction_ThenReturnsCompositePrediction
        (
            string sampleString, string dataKey
        )
        {
            var expectedValues = GetTestData(dataKey) as PredictionValues[];

            var prediction = ParseResponseAndAssertType<CompositePrediction>(sampleString);
            Assert.AreEqual(expectedValues.Length, prediction.Parts.Length);
            
            for(int i = 0; i < expectedValues.Length; i++)
            {
                expectedValues[i].AssertEqual(
                    prediction.Parts[i]
                );
            }
        }

        [Test]
        [TestCase(new object[] {})]
        [TestCase(new object[] {"", null})]
        public void BuildResponse_WhenSampleEmpty_ThenReturnsEmptyResponse
        (
            params string[] sampleValue
        )
        => ParseResponseAndAssertType<EmptyResponse>(sampleValue);

        [Test]
        [TestCase(new[] {"invalid"}, typeof(SingleChannelResponse))]
        [TestCase(new[] {"invalid", "invalid"}, typeof(Response))]
        public void BuildResponse_WhenUnrecognizedSample_ThenReturnsUnparsedResponse
        (
            string[] sampleValue, Type expectedType
        )
        => ParseResponseAndAssertType(sampleValue, expectedType);


        private T ParseResponseAndAssertType<T>(string sampleString) where T: Response
        => ParseResponseAndAssertType(sampleString, typeof(T)) as T;
        private Response ParseResponseAndAssertType(string sampleString, Type expectedType)
        => ParseResponseAndAssertType(new[] {sampleString}, expectedType);
        private T ParseResponseAndAssertType<T>(string[] sampleValue) where T: Response
        => ParseResponseAndAssertType(sampleValue, typeof(T)) as T;
        private Response ParseResponseAndAssertType(string[] sampleValue, Type expectedType)
        {
            Response result = Response.BuildResponse(sampleValue, 0);
            Assert.AreEqual(expectedType, result.GetType());
            return result;
        }


        object GetTestData(string key)
        => key switch
        {
            "single prediction" => _predictionTestValues,
            "constituent prediction" => _constituentPredictionTestValues,
            _ => null
        };

        struct PredictionValues
        {
            public int Index;
            public float[] Probabilities;

            public void AssertEqual(Prediction actual)
            {
                Assert.AreEqual(Index, actual.Index);
                Assert.That(Probabilities, Is.EquivalentTo(actual.Probabilities));
            }
        }
        static readonly PredictionValues _predictionTestValues
        = new() { Index = 1, Probabilities = new float[] { 0.52f, 0.47f, 0.01f } };
        static readonly PredictionValues[] _constituentPredictionTestValues
        = new PredictionValues[]
        {
            new() {Index = 1, Probabilities = new float[] { 0.2853f, 0.7147f } },
            new() {Index = 0, Probabilities = new float[] { 0.8230f, 0.1770f } },
            new() {Index = 1, Probabilities = new float[] { 0.4573f, 0.5427f } },
            new() {Index = 0, Probabilities = new float[] { 0.9358f, 0.0642f } }
        };
    }
}