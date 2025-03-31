using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using BCIEssentials.Tests.Utilities.LSLFramework;
using BCIEssentials.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests
{
    public class BCIControllerBehaviorTests : PlayModeTestRunnerBase
    {
        private BCIControllerInstance _testController;
        private GameObject _testControllerObject;
        private BCIControllerBehavior _behavior;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            LogTestName();
            yield return LoadDefaultSceneAsync();

            _testController = CreateControllerInstance();
            _testControllerObject = _testController.gameObject;
            _behavior = _testControllerObject.AddComponent<EmptyBCIControllerBehavior>();

            yield return null;
        }

        [Test]
        public void WhenRegisterWithControllerInstance_ThenBehaviorRegistered()
        {
            _testControllerObject.SetActive(true);
            _behavior.RegisterWithController();

            Assert.IsTrue(BCIController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [Test]
        public void WhenUnregisterFromControllerInstance_ThenBehaviorUnregistered()
        {
            _testController.Initialize();

            _behavior.UnregisterFromController();

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
        public void WhenInitializeAndRequiresFactorySetUp_ThenObjectsCreated()
        {
            var spo = AddSPOToScene();
            var spoFactory = SPOGridFactory.CreateInstance(spo, 2, 2, Vector2.one);

            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                FactorySetupRequired = true,
                _spoFactory = spoFactory
            });

            _behavior.Initialize(null, null);

            //Matrix generated + original
            Assert.AreEqual(5, CountComponentsInActiveScene<SPO>());
        }

        private int CountComponentsInActiveScene<T>() where T: Component
        {
            return SceneManager.GetActiveScene()
            .GetRootGameObjects().Sum
            (
                gameObject => gameObject
                .GetComponentsInChildren<T>().Length
            );
        }


        [UnityTest]
        public IEnumerator WhenCleanUp_ThenFactoryObjectsDestroyed()
        {
            var spo = AddSPOToScene();
            var spoFactory = SPOGridFactory.CreateInstance(spo, 2, 2, Vector2.one);

            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _spoFactory = spoFactory
            });
            spoFactory.CreateObjects();

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
            var noComponent = new GameObject { tag = _behavior.SPOTag };
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
        private BCIControllerInstance _testController;
        private BCIControllerBehavior _behavior;
        private LSLMarkerWriter _testMarkerStream;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateControllerInstance(setActive: true);
            _testController.Initialize();

            _behavior = AddComponent<EmptyBCIControllerBehavior>();
            _behavior.RegisterWithController(true);

            _testMarkerStream = _testController.GetComponent<LSLMarkerWriter>();

            //Set marker stream to something unique each test to avoid interference
            _testMarkerStream.StreamName = $"{Guid.NewGuid()}";

            //Initialize Stream here so we can resolve it with the listener
            _testMarkerStream.OpenStream();
            yield return null;
        }

        [Test]
        public void WhenStartStimulusRun_ThenStimulusRunning()
        {
            _behavior.StartStimulusRun();

            Assert.True(_behavior.StimulusRunning);
        }

        [UnityTest]
        public IEnumerator WhenStimulusRunningAndStartStimulusRun_ThenStimulusStoppedThenStarted()
        {
            var markerListener = AddComponent<LSLRawStreamReader>();
            markerListener.OpenStreamByName(_testMarkerStream.StreamName);

            _behavior.StartStimulusRun();
            _behavior.StartStimulusRun();
            yield return new WaitForSecondsRealtime(0.5f);
            var responses = markerListener.PullAllSamples();

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
        private BCIControllerInstance _testController;
        private GameObject _testControllerObject;
        private BCIControllerBehavior _behavior;
        private List<SPO> _spos;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateControllerInstance(setActive: true);
            _testController.Initialize();

            _behavior = AddComponent<EmptyBCIControllerBehavior>();
            _behavior.RegisterWithController(true);

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
        private BCIControllerInstance _testController;
        private BCIControllerBehavior _behavior;
        private List<SPO> _spos;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateControllerInstance(setActive: true);
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
        private BCIControllerInstance _testController;
        private GameObject _testControllerObject;
        private P300ControllerBehavior _behavior;
        private const string MyTag = "BCI";

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            LogTestName();
            yield return LoadDefaultSceneAsync();

            _testController = CreateControllerInstance();
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
            var result = _behavior.GetSPOGameObjectsInCameraViewByTag();

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