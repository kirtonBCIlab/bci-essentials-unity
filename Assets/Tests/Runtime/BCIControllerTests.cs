using System;
using System.Collections;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using BCIEssentials.LSL;
using BCIEssentials.Tests.TestResources;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LogAssert = BCIEssentials.Tests.TestResources.LogAssert;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests
{
    public class BCIControllerTests : PlayModeTestRunnerBase
    {
        private BCIController _testController;
        private GameObject _testControllerObject;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _testController = CreateController();
            _testControllerObject = _testController.gameObject;
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
        public void WhenAwakeAndNoMarkerStreamPresent_ThenControllerDisabled()
        {
            LogAssert.ExpectAnyContains(LogType.Error, typeof(LSLMarkerStream).ToString());
            Object.DestroyImmediate(_testController.GetComponent<LSLMarkerStream>());

            _testController.gameObject.SetActive(true);

            Assert.IsFalse(_testController.enabled);
        }

        [Test]
        public void WhenAwakeAndNoResponseStreamPresent_ThenControllerDisabled()
        {
            LogAssert.ExpectAnyContains(LogType.Error, typeof(LSLResponseStream).ToString());
            Object.DestroyImmediate(_testController.GetComponent<LSLResponseStream>());

            _testController.gameObject.SetActive(true);

            Assert.IsFalse(_testController.enabled);
        }

        [Test]
        public void WhenAwakeAndAControllerInstanceExists_ThenControllerIsNotInstance()
        {
            var existingController = CreateController();
            existingController.gameObject.SetActive(true);

            _testController.gameObject.SetActive(true);

            Assert.IsTrue(_testController.enabled);
            Assert.AreEqual(BCIController.Instance, existingController);
            Assert.AreNotEqual(BCIController.Instance, _testController);
        }

        [UnityTest]
        public IEnumerator WhenDontDestroyInstanceIsTrue_ThenInstanceNotDestroyed()
        {
            _testController.SetInspectorProperties(new BCIControllerExtensions.Properties
            {
                _dontDestroyActiveInstance = true
            }).gameObject.SetActive(true);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNotNull(_testController);
        }

        [UnityTest]
        public IEnumerator WhenDontDestroyInstanceIsFalse_ThenInstanceDestroyed()
        {
            _testController.SetInspectorProperties(new BCIControllerExtensions.Properties
            {
                _dontDestroyActiveInstance = false
            }).gameObject.SetActive(true);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNull(_testController);
        }

        [UnityTest]
        public IEnumerator WhenDestroyedAndInstance_ThenInstanceNull()
        {
            _testController.SetInspectorProperties(new BCIControllerExtensions.Properties
            {
                _dontDestroyActiveInstance = false
            }).gameObject.SetActive(true);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNull(BCIController.Instance);
        }

        [UnityTest]
        public IEnumerator WhenDestroyedAndNotInstance_ThenInstanceNotNull()
        {
            CreateController(new BCIControllerExtensions.Properties
            {
                _dontDestroyActiveInstance = true
            }, true);
            _testController.gameObject.SetActive(true);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNotNull(BCIController.Instance);
        }

        [Test]
        [TestCase(typeof(P300ControllerBehavior))]
        [TestCase(typeof(MIControllerBehavior))]
        [TestCase(typeof(SSVEPControllerBehavior))]
        public void WhenRegisterBehavior_ThenBehaviorRegistered(Type behaviorType)
        {
            _testController.Initialize();
            var behavior = _testControllerObject.AddComponent(behaviorType);

            var registered = _testController.RegisterBehavior((BCIControllerBehavior)behavior);

            Assert.IsTrue(registered);
        }

        [Test]
        public void WhenRegisterBehaviorAndNullBehavior_ThenNoBehaviorRegistered()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Controller Behavior is null");

            var registered = _testController.RegisterBehavior(null);

            Assert.IsFalse(registered);
        }

        [Test]
        public void WhenRegisterBehaviorAndBehaviorAlreadyRegistered_ThenNoBehaviorRegistered()
        {
            LogAssert.ExpectStartingWith(LogType.Warning, "Was unable to register");
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b => b.gameObject.SetActive(false));

            var firstResult = _testController.RegisterBehavior(behavior);
            var secondResult = _testController.RegisterBehavior(behavior);

            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
        }

        [Test]
        public void WhenRegisterBehaviorAndSetAsActiveIsTrue_ThenBehaviorRegisteredAndBecomesActiveBehavior()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>();

            _testController.RegisterBehavior(behavior, true);

            Assert.AreEqual(behavior, _testController.ActiveBehavior);
        }

        [Test]
        public void WhenRegisterBehaviorAndSetAsActiveIsFalse_ThenBehaviorRegisteredAndIsNotActiveBehavior()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>();

            _testController.RegisterBehavior(behavior, false);

            Assert.IsNull(_testController.ActiveBehavior);
        }

        [Test]
        public void WhenUnregisterBehavior_ThenBehaviorUnregistered()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b => _testController.RegisterBehavior(b));

            _testController.UnregisterBehavior(behavior);
            var wasRegistered = _testController.RegisterBehavior(behavior);

            Assert.IsTrue(wasRegistered);
        }

        [Test]
        public void WhenUnregisterBehaviorAndBehaviorIsNull_ThenNoBehaviorUnregistered()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b => _testController.RegisterBehavior(b));

            _testController.UnregisterBehavior(null);
            var wasRegistered = _testController.RegisterBehavior(behavior);

            Assert.IsFalse(wasRegistered);
        }

        [Test]
        public void WhenUnregisterBehaviorAndIsNotRegistered_ThenNoBehaviorUnregistered()
        {
            _testController.Initialize();
            var miBehavior = AddComponent<EmptyBCIControllerBehavior>(b => b.MockBehaviorType =  BCIBehaviorType.MI);
            var p300Behavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.P300;
                _testController.RegisterBehavior(b);
            });

            _testController.UnregisterBehavior(miBehavior);
            var wasRegistered = _testController.RegisterBehavior(p300Behavior);

            Assert.IsFalse(wasRegistered);
        }

        [Test]
        public void WhenUnregisterBehaviorAndIsNotActiveBehavior_ThenActiveBehaviorUnchanged()
        {
            _testController.Initialize();
            var p300Behavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.P300;
                _testController.RegisterBehavior(b, true);
            });
            
            var miBehavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.MI;
                _testController.RegisterBehavior(b, false);
            });

            _testController.UnregisterBehavior(miBehavior);

            Assert.AreEqual(p300Behavior, _testController.ActiveBehavior);
        }

        [Test]
        public void WhenUnregisterBehaviorAndIsActiveBehavior_ThenActiveBehaviorRemoved()
        {
            var behavior = AddComponent<EmptyBCIControllerBehavior>();
            _testController.RegisterBehavior(behavior, true);

            _testController.UnregisterBehavior(behavior);

            UnityEngine.Assertions.Assert.IsNull(_testController.ActiveBehavior);
        }

        [Test]
        public void WhenChangeBehavior_ThenActiveBehaviorChanged()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.P300;
                _testController.RegisterBehavior(b);
            });

            _testController.ChangeBehavior(BCIBehaviorType.P300);

            Assert.AreEqual(_testController.ActiveBehavior, behavior);
        }
    }
}