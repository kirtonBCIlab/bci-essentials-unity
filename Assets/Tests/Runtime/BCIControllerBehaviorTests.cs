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
        private class MockBCIControllerBehavior : BCIControllerBehavior
        {
            public override BehaviorType BehaviorType => BehaviorType.Unset;
        }

        private BCIController _testController;
        private GameObject _testControllerObject;
        private BCIControllerBehavior _behavior;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController(false, true);
            _testControllerObject = _testController.gameObject;
            _behavior = _testControllerObject.AddComponent<MockBCIControllerBehavior>();

            yield return null;
        }

        [Test]
        public void OnStart_ThenBehaviorRegistered()
        {
            var isRegistered = _testController.HasBehaviorOfType<MockBCIControllerBehavior>();
            Assert.IsTrue(isRegistered);
        }

        [Test]
        public void OnDestroy_ThenBehaviorUnregistered()
        {
            Object.DestroyImmediate(_behavior);

            var isRegistered = _testController.HasBehaviorOfType<MockBCIControllerBehavior>();
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
            _behavior.PopulateObjectList();

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
        public void WhenPopulateObjectListWithChildrenMethod_THenThrows()
        {
            Assert.Throws<NotImplementedException>(
                () => { _behavior.PopulateObjectList(SpoPopulationMethod.Children); });
        }

        [UnityTest]
        public IEnumerator WhenRunStimulus_ThenSPOsTurnedOnAndOff()
        {
            var enableStimulus = AddCoroutineRunner(DelayForFrames(5, () => _behavior.stimOn = false), destroyOnComplete:false);
            var runStimulus = AddCoroutineRunner(_behavior.Stimulus(), destroyOnComplete:false);
            
            var mockSpo = new GameObject().AddComponent<MockSPO>();
            SetField(_behavior, "objectList", new List<SPO>{ mockSpo });
            
            int turnedOnCount = 0;
            mockSpo.TurnOnAction = () =>
            {
                ++turnedOnCount;
            };
            
            int turnedOffCount = 0;
            mockSpo.TurnOffAction = () =>
            {
                ++turnedOffCount;
            };
            
            
            //Run Test
            _behavior.stimOn = true;
            enableStimulus.StartRun();
            runStimulus.StartRun();
            yield return new WaitWhile(()=> runStimulus.IsRunning);
            
            
            Assert.AreEqual(5, turnedOnCount);
            Assert.AreEqual(1, turnedOffCount);
        }

        [UnityTest]
        public IEnumerator WhenSendMarkers_ThenMarkerSentInIntervals()
        {
            var testDurationSeconds = 6;
            _testController.ChangeBehavior(BehaviorType.Unset);
            _behavior.stimOn = true;
            _behavior.windowLength = 1;
            _behavior.interWindowInterval = 1;
            
            var streamListener = AddComponent<LSLResponseStream>();
            streamListener.value = "UnityMarkerStream";
            var streamResponses = new List<string[]>();
            
            var enableStimulus = AddCoroutineRunner(DelayForSeconds(testDurationSeconds, ()=> _behavior.stimOn = false), destroyOnComplete:false);
            var responseListener = AddCoroutineRunner(ListenForMarkerStreams(streamListener, streamResponses), destroyOnComplete:false);
            var runSendMarkers = AddCoroutineRunner(_behavior.SendMarkers(), destroyOnComplete:false);
            
            //Run Test
            responseListener.StartRun();
            enableStimulus.StartRun();
            runSendMarkers.StartRun();
            yield return new WaitWhile(()=> runSendMarkers.IsRunning);
            responseListener.StopRun();

            Assert.AreEqual(testDurationSeconds / (_behavior.windowLength + _behavior.interWindowInterval ), streamResponses.Count);
        }

        [UnityTest]
        [TestCase("ping")]
        [TestCase("")]
        [TestCase("notaselection")]
        public IEnumerator WhenReceiveMarkersAndHasInvalidResponse_ThenNoSpoSelected(string markerValue)
        {
            var testDurationSeconds = 6;
            _testController.ChangeBehavior(BehaviorType.Unset);
            _behavior.windowLength = 1;
            _behavior.interWindowInterval = 1;

            int selectedIndex = -1;
            for (int i = 0; i < 5; i++)
            {
                var spo = AddSPOToScene<MockSPO>();
                spo.OnSelectionAction = () => selectedIndex = spo.myIndex;
            }
            
            var sendMarkers = AddCoroutineRunner(SendMockMarker(AddComponent<LSLMarkerStream>(), markerValue, 1));
            var receiveMarkers = AddCoroutineRunner(_behavior.ReceiveMarkers(), destroyOnComplete:false);
            AddCoroutineRunner(DelayForSeconds(testDurationSeconds, _behavior.StopAllCoroutines), destroyOnComplete:false);

            
            receiveMarkers.StartRun();
            sendMarkers.StartRun();
            yield return new WaitWhile(() => receiveMarkers.IsRunning);
            
            UnityEngine.Assertions.Assert.IsNull(selectedSPO);
        }
        
        [UnityTest]
        [TestCase(1)]
        [TestCase(5)]
        public IEnumerator WhenReceiveMarkersAndHasValidResponse_ThenNoSpoSelected(int markerValue)
        {
            var testDurationSeconds = 6;
            _testController.ChangeBehavior(BehaviorType.Unset);
            _behavior.windowLength = 1;
            _behavior.interWindowInterval = 1;

            int selectedIndex = -1;
            for (int i = 0; i < 5; i++)
            {
                var spo = AddSPOToScene<MockSPO>();
                spo.OnSelectionAction = () => selectedIndex = spo.myIndex;
            }
            
            var sendMarkers = AddCoroutineRunner(SendMockMarker(AddComponent<LSLMarkerStream>(), markerValue.ToString(), 1));
            var receiveMarkers = AddCoroutineRunner(_behavior.ReceiveMarkers(), destroyOnComplete:false);
            AddCoroutineRunner(DelayForSeconds(testDurationSeconds, _behavior.StopAllCoroutines), destroyOnComplete:false);

            
            receiveMarkers.StartRun();
            sendMarkers.StartRun();
            yield return new WaitWhile(() => receiveMarkers.IsRunning);
            
            Assert.AreEqual(markerValue, selectedIndex);
        }

        private IEnumerator DelayForFrames(int frameCount, Action onRun)
        {
            int framesRan = 0;

            while (framesRan < frameCount)
            {
                ++framesRan;
                Debug.Log(framesRan);
                yield return null;
            }
            
            onRun?.Invoke();
        }

        private IEnumerator DelayForSeconds(float seconds, Action onRun)
        {
            yield return new WaitForSecondsRealtime(seconds);
            onRun?.Invoke();
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

        private IEnumerator SendMockMarker(LSLMarkerStream markerStream, string value,
            float intervalSeconds = 0.1f)
        {
            while (true)
            {
                //markerStream.
            }
        }
    }
}