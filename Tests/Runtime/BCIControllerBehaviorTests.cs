using System;
using System.Collections;
using System.Collections.Generic;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using BCIEssentials.Utilities;
using NUnit.Framework;
using BCIEssentials.Tests.TestResources;
using UnityEngine;
using UnityEngine.TestTools;
using LogAssert = BCIEssentials.Tests.TestResources.LogAssert;
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
            LogTestName();
            yield return LoadDefaultSceneAsync();

            _testController = CreateController();
            _testControllerObject = _testController.gameObject;
            _behavior = _testControllerObject.AddComponent<EmptyBCIControllerBehavior>();

            yield return null;
        }

        [Test]
        public void WhenRegisterWithControllerInstance_ThenBehaviorRegistered()
        {
            _testControllerObject.SetActive(true);
            _behavior.RegisterWithControllerInstance();

            Assert.IsTrue(BCIController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [Test]
        public void WhenNoControllerInstanceAndRegisterBehavior_ThenCompletesWithError()
        {
            LogAssert.ExpectStartingWith(LogType.Error, "No BCI Controller instance set");

            _behavior.RegisterWithControllerInstance();

            Assert.IsFalse(BCIController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [Test]
        public void WhenUnregisterFromControllerInstance_ThenBehaviorUnregistered()
        {
            _testController.Initialize();

            _behavior.UnregisterFromControllerInstance();

            Assert.IsFalse(BCIController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [UnityTest]
        public IEnumerator OnStartAndSelfRegister_ThenBehaviorRegistered()
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selfRegister = true
            });

            _testControllerObject.SetActive(true);
            yield return null;

            Assert.IsTrue(BCIController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [UnityTest]
        public IEnumerator OnStartAndSelfRegisterAsActive_ThenBehaviorRegisteredAndSetToActive()
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selfRegisterAsActive = true
            });

            _testControllerObject.SetActive(true);
            yield return null;

            Assert.AreEqual(_behavior.BehaviorType, _testController.ActiveBehavior.BehaviorType);
        }

        [UnityTest]
        public IEnumerator OnStartAndNotSelfRegister_ThenBehaviorNotRegistered()
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selfRegister = false
            });
            _testControllerObject.SetActive(true);
            yield return null;

            Assert.IsFalse(BCIController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [UnityTest]
        public IEnumerator OnDestroyAndSelfRegister_ThenBehaviorUnregistered()
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selfRegister = true
            });
            _testControllerObject.SetActive(true);
            yield return null;

            Object.DestroyImmediate(_behavior);

            Assert.IsFalse(BCIController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [Test]
        [TestCase(-1)]
        [TestCase(5)]
        [TestCase(500)]
        public void WhenInitializeWithValidTargetFrameRate_ThenTargetFrameRateSet(int targetFrameRate)
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                targetFrameRate = targetFrameRate
            });

            _behavior.Initialize(null, null);

            Assert.AreEqual(targetFrameRate, Application.targetFrameRate);

            Application.targetFrameRate = -1;
        }

        [Test]
        [TestCase(0)]
        [TestCase(-5)]
        [TestCase(-500)]
        public void WhenInitializeWithInvalidTargetFrameRate_ThenTargetFrameRateNotSet(int targetFrameRate)
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                targetFrameRate = targetFrameRate
            });

            _behavior.Initialize(null, null);

            Assert.AreNotEqual(targetFrameRate, Application.targetFrameRate);

            Application.targetFrameRate = -1;
        }

        [Test]
        public void WhenInitializeAndRequiresMatrixSetUp_ThenMatrixSetUp()
        {
            var matrix = _testControllerObject.AddComponent<MatrixSetup>();
            var spo = AddSPOToScene();
            matrix.Initialize(spo, 2, 2, Vector2.one);
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                setupRequired = true,
                setup = matrix
            });

            _behavior.Initialize(null, null);

            //Matrix generated + original
            Assert.AreEqual(5, Object.FindObjectsOfType<SPO>().Length);
        }

        [Test]
        public void WhenInitialized_HotKeysEnabled()
        {
            _testController.Initialize();
            Assert.IsTrue(_behavior._hotkeysEnabled);
        }

        [UnityTest]
        public IEnumerator WhenCleanUp_ThenMatrixDestroyed()
        {
            var matrix = _testControllerObject.AddComponent<MatrixSetup>();
            var spo = AddSPOToScene();
            matrix.Initialize(spo, 2, 2, Vector2.one);
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                setup = matrix
            });
            matrix.SetUpMatrix();

            _behavior.CleanUp();
            yield return null;

            //Only original
            Assert.AreEqual(1, Object.FindObjectsOfType<SPO>().Length);
        }

        [UnityTest]
        public IEnumerator WhenCleanUpAndStimulusRunning_ThenStimulusRunStopped()
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selfRegisterAsActive = true
            });
            _testControllerObject.SetActive(true);
            yield return null; //Wait for monobehaviors
            _behavior.StartStimulusRun();

            _behavior.CleanUp();

            Assert.IsFalse(_behavior.StimulusRunning);
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

            Assert.AreEqual(1, _behavior.SelectableSPOs.Count);
            UnityEngine.Assertions.Assert.AreEqual(included, _behavior.SelectableSPOs[0]);
        }

        [Test]
        public void WhenPopulateObjectListWithPredefinedMethod_ThenObjectListPopulatedWithExistingValues()
        {
            var existingSPOs = new List<SPO>
            {
                AddSPOToScene(),
                AddSPOToScene(),
                AddSPOToScene()
            };
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selectableSPOs = existingSPOs
            });

            _behavior.PopulateObjectList(SpoPopulationMethod.Children);

            Assert.AreEqual(existingSPOs.Count, _behavior.SelectableSPOs.Count);
            UnityEngine.Assertions.Assert.AreEqual(existingSPOs[0], _behavior.SelectableSPOs[0]);
            UnityEngine.Assertions.Assert.AreEqual(existingSPOs[1], _behavior.SelectableSPOs[1]);
            UnityEngine.Assertions.Assert.AreEqual(existingSPOs[2], _behavior.SelectableSPOs[2]);
        }

        [Test]
        public void WhenPopulateObjectListWithChildrenMethod_ThenObjectListPopulatedWithExistingValues()
        {
            var existingSPOs = new List<SPO>
            {
                AddSPOToScene(),
                AddSPOToScene(),
                AddSPOToScene()
            };
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selectableSPOs = existingSPOs
            });

            _behavior.PopulateObjectList(SpoPopulationMethod.Children);

            Assert.AreEqual(existingSPOs.Count, _behavior.SelectableSPOs.Count);
            UnityEngine.Assertions.Assert.AreEqual(existingSPOs[0], _behavior.SelectableSPOs[0]);
            UnityEngine.Assertions.Assert.AreEqual(existingSPOs[1], _behavior.SelectableSPOs[1]);
            UnityEngine.Assertions.Assert.AreEqual(existingSPOs[2], _behavior.SelectableSPOs[2]);
        }
    }

    public class BCIControllerBehaviorTests_StimulusRunTests : PlayModeTestRunnerBase
    {
        private BCIController _testController;
        private BCIControllerBehavior _behavior;
        private LSLMarkerStream _testMarkerStream;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController();
            _testController.Initialize();

            _behavior = AddComponent<EmptyBCIControllerBehavior>();
            _behavior.RegisterWithControllerInstance(true);

            _testMarkerStream = _testController.GetComponent<LSLMarkerStream>();

            //Set marker stream to something unique each test to avoid interference
            _testMarkerStream.StreamName = $"{Guid.NewGuid()}";

            //Initialize Stream here so we can resolve it with the listener
            _testMarkerStream.InitializeStream();
            yield return null;
        }

        [Test]
        public void WhenStartStimulusRun_ThenStimulusRunning()
        {
            _behavior.StartStimulusRun();

            Assert.True(_behavior.StimulusRunning);
        }

        [UnityTest]
        public IEnumerator WhenStartStimulusRunAndSendConstantMarkers_ThenMarkerSentDuringRun()
        {
            var markerCount = (_behavior.windowLength + _behavior.interWindowInterval) * 3;
            var markerListener = AddComponent<LSLResponseStream>(rs =>
            {
                rs.Connect(_testMarkerStream.StreamName);
            });

            _behavior.StartStimulusRun(true);
            yield return new WaitForSecondsRealtime(markerCount);

            var responses = markerListener.GetResponses();

            Assert.AreEqual(markerCount + 1, responses.Length);
        }

        [UnityTest]
        public IEnumerator WhenStimulusRunningAndStartStimulusRun_ThenStimulusStoppedThenStarted()
        {
            var markerListener = AddComponent<LSLResponseStream>(rs =>
            {
                rs.Connect(_testMarkerStream.StreamName);
            });

            _behavior.StartStimulusRun(false);
            _behavior.StartStimulusRun(false);
            yield return null;
            var responses = markerListener.GetResponses();

            Assert.AreEqual(3, responses.Length);
            Assert.AreEqual("Trial Started", responses[0]);
            Assert.AreEqual("Trial Ends", responses[1]);
            Assert.AreEqual("Trial Started", responses[2]);
            Assert.True(_behavior.StimulusRunning);
        }

        [Test]
        public void WhenStopStimulusRun_ThenStimulusStopped()
        {
            _behavior.StartStimulusRun();

            _behavior.StopStimulusRun();

            Assert.False(_behavior.StimulusRunning);
        }
    }

    public class BCIControllerBehaviorTests_SelectObjectTests : PlayModeTestRunnerBase
    {
        private BCIController _testController;
        private GameObject _testControllerObject;
        private BCIControllerBehavior _behavior;
        private List<SPO> _spos;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController();
            _testController.Initialize();

            _behavior = AddComponent<EmptyBCIControllerBehavior>();
            _behavior.RegisterWithControllerInstance(true);

            _spos = new List<SPO>
            {
                AddSPOToScene(),
                AddSPOToScene(),
                AddSPOToScene(),
            };

            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selectableSPOs = _spos
                
            });
            Debug.Log("Number of SPOs: " + _spos.Count);
            yield return null;
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void WhenSelectSPO_ThenSPOSelected(int spoIndex)
        {
            _behavior.SelectSPO(spoIndex);

            Assert.AreEqual(_spos[spoIndex], _behavior.LastSelectedSPO);
        }

        //Setup test to check if the SPO Object ID is correctly unique for objects when PopulateObject method is used
        [UnityTest]
        public IEnumerator WhenPopulateObjectList_PopulateUniqueSPOIDs()
        {
            //Assert that the SPOs have same ID before Populating Object LIst
            Assert.AreEqual(_spos[0].ObjectID, _spos[1].ObjectID);

            _behavior.PopulateObjectList();
            yield return null;
            //Assert now that the SPO IDs are unique
            Assert.AreNotEqual(_spos[0].ObjectID, _spos[1].ObjectID);

        }

        [UnityTest]
        public IEnumerator WhenSelectSPOAndStopStimulusRun_ThenSPOSelectedAndStimulusRunEnded()
        {
            _behavior.StartStimulusRun();
            yield return null;
            _behavior.SelectSPO(0, true);

            Assert.False(_behavior.StimulusRunning);
        }

        [UnityTest]
        public IEnumerator WhenSelectSPOAtEndOfRun_ThenSPOSelected()
        {
            _behavior.StartStimulusRun();
            _behavior.SelectSPOAtEndOfRun(0);
            yield return null;
            _behavior.StopStimulusRun();
            yield return null;

            Assert.AreEqual(_spos[0], _behavior.LastSelectedSPO);
        }

        [UnityTest]
        public IEnumerator WhenSelectSPOAtEndOfRunAndSpoSelectedDuringRun_ThenSpoSelectedNotSelected()
        {
            _behavior.StartStimulusRun();
            yield return null;
            _behavior.SelectSPOAtEndOfRun(0);
            yield return null;
            _behavior.SelectSPO(1);
            _behavior.StopStimulusRun();

            UnityEngine.Assertions.Assert.AreEqual(_spos[1], _behavior.LastSelectedSPO);
        }
    }

    public class BCIControllerBehaviorTests_TrainingTests : PlayModeTestRunnerBase
    {
        private BCIController _testController;
        private BCIControllerBehavior _behavior;
        private List<SPO> _spos;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController();
            _testController.Initialize();
            
            _behavior = AddComponent<EmptyBCIControllerBehavior>();
            
            _spos = new List<SPO>
            {
                AddSPOToScene(),
                AddSPOToScene(),
                AddSPOToScene(),
            };

            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selectableSPOs = _spos
            });

            yield return null;
            
            BCIController.ChangeBehavior(_behavior.BehaviorType);
            
            _spos = new List<SPO>
            {
                AddSPOToScene(),
                AddSPOToScene(),
                AddSPOToScene(),
            };

            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selectableSPOs = _spos
                
            });
            Debug.Log("Number of SPOs: " + _spos.Count);
            yield return null;
        }

        [Test]
        [TestCase(BCITrainingType.Automated)]
        [TestCase(BCITrainingType.User)]
        [TestCase(BCITrainingType.Iterative)]
        [TestCase(BCITrainingType.Single)]
        public void WhenStartTraining_ThenTrainingRun(BCITrainingType trainingType)
        {
            _behavior.StartTraining(trainingType);
            
            Assert.IsTrue(_behavior.TrainingRunning);
            Assert.AreEqual(trainingType, _behavior.CurrentTrainingType);
        }

        [Test]
        public void WhenStartTrainingAndTrainingTypeNone_ThenNoTrainingRun()
        {
            _behavior.StartTraining(BCITrainingType.None);
            
            Assert.False(_behavior.TrainingRunning);
            Assert.AreEqual(BCITrainingType.None, _behavior.CurrentTrainingType);
        }

        [Test]
        public void WhenStimulusRunningAndStartTraining_ThenStimulusRunStopped()
        {
            _behavior.StartStimulusRun();
            
            _behavior.StartTraining(BCITrainingType.None);
            
            Assert.False(_behavior.StimulusRunning);
        }

        [Test]
        public void WhenTrainingRunningAndRequestDifferentTraining_ThenNewTrainingStarted()
        {
            _behavior.StartTraining(BCITrainingType.Automated);
            Assert.AreEqual(BCITrainingType.Automated, _behavior.CurrentTrainingType);
            
            _behavior.StartTraining(BCITrainingType.Iterative);
            Assert.AreEqual(BCITrainingType.Iterative, _behavior.CurrentTrainingType);
        }
    }

    #region P300ControllerBehaviorTests
    //This is where the P300ControllerBehavior tests will be included and defined.

    //TODO: Add test for P300 Graph partitioning

        public class BCIP300ControllerBehavior_SpoGraphTests : PlayModeTestRunnerBase
     {
        private BCIController _testController;
        private GameObject _testControllerObject;
        private P300ControllerBehavior _behavior;
        private const string MyTag = "BCI";

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            LogTestName();
            yield return LoadDefaultSceneAsync();

            _testController = CreateController();
            _testControllerObject = _testController.gameObject;
            _behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator WhenGetGameSPOsInCameraView_ThenReturnsVisibleTaggedObjects()
        {
            // Arrange
            var visibleSPOs = new List<SPO>
            {
                CreateSPO(new Vector3(0, 0, 0)),
                CreateSPO(new Vector3(1, 0, 0)),
                CreateSPO(new Vector3(0, 1, 0))
            };

            var invisibleSPO = CreateSPO(new Vector3(0, 1000, 0));

            // Ensure a camera is available and tagged as "MainCamera"
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = new GameObject("MainCamera").AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
            }

            // Place camera so all SPOs are in view
            mainCamera.transform.position = new Vector3(0, 0, -10);

            // Act
            var result = _behavior.GetGameSPOsInCameraView();

            // Assert
            Assert.AreEqual(visibleSPOs.Count, result.Count, "The number of visible SPOs should match the expected count.");

            foreach (var spo in visibleSPOs)
            {
                Assert.Contains(spo.gameObject, result, $"SPO {spo.name} should be included in the visible SPOs list.");
            }

            Assert.IsFalse(result.Contains(invisibleSPO.gameObject), $"SPO {invisibleSPO.name} should not be included in the visible SPOs list.");

            yield return null;
        }

        private SPO CreateSPO(Vector3 position)
        {
            var spoGameObject = new GameObject("SPO");
            spoGameObject.tag = MyTag;
            spoGameObject.transform.position = position;
            var spo = spoGameObject.AddComponent<SPO>();
            spo.Selectable = true;
            spoGameObject.AddComponent<MeshRenderer>(); // Add a renderer to make it visible
            return spo;
        }
    }
    
    #endregion
}