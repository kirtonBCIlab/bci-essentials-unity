using System;
using System.Collections;
using System.Collections.Generic;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using BCIEssentials.LSL;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using BCIEssentials.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests
{
    public class BCIControllerBehaviorTests : PlayModeTestRunnerBase
    {
        private BCIController _testController;
        private GameObject _testControllerObject;
        private BCIControllerBehavior _behavior;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController(false, true);
            _testControllerObject = _testController.gameObject;
            _behavior = _testControllerObject.AddComponent<EmptyBCIControllerBehavior>();

            yield return null;
        }

        [Test]
        public void OnStart_ThenBehaviorRegistered()
        {
            var isRegistered = _testController.HasBehaviorOfType<EmptyBCIControllerBehavior>();
            Assert.IsTrue(isRegistered);
        }

        [Test]
        public void OnDestroy_ThenBehaviorUnregistered()
        {
            Object.DestroyImmediate(_behavior);

            var isRegistered = _testController.HasBehaviorOfType<EmptyBCIControllerBehavior>();
            Assert.IsFalse(isRegistered);
        }

        [Test]
        public void WhenInitialize_ThenTargetFrameRateSet()
        {
            int previousFrameRate = Application.targetFrameRate;
            Application.targetFrameRate = 500;

            _behavior.Initialize(null, null);

            Assert.AreEqual(60, Application.targetFrameRate);

            Application.targetFrameRate = previousFrameRate;
        }

        [Test]
        public void WhenInitializeAndRequiresMatrixSetUp_ThenMatrixSetUp()
        {
            var matrix = _testControllerObject.AddComponent<MatrixSetup>();
            var spo = AddSPOToScene();
            matrix.Initialize(spo, 2, 2, Vector2.one);
            SetField(_behavior, "setupRequired", true);
            SetField(_behavior, "setup", matrix);

            _behavior.Initialize(null, null);

            //Matrix generated + original
            Assert.AreEqual(5, Object.FindObjectsOfType<SPO>().Length);
        }

        [UnityTest]
        public IEnumerator WhenCleanUp_ThenMatrixDestroyed()
        {
            var matrix = _testControllerObject.AddComponent<MatrixSetup>();
            var spo = AddSPOToScene();
            matrix.Initialize(spo, 2, 2, Vector2.one);
            SetField(_behavior, "setup", matrix);
            matrix.SetUpMatrix();

            _behavior.CleanUp();
            yield return null;

            //Only original
            Assert.AreEqual(1, Object.FindObjectsOfType<SPO>().Length);
        }

        [Test]
        public void WhenPopulateObjectListWithTagMethod_ThenObjectListPopulated()
        {
            var noComponent = new GameObject { tag = _behavior.myTag };
            var noTag = AddSPOToScene("");
            var falseIncludeMe = AddSPOToScene(includeMe: false);
            var included = AddSPOToScene();

            //Deliberate use of default
            _behavior.PopulateObjectList(SpoPopulationMethod.Tag);

            Assert.AreEqual(1, _behavior.ObjectList.Count);
            UnityEngine.Assertions.Assert.AreEqual(included, _behavior.ObjectList[0]);
        }

        [Test]
        public void WhenPopulateObjectListWithPredefinedMethod_ThenObjectListPopulated()
        {
            var noComponent = new GameObject { tag = _behavior.myTag };
            var noTag = AddSPOToScene("");
            var included = AddSPOToScene();
            var falseIncludeMe = AddSPOToScene(includeMe: false);
            SetField(_behavior, "objectList", new List<SPO>
            {
                noTag,
                included,
                falseIncludeMe
            });

            _behavior.PopulateObjectList(SpoPopulationMethod.Predefined);

            Assert.AreEqual(3, _behavior.ObjectList.Count);
            UnityEngine.Assertions.Assert.AreEqual(noTag, _behavior.ObjectList[0]);
            UnityEngine.Assertions.Assert.AreEqual(included, _behavior.ObjectList[1]);
            UnityEngine.Assertions.Assert.AreEqual(falseIncludeMe, _behavior.ObjectList[2]);
        }

        [Test]
        public void WhenPopulateObjectListWithChildrenMethod_ThenThrows()
        {
            Assert.Throws<NotImplementedException>(
                () => { _behavior.PopulateObjectList(SpoPopulationMethod.Children); });
        }

        [UnityTest]
        public IEnumerator WhenRunStimulus_ThenSPOsTurnedOnAndOff()
        {
            var enableStimulus = AddCoroutineRunner(DelayForFrames(5, () => _behavior.stimOn = false));
            var runStimulus = AddCoroutineRunner(_behavior.Stimulus());

            var mockSpo = new GameObject().AddComponent<MockSPO>();
            SetField(_behavior, "objectList", new List<SPO> { mockSpo });

            int turnedOnCount = 0;
            mockSpo.TurnOnAction = () => { ++turnedOnCount; };

            int turnedOffCount = 0;
            mockSpo.TurnOffAction = () => { ++turnedOffCount; };


            //Run Test
            _behavior.stimOn = true;
            enableStimulus.StartRun();
            runStimulus.StartRun();
            yield return new WaitWhile(() => runStimulus.IsRunning);


            Assert.AreEqual(5, turnedOnCount);
            Assert.AreEqual(1, turnedOffCount);
        }
    }

    public class BCIControllerBehaviorTests_WhenSendReceiveMarkers : PlayModeTestRunnerBase
    {
        private static (string, int)[] k_WhenReceiveMarkersTestMarkerValues =
        {
            ("", -1),
            ("ping", -1),
            ("notavalue", -1),
            ("500", -1),
            ("0", 0),
            ("1", 1),
            ("5", 5)
        };

        private BCIController _testController;
        private BCIControllerBehavior _behavior;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController(false, true);

            _behavior = _testController.gameObject.AddComponent<EmptyBCIControllerBehavior>();
            _behavior.stimOn = true;
            _behavior.windowLength = 1;
            _behavior.interWindowInterval = 1;

            _testController.RegisterBehavior(_behavior, true);
        }

        [TearDown]
        public void TestCleanup()
        {
            StopAllCoroutineRunners();
        }

        [UnityTest]
        public IEnumerator WhenSendMarkers_ThenMarkerSentInIntervals()
        {
            var testDurationSeconds = 6;
            var streamListener = AddComponent<LSLResponseStream>();
            streamListener.value = "UnityMarkerStream";
            var streamResponses = new List<string[]>();

            var enableStimulusRunner =
                AddCoroutineRunner(DelayForSeconds(testDurationSeconds, () => _behavior.stimOn = false));
            var listenForMarkerRunner = AddCoroutineRunner(ListenForMarkerStreams(streamListener, streamResponses));
            var behaviorSendMarkers = AddCoroutineRunner(_behavior.SendMarkers());

            //Run Test
            listenForMarkerRunner.StartRun();
            enableStimulusRunner.StartRun();
            behaviorSendMarkers.StartRun();
            yield return new WaitWhile(() => behaviorSendMarkers.IsRunning);

            Assert.AreEqual(testDurationSeconds / (_behavior.windowLength + _behavior.interWindowInterval),
                streamResponses.Count);
        }

        [UnityTest]
        public IEnumerator WhenReceiveMarkersWithResponseValues_ThenExpectedSpoSelected(
            [ValueSource(nameof(k_WhenReceiveMarkersTestMarkerValues))] (string, int) testValues)
        {
            var testDurationRunner = AddCoroutineRunner(DelayForSeconds(6, StopAllCoroutineRunners));
            var sendMarkerRunner = AddCoroutineRunner(WriteMockMarker(AddComponent<LSLMarkerStream>(), testValues.Item1, 1));
            var behaviorRunner = AddCoroutineRunner(_behavior.ReceiveMarkers());

            var selectedIndex = -1;
            for (var i = 0; i < 6; i++)
            {
                var spo = AddSPOToScene<MockSPO>();
                spo.OnSelectionAction = () => selectedIndex = spo.SelectablePoolIndex;
            }
            
            _behavior.PopulateObjectList();

            testDurationRunner.StartRun();
            sendMarkerRunner.StartRun();
            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.AreEqual(testValues.Item2, selectedIndex);
        }

        private IEnumerator ListenForMarkerStreams(LSLResponseStream responseStream, List<string[]> responses)
        {
            responseStream.ResolveResponse();
            yield return new WaitForEndOfFrame();

            while (true)
            {
                var response = responseStream.PullResponse(new string[1], 0);
                if (response.Length > 0 && !response[0].Equals(""))
                {
                    responses.Add(response);
                }

                yield return new WaitForSecondsRealtime(1 / Application.targetFrameRate);
            }
        }

        private IEnumerator WriteMockMarker(LSLMarkerStream markerStream, string value,
            float intervalSeconds = 0.1f)
        {
            markerStream.StreamName = "PythonResponse";
            markerStream.InitializeStream();

            while (true)
            {
                markerStream.Write(value);
                yield return new WaitForSeconds(intervalSeconds);
            }
        }
    }

    public class BCIControllerBehaviorTests_WhenDoTraining : PlayModeTestRunnerBase
    {
        private BCIController _testController;
        private BCIControllerBehavior _behavior;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController(false, true);
            _behavior = _testController.gameObject.AddComponent<EmptyBCIControllerBehavior>();
            _testController.RegisterBehavior(_behavior, true);
        }

        [UnityTest]
        public IEnumerator WhenDoTrainingForTrainingCount_ThenSPOsTrained()
        {
            var behaviorRunner = AddCoroutineRunner(_behavior.DoTraining());
            _behavior.numTrainingSelections = 2;

            int spoCount = 3;
            int sposTrained = 0;
            for (int i = 0; i < spoCount; i++)
            {
                AddSPOToScene<MockSPO>().OnTrainTargetAction = () => ++sposTrained;
            }

            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.AreEqual(2, sposTrained);
            Assert.AreEqual(_behavior.ObjectList.Count, spoCount);
        }

        [UnityTest]
        public IEnumerator WhenDoTrainingAndTrainTargetPersistent_ThenSPOsTrained()
        {
            var behaviorRunner = AddCoroutineRunner(_behavior.DoTraining());
            _behavior.numTrainingSelections = 1;
            _behavior.trainTargetPersistent = true;

            bool trainingOffCalled = false;
            AddSPOToScene<MockSPO>().OffTrainTargetAction = () => trainingOffCalled = true;

            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.IsTrue(trainingOffCalled);
        }

        [UnityTest]
        public IEnumerator WhenDoTrainingAndShamFeedback_ThenSPOsTrained()
        {
            var behaviorRunner = AddCoroutineRunner(_behavior.DoTraining());
            _behavior.numTrainingSelections = 1;
            _behavior.shamFeedback = true;

            bool onSelectedCalled = false;
            AddSPOToScene<MockSPO>().OnSelectionAction = () => onSelectedCalled = true;

            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.IsTrue(onSelectedCalled);
        }
    }
    
}