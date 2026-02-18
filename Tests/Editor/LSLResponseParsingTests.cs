using System;
using BCIEssentials.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.Editor
{
    internal class LSLResponseParsingTests
    {
        [Test]
        [TestCase("ping")]
        [TestCase("[ping]")]
        public void BuildResponse_WhenPing_ThenReturnsPing
        (
            string sampleString
        )
        => ParseResponseAndAssertType<LSLPing>(sampleString);

        [Test]
        [TestCase("1", 0)]
        [TestCase("[3]", 2)]
        [TestCase(" 2 ", 1)]
        public void BuildResponse_WhenPrediction_ThenReturnsPredictionResponse
        (
            string sampleString, int expectedValue
        )
        {
            var prediction = ParseResponseAndAssertType<LSLPredictionResponse>(sampleString);
            Assert.AreEqual(expectedValue, prediction.Value);
        }

        [Test]
        [TestCase(new object[] {})]
        [TestCase(new object[] {"", null})]
        public void BuildResponse_WhenSampleEmpty_ThenReturnsEmptyResponse
        (
            params string[] sampleValue
        )
        => ParseResponseAndAssertType<EmptyLSLResponse>(sampleValue);

        [Test]
        [TestCase(new[] {"invalid"}, typeof(SingleChannelLSLResponse))]
        [TestCase(new[] {"invalid", "invalid"}, typeof(LSLResponse))]
        public void BuildResponse_WhenUnrecognizedSample_ThenReturnsUnparsedResponse
        (
            string[] sampleValue, Type expectedType
        )
        => ParseResponseAndAssertType(sampleValue, expectedType);


        private T ParseResponseAndAssertType<T>(string sampleString) where T: LSLResponse
        => ParseResponseAndAssertType(sampleString, typeof(T)) as T;
        private LSLResponse ParseResponseAndAssertType(string sampleString, Type expectedType)
        => ParseResponseAndAssertType(new[] {sampleString}, expectedType);
        private T ParseResponseAndAssertType<T>(string[] sampleValue) where T: LSLResponse
        => ParseResponseAndAssertType(sampleValue, typeof(T)) as T;
        private LSLResponse ParseResponseAndAssertType(string[] sampleValue, Type expectedType)
        {
            LSLResponse result = LSLResponse.BuildResponse(sampleValue, 0);
            Assert.AreEqual(expectedType, result.GetType());
            return result;
        }
    }
}