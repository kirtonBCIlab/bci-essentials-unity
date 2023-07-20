using System.Collections;
using System.Collections.Generic;
using BCIEssentials.LSLFramework;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using BCIEssentials.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests
{
    public class ControllerTests: PlayModeTestRunnerBase
    {
        private Controller _testController;
            
        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();
            _testController = CreateController();
        }

        [TearDown]
        public void TestCleanup()
        {
            foreach (var sceneObjects in Object.FindObjectsOfType<GameObject>())
            {
                Object.DestroyImmediate(sceneObjects);
            }
        }

        [UnityTest]
        public IEnumerator WhenStart_ThenComponentsAssigned()
        {
            _testController.gameObject.SetActive(true);
            yield return null;

            Assert.IsNotNull(_testController.setup);
            Assert.IsNotNull(_testController.marker);
            Assert.IsNotNull(_testController.response);
        }

        [UnityTest]
        public IEnumerator WhenStart_ThenTargetFrameRateSet()
        {
            _testController.refreshRate = 555;
            
            _testController.gameObject.SetActive(true);
            yield return null;

            Assert.AreEqual(555, Application.targetFrameRate);
        }

        [UnityTest]
        public IEnumerator WhenStartWithMatrixAndSetupRequired_ThenMatrixSetUp()
        {
            _testController.setupRequired = true;
            var spoPrefab = new GameObject("testSPO").AddComponent<SPO>();
            _testController.GetComponent<MatrixSetup>().Initialize(spoPrefab, 1, 1, Vector2.zero);
            
            _testController.gameObject.SetActive(true);
            yield return null;
            
            Assert.IsTrue(Object.FindObjectsOfType<SPO>().Length > 1);
        }

        [Test]
        public void WhenPopulateObjectListWithTagMethod_ThenObjectListPopulated()
        {
            CreateSpoObjects(out var noComponent, out var noTag, out var falseIncludeMe, out var included);

            _testController.PopulateObjectList("tag");

            Assert.AreEqual(1, _testController.objectList.Count);
            UnityEngine.Assertions.Assert.AreEqual(included.gameObject, _testController.objectList[0]);
            Assert.AreEqual(0, _testController.objectList[0].GetComponent<SPO>().SelectablePoolIndex);
        }

        [Test]
        public void WhenPopulateObjectListWithPredefinedMethod_ThenObjectListPopulated()
        {
            CreateSpoObjects(out var noComponent, out var noTag, out var falseIncludeMe, out var included);
            _testController.objectList = new List<GameObject>
            {
                noTag,
                falseIncludeMe,
                included
            };

            _testController.PopulateObjectList("predefined");

            Assert.AreEqual(2, _testController.objectList.Count);
            UnityEngine.Assertions.Assert.AreEqual(noTag, _testController.objectList[0]);
            Assert.AreEqual(0, _testController.objectList[0].GetComponent<SPO>().SelectablePoolIndex);
            UnityEngine.Assertions.Assert.AreEqual(included, _testController.objectList[1]);
            Assert.AreEqual(1, _testController.objectList[1].GetComponent<SPO>().SelectablePoolIndex);
        }

        [Test]
        public void WhenPopulateObjectListWithChildrenMethod_ThenObjectListPopulated()
        {
            CreateSpoObjects(out var noComponent, out var noTag, out var falseIncludeMe, out var included);
            _testController.PopulateObjectList("children");
            
            Assert.AreEqual(0, _testController.objectList.Count);
        }

        [UnityTest]
        public IEnumerator WhenStimulus_ThenSPOsTurnedOnAndOff()
        {
            var enableStimulus = AddCoroutineRunner(DelayForFrames(5, () => _testController.stimOn = false));
            var runStimulus = AddCoroutineRunner(_testController.Stimulus());

            var mockSpo = new GameObject().AddComponent<MockSPO>();
            _testController.objectList = new List<GameObject> { mockSpo.gameObject };

            int turnedOnCount = 0;
            mockSpo.StartStimulusEvent.AddListener(() => { ++turnedOnCount; });

            int turnedOffCount = 0;
            mockSpo.StopStimulusEvent.AddListener(() => { ++turnedOffCount; });


            //Run Test
            _testController.stimOn = true;
            enableStimulus.StartRun();
            runStimulus.StartRun();
            yield return new WaitWhile(() => runStimulus.IsRunning);


            Assert.AreEqual(5, turnedOnCount);
            Assert.AreEqual(1, turnedOffCount);
        }

        [UnityTest]
        public IEnumerator WhenSelectObjectAndStimOff_ThenObjectSelected()
        {
            var runStimulus = AddCoroutineRunner(_testController.SelectObject(0));
            var mockSpo = new GameObject().AddComponent<MockSPO>();
            bool wasSelected = false;
            mockSpo.OnSelectedEvent.AddListener(() => { wasSelected = true; });
            _testController.objectList = new List<GameObject> { mockSpo.gameObject };
            
            //Run Test
            runStimulus.StartRun();
            yield return new WaitWhile(() => runStimulus.IsRunning);
            
            Assert.IsTrue(wasSelected);
        }

        [UnityTest]
        public IEnumerator WhenSelectObjectAndStimOn_ThenObjectSelectedAfterStimEnds()
        {
            var enableStimulus = AddCoroutineRunner(DelayForFrames(5, () => _testController.stimOn = false));
            var runStimulus = AddCoroutineRunner(_testController.SelectObject(0));
            var mockSpo = new GameObject().AddComponent<MockSPO>();
            bool wasSelected = false;
            mockSpo.OnSelectedEvent.AddListener(() => { wasSelected = true; });
            _testController.objectList = new List<GameObject> { mockSpo.gameObject };
            _testController.stimOn = true;
            
            //Run Test
            enableStimulus.StartRun();
            runStimulus.StartRun();
            yield return new WaitWhile(() => runStimulus.IsRunning);
            
            Assert.IsTrue(wasSelected);
        }

        private static Controller CreateController(bool setActive = false)
        {
            var gameObject = new GameObject();
            gameObject.SetActive(false);

            gameObject.AddComponent<MatrixSetup>();
            gameObject.AddComponent<LSLMarkerStream>();
            gameObject.AddComponent<LSLResponseStream>();

            var controller = gameObject.AddComponent<Controller>();
            controller.myTag = "Player"; //Selected from list of default Unity tags
            
            if (setActive)
            {
                gameObject.SetActive(true);
            }

            return controller;
        }

        private void CreateSpoObjects(out GameObject noComponent, out GameObject noTag, out GameObject falseIncludeMe, out GameObject included)
        {
            noComponent = new GameObject { tag = _testController.myTag };
            noTag = AddSPOToScene("").gameObject;
            falseIncludeMe = AddSPOToScene(includeMe: false).gameObject;
            included = AddSPOToScene().gameObject;
        }
    }
    
    public class Controller_WhenSendReceiveMarkers : PlayModeTestRunnerBase
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

        private Controller _testController;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();
            
            var gameObject = new GameObject();
            gameObject.SetActive(false);

            gameObject.AddComponent<MatrixSetup>();
            gameObject.AddComponent<LSLMarkerStream>();
            gameObject.AddComponent<LSLResponseStream>();

            _testController = gameObject.AddComponent<Controller>();
            _testController.myTag = "Player"; //Selected from list of default Unity tags
            _testController.stimOn = true;
           _testController.windowLength = 1;
           _testController.interWindowInterval = 1;
           
           gameObject.SetActive(true);
           yield return null;
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
            streamListener.Connect("UnityMarkerStream");

            var enableStimulusRunner =
                AddCoroutineRunner(DelayForSeconds(testDurationSeconds, () => _testController.stimOn = false));
            var behaviorSendMarkers = AddCoroutineRunner(_testController.SendMarkers());

            //Run Test
            enableStimulusRunner.StartRun();
            behaviorSendMarkers.StartRun();
            yield return new WaitWhile(() => behaviorSendMarkers.IsRunning);

            var markersSent = testDurationSeconds / (_testController.windowLength + _testController.interWindowInterval);
            var responses = streamListener.GetResponses();
            Assert.AreEqual(responses.Length, markersSent);
        }

        [UnityTest]
        public IEnumerator WhenReceiveMarkersWithResponseValues_ThenExpectedSpoSelected(
            [ValueSource(nameof(k_WhenReceiveMarkersTestMarkerValues))] (string, int) testValues)
        {
            var testDurationRunner = AddCoroutineRunner(DelayForSeconds(6, StopAllCoroutineRunners));
            var sendMarkerRunner = AddCoroutineRunner(WriteMockMarker(AddComponent<LSLMarkerStream>(), testValues.Item1, 1));
            var behaviorRunner = AddCoroutineRunner(_testController.ReceiveMarkers());

            var selectedIndex = -1;
            for (var i = 0; i < 6; i++)
            {
                var spo = AddSPOToScene<MockSPO>();
                spo.OnSelectedEvent.AddListener(() => selectedIndex = spo.SelectablePoolIndex);
            }
            
            _testController.PopulateObjectList("tag");

            testDurationRunner.StartRun();
            sendMarkerRunner.StartRun();
            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.AreEqual(testValues.Item2, selectedIndex);
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

    public class ControllerTests_WhenDoTraining : PlayModeTestRunnerBase
    {
        private Controller _testController;
            
        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();
            
            var gameObject = new GameObject();
            gameObject.SetActive(false);

            gameObject.AddComponent<MatrixSetup>();
            gameObject.AddComponent<LSLMarkerStream>();
            gameObject.AddComponent<LSLResponseStream>();

            _testController = gameObject.AddComponent<Controller>();
            _testController.myTag = "Player"; //Selected from list of default Unity tags
            
            gameObject.SetActive(true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WhenDoTrainingForTrainingCount_ThenSPOsTrained()
        {
            var behaviorRunner = AddCoroutineRunner(_testController.DoTraining());
            _testController.numTrainingSelections = 2;

            int spoCount = 3;
            int sposTrained = 0;
            for (int i = 0; i < spoCount; i++)
            {
                AddSPOToScene<MockSPO>().OnTrainTargetAction = () => ++sposTrained;
            }

            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.AreEqual(2, sposTrained);
            Assert.AreEqual(_testController.objectList.Count, spoCount);
        }

        [UnityTest]
        public IEnumerator WhenDoTrainingAndTrainTargetPersistent_ThenSPOsTrained()
        {
            var behaviorRunner = AddCoroutineRunner(_testController.DoTraining());
            _testController.numTrainingSelections = 1;
            _testController.trainTargetPersistent = true;

            bool trainingOffCalled = false;
            AddSPOToScene<MockSPO>().OffTrainTargetAction = () => trainingOffCalled = true;

            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.IsTrue(trainingOffCalled);
        }

        [UnityTest]
        public IEnumerator WhenDoTrainingAndShamFeedback_ThenSPOsTrained()
        {
            var behaviorRunner = AddCoroutineRunner(_testController.DoTraining());
            _testController.numTrainingSelections = 1;
            _testController.shamFeedback = true;

            bool onSelectedCalled = false;
            AddSPOToScene<MockSPO>().OnSelectedEvent.AddListener(() => onSelectedCalled = true);

            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.IsTrue(onSelectedCalled);
        }
    }
}