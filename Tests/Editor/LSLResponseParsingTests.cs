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
        [TestCase("0", 0)]
        [TestCase("[2]", 2)]
        [TestCase(" 1 ", 1)]
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
        [TestCase(new[] {"marker received: invalid"}, typeof(CommandMarkerReceipt))]
        [TestCase(new[] {"marker received: invalid,1,1,1"}, typeof(MarkerReceipt))]
        [TestCase(new[] {"marker received: p300,invalid,1,1,1"}, typeof(P300EventMarkerReceipt))]
        public void BuildResponse_WhenUnrecognizedSample_ThenReturnsUnparsedResponse
        (
            string[] sampleValue, Type expectedType
        )
        => ParseResponseAndAssertType(sampleValue, expectedType);

        

        [Test]
        [TestCase("Trial Started", typeof(TrialStartedMarkerReceipt))]
        [TestCase("Trial Ends", typeof(TrialEndsMarkerReceipt))]
        [TestCase("Training Complete", typeof(TrainingCompleteMarkerReceipt))]
        [TestCase("Update Classifier", typeof(UpdateClassifierMarkerReceipt))]
        [TestCase("mi,1,0,2.5", typeof(MIEventMarkerReceipt))]
        [TestCase("switch,1,0,2.5", typeof(SwitchEventMarkerReceipt))]
        [TestCase("ssvep,4,2,1.5,12.5,18.7,24.4,30.1", typeof(SSVEPEventMarkerReceipt))]
        [TestCase("tvep,6,2,1.5,15.0", typeof(TVEPEventMarkerReceipt))]
        [TestCase("p300,s,8,3,1", typeof(SingleFlashP300EventMarkerReceipt))]
        [TestCase("p300,m,8,3,1,3,5,7", typeof(MultiFlashP300EventMarkerReceipt))]
        public void BuildResponse_WhenMarkerReceipt_ThenReturnsRelevantMarkerReceipt
        (
            string markerString, Type expectedType
        )
        {
            string sampleString = BuildMarkerReceiptString(markerString);
            ParseResponseAndAssertType(sampleString, expectedType);
        }

        [Test]
        [TestCase("mi,1,0,2.5", 1, 0, 2.5f)]
        [TestCase("mi,1,-2,1.0", 1, -2, 1.0f)]
        [TestCase("mi,2,3,1.245", 2, 3, 1.245f)]
        public void BuildResponse_WhenMIMarkerReceipt_ThenParsesMIMarkerReceipt
        (
            string markerString, int expectedObjectCount,
            int expectedTrainTarget, float expectedWindowLength
        )
        {
            string sampleString = BuildMarkerReceiptString(markerString);
            var markerReceipt = ParseResponseAndAssertType<MIEventMarkerReceipt>(sampleString);
            Assert.AreEqual(expectedObjectCount, markerReceipt.ObjectCount);
            Assert.AreEqual(expectedTrainTarget, markerReceipt.TrainingTarget);
            Assert.AreEqual(expectedWindowLength, markerReceipt.WindowLength);
        }


        private string BuildMarkerReceiptString(string markerString)
        => "marker received: " + markerString;

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