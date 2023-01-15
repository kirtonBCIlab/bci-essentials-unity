using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BCIEssentials.LSL;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using BCIEssentials.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests
{
    public class MIControllerTests : PlayModeTestRunnerBase
    {
        private MIController _testController;
            
        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();
            
            var gameObject = new GameObject();
            gameObject.SetActive(false);

            gameObject.AddComponent<MatrixSetup>();
            gameObject.AddComponent<LSLMarkerStream>();
            gameObject.AddComponent<LSLResponseStream>();

            _testController = gameObject.AddComponent<MIController>();
            _testController.stimOn = true;
            _testController.gameObject.SetActive(true);
            yield return null;
        }

        [TearDown]
        public void TestCleanup()
        {
            foreach (var sceneObjects in Object.FindObjectsOfType<GameObject>())
            {
                Object.DestroyImmediate(sceneObjects);
            }
        }

        
        [Test]
        public void WhenPopulateObjectListWithTagMethod_ThenObjectListPopulated()
        {
            CreateSpoObjects(out var noComponent, out var noTag, out var falseIncludeMe, out var included);

            _testController.PopulateObjectList("tag");

            Assert.AreEqual(1, _testController.objectList.Count);
            UnityEngine.Assertions.Assert.AreEqual(included.gameObject, _testController.objectList[0]);
            Assert.AreEqual(0, _testController.objectList[0].GetComponent<SPO>().myIndex);
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
            Assert.AreEqual(0, _testController.objectList[0].GetComponent<SPO>().myIndex);
            UnityEngine.Assertions.Assert.AreEqual(included, _testController.objectList[1]);
            Assert.AreEqual(1, _testController.objectList[1].GetComponent<SPO>().myIndex);
        }

        [Test]
        public void WhenPopulateObjectListWithChildrenMethod_ThenObjectListPopulated()
        {
            CreateSpoObjects(out var noComponent, out var noTag, out var falseIncludeMe, out var included);
            _testController.PopulateObjectList("children");
            
            Assert.AreEqual(0, _testController.objectList.Count);
        }
        
        [UnityTest]
        public IEnumerator WhenSendMarkers_ThenMarkerSentInIntervals()
        {
            var testDurationSeconds = 6;
            var streamListener = AddComponent<LSLResponseStream>();
            streamListener.value = "UnityMarkerStream";
            var streamResponses = new List<string[]>();

            var enableStimulusRunner =
                AddCoroutineRunner(DelayForSeconds(testDurationSeconds, () => _testController.stimOn = false));
            var listenForMarkerRunner = AddCoroutineRunner(ListenForMarkerStreams(streamListener, streamResponses));
            var behaviorSendMarkers = AddCoroutineRunner(_testController.SendMarkers());

            //Run Test
            listenForMarkerRunner.StartRun();
            enableStimulusRunner.StartRun();
            behaviorSendMarkers.StartRun();
            yield return new WaitWhile(() => behaviorSendMarkers.IsRunning);

            Assert.AreEqual(testDurationSeconds / (_testController.windowLength + _testController.interWindowInterval),
                streamResponses.Count);
        }

        [UnityTest]
        public IEnumerator WhenDoIterativeTraining_ThenSPOsSelected()
        {
            _testController.numTrainingSelections = 2;
            var streamListener = AddComponent<LSLResponseStream>();
            streamListener.value = "UnityMarkerStream";
            var streamResponses = new List<string[]>();

            int spoCount = 2;
            int sposTrained = 0;
            for (int i = 0; i < spoCount; i++)
            {
                AddSPOToScene<MockSPO>().OnTrainTargetAction = () => ++sposTrained;
            }
            
            var behaviorRunner = AddCoroutineRunner(_testController.DoIterativeTraining());
            var listenForMarkerRunner = AddCoroutineRunner(ListenForMarkerStreams(streamListener, streamResponses));

            listenForMarkerRunner.StartRun();
            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.AreEqual(2, sposTrained);
            Assert.AreEqual(_testController.objectList.Count, spoCount);
            Assert.AreEqual(1, streamResponses.Count(r => !string.IsNullOrEmpty(r[0]) && r[0].Equals("Training Complete")));
        }

        private void CreateSpoObjects(out GameObject noComponent, out GameObject noTag, out GameObject falseIncludeMe, out GameObject included)
        {
            noComponent = new GameObject { tag = _testController.myTag };
            noTag = AddSPOToScene("").gameObject;
            falseIncludeMe = AddSPOToScene(includeMe: false).gameObject;
            included = AddSPOToScene().gameObject;
        }
        
        
        private IEnumerator ListenForMarkerStreams(LSLResponseStream responseStream, List<string[]> responses)
        {
            responseStream.ResolveResponse();
            yield return new WaitForEndOfFrame();

            while (true)
            {
                var response = responseStream.PullResponse(new string[1], 0);
                if (response.Length > 0 && !string.IsNullOrEmpty(response[0]))
                {
                    responses.Add(response);
                }

                yield return new WaitForSecondsRealtime(1 / Application.targetFrameRate);
            }
        }

    }
}