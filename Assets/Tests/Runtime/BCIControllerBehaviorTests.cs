using System.Collections;
using System.Collections.Generic;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using BCIEssentials.Utilities;
using NUnit.Framework;
using BCIEssentials.Tests.TestResources;
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
            
            Assert.IsTrue(_testController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [Test]
        public void WhenUnregisterFromControllerInstance_ThenBehaviorUnregistered()
        {
            _behavior.RegisterWithControllerInstance();
            
            _behavior.UnregisterFromControllerInstance();
            
            Assert.IsFalse(_testController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
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
            
            Assert.IsTrue(_testController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
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
            
            Assert.IsFalse(_testController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
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

            Assert.IsFalse(_testController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
        }

        [UnityTest]
        public IEnumerator OnDestroyAndNotSelfRegister_ThenBehaviorNotUnregistered()
        {
            _behavior.AssignInspectorProperties(new BCIControllerBehaviorExtensions.Properties
            {
                _selfRegister = false
            });
            _behavior.RegisterWithControllerInstance();
            yield return null;
            
            Object.DestroyImmediate(_behavior);

            Assert.IsFalse(_testController.HasBehaviorOfType<EmptyBCIControllerBehavior>());
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

    public class BCIControllerBehaviorTests_WhenSendReceiveMarkers : PlayModeTestRunnerBase
    {
        private BCIController _testController;
        private BCIControllerBehavior _behavior;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return LoadDefaultSceneAsync();

            _testController = CreateController(new BCIControllerExtensions.Properties
            {
                _dontDestroyActiveInstance = false
            }, true);

            _behavior = _testController.gameObject.AddComponent<EmptyBCIControllerBehavior>();
            _behavior.windowLength = 1;
            _behavior.interWindowInterval = 1;

            _testController.RegisterBehavior(_behavior, true);
        }

        [TearDown]
        public void TestCleanup()
        {
            StopAllCoroutineRunners();
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

            _testController = CreateController(new BCIControllerExtensions.Properties
            {
                _dontDestroyActiveInstance = false
            }, true);
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
            Assert.AreEqual(_behavior.SelectableSPOs.Count, spoCount);
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
            AddSPOToScene<MockSPO>().OnSelectedEvent.AddListener(() => onSelectedCalled = true);

            behaviorRunner.StartRun();
            yield return new WaitWhile(() => behaviorRunner.IsRunning);

            Assert.IsTrue(onSelectedCalled);
        }
    }
    
}