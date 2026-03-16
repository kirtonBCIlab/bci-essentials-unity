using System;
using NUnit.Framework;
using BCIEssentials.SerialPort;
using BCIEssentials.LSLFramework;

namespace BCIEssentials.Tests.Editor
{
    internal class SerialTriggerWriterTests
    {
        private SerialMarkerWriter _writer;

        [SetUp]
        public void SetUp()
        {
            var go = new UnityEngine.GameObject("TestSerialTrigger");
            _writer = go.AddComponent<SerialMarkerWriter>();
            _writer.ConnectOnAwake = false;
        }

        [TearDown]
        public void TearDown()
        {
            if (_writer != null)
                UnityEngine.Object.DestroyImmediate(_writer.gameObject);
        }


        [Test]
        [TestCase(typeof(TrialStartedMarker), 240)]
        [TestCase(typeof(TrialEndsMarker), 241)]
        [TestCase(typeof(TrainingCompleteMarker), 242)]
        [TestCase(typeof(TrainClassifierMarker), 243)]
        [TestCase(typeof(UpdateClassifierMarker), 244)]
        [TestCase(typeof(DoneWithRestingStateCollectionMarker), 245)]
        public void ResolveTriggerByte_WhenStatusMarker_ThenReturnsConfiguredByte
        (
            Type markerType, int expectedByte
        )
        {
            var marker = (IStatusMarker)Activator.CreateInstance(markerType);
            Assert.AreEqual(expectedByte, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void ResolveTriggerByte_WhenStatusByteChanged_ThenReturnsNewValue()
        {
            _writer.TrialStartedByte = 42;
            Assert.AreEqual(42, _writer.ResolveTriggerByte(new TrialStartedMarker()));
        }


        [Test]
        [TestCase(0, 1)]
        [TestCase(2, 3)]
        [TestCase(5, 6)]
        public void ResolveTriggerByte_WhenSingleFlashP300_ThenReturnsStimulusIndexPlusOne
        (
            int stimulusIndex, int expectedByte
        )
        {
            var marker = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: stimulusIndex
            );
            Assert.AreEqual(expectedByte, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void ResolveTriggerByte_WhenMultiFlashP300_ThenReturnsFirstIndexPlusOne()
        {
            var marker = new MultiFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0,
                stimulusIndices: new int[] { 3, 5 }
            );
            Assert.AreEqual(4, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void ResolveTriggerByte_WhenMultiFlashP300EmptyIndices_ThenReturnsZero()
        {
            var marker = new MultiFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0,
                stimulusIndices: new int[0]
            );
            Assert.AreEqual(0, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 2)]
        public void ResolveTriggerByte_WhenMITraining_ThenReturnsTargetIndexPlusOne
        (
            int trainingTarget, int expectedByte
        )
        {
            var marker = new MIEventMarker(stateCount: 2, trainingTargetIndex: trainingTarget, epochLength: 2.0f);
            Assert.AreEqual(expectedByte, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void ResolveTriggerByte_WhenMIClassification_ThenReturnsZero()
        {
            var marker = new MIEventMarker(stateCount: 2, trainingTargetIndex: -1, epochLength: 2.0f);
            Assert.AreEqual(0, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void ResolveTriggerByte_WhenSSVEPTraining_ThenReturnsTargetIndexPlusOne()
        {
            var marker = new SSVEPEventMarker(
                trainingTargetIndex: 0, epochLength: 4.0f,
                frequencies: new float[] { 8f, 10f, 12f }
            );
            Assert.AreEqual(1, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void ResolveTriggerByte_WhenSSVEPClassification_ThenReturnsZero()
        {
            var marker = new SSVEPEventMarker(
                trainingTargetIndex: -1, epochLength: 4.0f,
                frequencies: new float[] { 8f, 10f }
            );
            Assert.AreEqual(0, _writer.ResolveTriggerByte(marker));
        }


        [Test]
        public void SimpleEncoding_WhenP300Target_ThenReturnsTargetByte()
        {
            _writer.UseSimpleTargetEncoding = true;
            _writer.TargetByte = 1;
            var marker = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: 0
            );
            Assert.AreEqual(1, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void SimpleEncoding_WhenP300NonTarget_ThenReturnsNonTargetByte()
        {
            _writer.UseSimpleTargetEncoding = true;
            _writer.NonTargetByte = 2;
            var marker = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: 1
            );
            Assert.AreEqual(2, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void SimpleEncoding_WhenMultiFlashContainsTarget_ThenReturnsTargetByte()
        {
            _writer.UseSimpleTargetEncoding = true;
            _writer.TargetByte = 1;
            var marker = new MultiFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 2,
                stimulusIndices: new int[] { 1, 2, 3 }
            );
            Assert.AreEqual(1, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void SimpleEncoding_WhenMITraining_ThenReturnsTargetByte()
        {
            _writer.UseSimpleTargetEncoding = true;
            _writer.TargetByte = 1;
            var marker = new MIEventMarker(stateCount: 2, trainingTargetIndex: 0, epochLength: 2.0f);
            Assert.AreEqual(1, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void SimpleEncoding_WhenMIClassification_ThenReturnsNonTargetByte()
        {
            _writer.UseSimpleTargetEncoding = true;
            _writer.NonTargetByte = 2;
            var marker = new MIEventMarker(stateCount: 2, trainingTargetIndex: -1, epochLength: 2.0f);
            Assert.AreEqual(2, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void SimpleEncoding_WhenStatusMarker_ThenIgnoresSimpleEncoding()
        {
            _writer.UseSimpleTargetEncoding = true;
            _writer.TrialStartedByte = 99;
            Assert.AreEqual(99, _writer.ResolveTriggerByte(new TrialStartedMarker()));
        }


        [Test]
        public void SetCustomTriggerMap_WhenSet_ThenOverridesResolveTriggerByte()
        {
            var custom = new System.Collections.Generic.Dictionary<string, byte>
            {
                { nameof(TrialStartedMarker), 77 }
            };
            _writer.SetCustomTriggerMap(custom);
            Assert.AreEqual(77, _writer.ResolveTriggerByte(new TrialStartedMarker()));
        }

        [Test]
        public void SetCustomTriggerMap_WhenNull_ThenRevertsToFieldDefaults()
        {
            _writer.TrialStartedByte = 240;
            var custom = new System.Collections.Generic.Dictionary<string, byte>
            {
                { nameof(TrialStartedMarker), 77 }
            };
            _writer.SetCustomTriggerMap(custom);
            _writer.SetCustomTriggerMap(null);
            Assert.AreEqual(240, _writer.ResolveTriggerByte(new TrialStartedMarker()));
        }

        [Test]
        public void SetCustomTriggerMap_WhenTypeNotInMap_ThenFallsThroughToDefault()
        {
            var custom = new System.Collections.Generic.Dictionary<string, byte>
            {
                { nameof(TrialStartedMarker), 77 }
            };
            _writer.SetCustomTriggerMap(custom);
            Assert.AreEqual(241, _writer.ResolveTriggerByte(new TrialEndsMarker()));
        }


        [Test]
        public void RegisterStimulusOverride_WhenSet_ThenOverridesDefaultByte()
        {
            _writer.RegisterStimulusOverride(2, 99);
            var marker = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: 2
            );
            Assert.AreEqual(99, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void RegisterStimulusOverride_WhenNotOverridden_ThenUsesDefault()
        {
            _writer.RegisterStimulusOverride(2, 99);
            var marker = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: 3
            );
            Assert.AreEqual(4, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void ClearStimulusOverrides_WhenCleared_ThenRevertsToDefault()
        {
            _writer.RegisterStimulusOverride(2, 99);
            _writer.ClearStimulusOverrides();
            var marker = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: 2
            );
            Assert.AreEqual(3, _writer.ResolveTriggerByte(marker));
        }

        [Test]
        public void UnregisterStimulusOverride_WhenUnregistered_ThenRevertsSpecificIndex()
        {
            _writer.RegisterStimulusOverride(2, 99);
            _writer.RegisterStimulusOverride(3, 88);
            _writer.UnregisterStimulusOverride(2);
            var marker2 = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: 2
            );
            var marker3 = new SingleFlashP300EventMarker(
                presenterCount: 6, trainingTargetIndex: 0, stimulusIndex: 3
            );
            Assert.AreEqual(3, _writer.ResolveTriggerByte(marker2));
            Assert.AreEqual(88, _writer.ResolveTriggerByte(marker3));
        }


        [Test]
        public void FakeMode_WhenConnected_ThenSendMarkerTriggerSendsPulseAndReset()
        {
            _writer.FakeMode = true;
            _writer.Connect();
            _writer.SendMarkerTrigger(new TrialStartedMarker());
            Assert.AreEqual(0, _writer.LastFakeByteSent);
            Assert.AreEqual(2, _writer.FakeBytesWritten);
        }

        [Test]
        public void FakeMode_WhenConnected_ThenSendPulseSendsValueThenZero()
        {
            _writer.FakeMode = true;
            _writer.Connect();
            _writer.SendPulse(42);
            Assert.AreEqual(0, _writer.LastFakeByteSent);
            Assert.AreEqual(2, _writer.FakeBytesWritten);
        }

        [Test]
        public void FakeMode_WhenDisconnected_ThenSendsZeroByte()
        {
            _writer.FakeMode = true;
            _writer.Connect();
            _writer.Disconnect();
            Assert.AreEqual(0, _writer.LastFakeByteSent);
            Assert.AreEqual(1, _writer.FakeBytesWritten);
        }


        [Test]
        public void SerialMarkerWriter_IsAssignableFrom_MarkerWriter()
        {
            Assert.IsTrue(typeof(MarkerWriter).IsAssignableFrom(typeof(SerialMarkerWriter)));
        }

        [Test]
        public void SerialMarkerWriter_WhenAddedToGameObject_ThenFoundAsMarkerWriter()
        {
            var go = new UnityEngine.GameObject("TestSMW");
            try
            {
                go.AddComponent<SerialMarkerWriter>();
                Assert.IsNotNull(go.GetComponent<MarkerWriter>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
