using System;
using System.Collections;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;
using LogAssert = BCIEssentials.Tests.Utilities.LogAssert;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests
{
    public class BCIControllerTests : PlayModeTestRunnerBase
    {
        private BCIControllerInstance _testController;
        private GameObject _testControllerObject;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _testController = CreateControllerInstance();
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
        public void WhenAwakeAndAControllerInstanceExists_ThenControllerIsNotInstance()
        {
            var existingController = CreateControllerInstance();
            existingController.gameObject.SetActive(true);

            _testController.gameObject.SetActive(true);

            Assert.IsTrue(_testController.enabled);
            Assert.AreEqual(BCIController.Instance, existingController);
            Assert.AreNotEqual(BCIController.Instance, _testController);
        }

        [UnityTest]
        public IEnumerator WhenDontDestroyInstanceIsTrue_ThenInstanceNotDestroyed()
        {
            _testController.AssignInspectorProperties(new BCIControllerExtensions.Properties
            {
                _persistBetweenScenes = true
            }).gameObject.SetActive(true);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNotNull(_testController);
        }

        [UnityTest]
        public IEnumerator WhenDontDestroyInstanceIsFalse_ThenInstanceDestroyed()
        {
            _testController.AssignInspectorProperties(new BCIControllerExtensions.Properties
            {
                _persistBetweenScenes = false
            }).gameObject.SetActive(true);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNull(_testController);
        }

        [UnityTest]
        public IEnumerator WhenDestroyedAndInstance_ThenInstanceNull()
        {
            _testController.AssignInspectorProperties(new BCIControllerExtensions.Properties
            {
                _persistBetweenScenes = false
            }).gameObject.SetActive(true);

            UnityEngine.Assertions.Assert.IsNotNull(BCIController.Instance);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNull(BCIController.Instance);
        }

        [UnityTest]
        public IEnumerator WhenDestroyedAndNotInstance_ThenInstanceNotNull()
        {
            CreateControllerInstance(new BCIControllerExtensions.Properties
            {
                _persistBetweenScenes = true
            }, true);
            _testController.gameObject.SetActive(true);

            UnityEngine.Assertions.Assert.IsNotNull(BCIController.Instance);

            yield return LoadEmptySceneAsync();

            UnityEngine.Assertions.Assert.IsNotNull(BCIController.Instance);
        }

        [Test]
        [TestCase(typeof(P300ControllerBehavior))]
        [TestCase(typeof(MIControllerBehavior))]
        [TestCase(typeof(SSVEPControllerBehavior))]
        [TestCase(typeof(TVEPControllerBehavior))]
        public void WhenRegisterBehavior_ThenBehaviorRegistered(Type behaviorType)
        {
            _testController.Initialize();
            var behavior = _testControllerObject.AddComponent(behaviorType);

            var registered = BCIController.RegisterBehavior((BCIControllerBehavior)behavior);

            Assert.IsTrue(registered);
        }

        [Test]
        public void WhenRegisterBehaviorAndNullBehavior_ThenThrow()
        {
            Assert.Throws<ArgumentNullException>(
                () => BCIController.RegisterBehavior(null)
            );
        }

        [Test]
        public void WhenRegisterBehaviorAndBehaviorAlreadyRegistered_ThenNoBehaviorRegistered()
        {
            LogAssert.ExpectStartingWith(LogType.Warning, "Was unable to register");
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b => b.gameObject.SetActive(false));

            var firstResult = BCIController.RegisterBehavior(behavior);
            var secondResult = BCIController.RegisterBehavior(behavior);

            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
        }

        [Test]
        public void WhenRegisterBehaviorAndSetAsActiveIsTrue_ThenBehaviorRegisteredAndBecomesActiveBehavior()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>();

            BCIController.RegisterBehavior(behavior, true);

            Assert.AreEqual(behavior, _testController.ActiveBehavior);
        }

        [Test]
        public void WhenRegisterBehaviorAndSetAsActiveIsFalse_ThenBehaviorRegisteredAndIsNotActiveBehavior()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>();

            BCIController.RegisterBehavior(behavior, false);

            Assert.IsNull(_testController.ActiveBehavior);
        }

        [Test]
        public void WhenUnregisterBehavior_ThenBehaviorUnregistered()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b => BCIController.RegisterBehavior(b));

            BCIController.UnregisterBehavior(behavior);
            var wasRegistered = BCIController.RegisterBehavior(behavior);

            Assert.IsTrue(wasRegistered);
        }

        [Test]
        public void WhenUnregisterBehaviorAndBehaviorIsNull_ThenThrows()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b => BCIController.RegisterBehavior(b));

            Assert.Throws<ArgumentNullException>(
                () => BCIController.UnregisterBehavior(null)
            );
            var wasRegistered = BCIController.RegisterBehavior(behavior);

            Assert.IsFalse(wasRegistered);
        }

        [Test]
        public void WhenUnregisterBehaviorAndIsNotRegistered_ThenNoBehaviorUnregistered()
        {
            _testController.Initialize();
            var miBehavior = AddComponent<EmptyBCIControllerBehavior>(b => b.MockBehaviorType = BCIBehaviorType.MI);
            var p300Behavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.P300;
                BCIController.RegisterBehavior(b);
            });

            BCIController.UnregisterBehavior(miBehavior);
            var wasRegistered = BCIController.RegisterBehavior(p300Behavior);

            Assert.IsFalse(wasRegistered);
        }

        [Test]
        public void WhenUnregisterBehaviorAndIsNotActiveBehavior_ThenActiveBehaviorUnchanged()
        {
            _testController.Initialize();
            var p300Behavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.P300;
                BCIController.RegisterBehavior(b, true);
            });

            var miBehavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.MI;
                BCIController.RegisterBehavior(b, false);
            });

            BCIController.UnregisterBehavior(miBehavior);

            Assert.AreEqual(p300Behavior, _testController.ActiveBehavior);
        }

        [Test]
        public void WhenUnregisterBehaviorAndIsActiveBehavior_ThenActiveBehaviorRemoved()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>();
            BCIController.RegisterBehavior(behavior, true);

            BCIController.UnregisterBehavior(behavior);

            UnityEngine.Assertions.Assert.IsNull(_testController.ActiveBehavior);
        }

        [Test]
        public void WhenChangeBehavior_ThenActiveBehaviorChanged()
        {
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.P300;
                BCIController.RegisterBehavior(b);
            });

            BCIController.ChangeBehavior(BCIBehaviorType.P300);

            Assert.AreEqual(_testController.ActiveBehavior, behavior);
        }

        [Test]
        public void WhenChangeBehaviorAndBehaviorIsNull_ThenUnregisterBehaviorType()
        {
            LogAssert.ExpectAnyContains(LogType.Error, "Unable to find");
            _testController.Initialize();
            var behavior = AddComponent<EmptyBCIControllerBehavior>(b =>
            {
                b.MockBehaviorType = BCIBehaviorType.P300;
                BCIController.RegisterBehavior(b);
            });
            Object.DestroyImmediate(behavior);
            
            BCIController.ChangeBehavior(BCIBehaviorType.P300);

            Assert.IsNull(BCIController.Instance.ActiveBehavior);
            Assert.IsFalse(BCIController.HasBehaviorForType(BCIBehaviorType.P300));
        }
    }
}