using System;
using System.Collections;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using BCIEssentials.LSL;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests
{
    public class BCIControllerTests
    {
        public class BCIControllerAndOnAwake : PlayModeTestRunnerBase
        {
            private BCIController _testController;
            
            [UnitySetUp]
            public override IEnumerator TestSetup()
            {
                yield return base.TestSetup();

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
            
            [Test]
            public void WhenNoMarkerStreamPresent_ThenControllerDisabled()
            {
                LogAssert.ExpectAnyContains(LogType.Error, typeof(LSLMarkerStream).ToString());
                Object.DestroyImmediate(_testController.GetComponent<LSLMarkerStream>());

                _testController.gameObject.SetActive(true);
                
                Assert.IsFalse(_testController.enabled);
            }

            [Test]
            public void WhenNoResponseStreamPresent_ThenControllerDisabled()
            {
                LogAssert.ExpectAnyContains(LogType.Error, typeof(LSLResponseStream).ToString());
                Object.DestroyImmediate(_testController.GetComponent<LSLResponseStream>());

                _testController.gameObject.SetActive(true);
                
                Assert.IsFalse(_testController.enabled);
            }

            [Test]
            public void WhenAControllerInstanceExists_ThenControllerIsNotInstance()
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
                _testController.TestInitializable(true);
                _testController.gameObject.SetActive(true);

                yield return LoadEmptySceneAsync();
                
                UnityEngine.Assertions.Assert.IsNotNull(_testController);
            }

            [UnityTest]
            public IEnumerator WhenDontDestroyInstanceIsFalse_ThenInstanceDestroyed()
            {
                _testController.TestInitializable(false);
                _testController.gameObject.SetActive(true);

                yield return LoadEmptySceneAsync();
                
                UnityEngine.Assertions.Assert.IsNull(_testController);
            }
        }

        public class BCIControllerAndOnDestroy : PlayModeTestRunnerBase
        {
            private BCIController _testController;
            
            [UnitySetUp]
            public override IEnumerator TestSetup()
            {
                yield return base.TestSetup();

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
            public IEnumerator WhenDestroyedAndInstance_ThenInstanceNull()
            {
                _testController.TestInitializable(false);
                _testController.gameObject.SetActive(true);

                yield return LoadEmptySceneAsync();
                
                UnityEngine.Assertions.Assert.IsNull(BCIController.Instance);
            }

            [UnityTest]
            public IEnumerator WhenDestroyedAndNotInstance_ThenInstanceNotNull()
            {
                CreateController(true, true);
                _testController.gameObject.SetActive(true);

                yield return LoadEmptySceneAsync();
                
                UnityEngine.Assertions.Assert.IsNotNull(BCIController.Instance);
            }
        }

        public class BCIControllerAndManagingBehaviors : PlayModeTestRunnerBase
        {
            private BCIController _testController;
            private GameObject _testControllerObject;
            
            [UnitySetUp]
            public override IEnumerator TestSetup()
            {
                yield return base.TestSetup();

                _testController = CreateController(false, true);
                _testControllerObject = _testController.gameObject;
            }

            [Test]
            [TestCase(typeof(P300ControllerBehavior))]
            [TestCase(typeof(MIControllerBehavior))]
            [TestCase(typeof(SSVEPControllerBehavior))]
            public void WhenRegisterBehavior_ThenBehaviorRegistered(Type behaviorType)
            {
                var behavior = _testControllerObject.AddComponent(behaviorType);
                
                var registered = _testController.RegisterBehavior((BCIControllerBehavior) behavior);
                
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
                var behaviorObject = new GameObject();
                behaviorObject.SetActive(false);
                
                var firstResult = _testController.RegisterBehavior(behaviorObject.AddComponent<P300ControllerBehavior>());
                var secondResult = _testController.RegisterBehavior(behaviorObject.AddComponent<P300ControllerBehavior>());
                
                Assert.IsTrue(firstResult);
                Assert.IsFalse(secondResult);
            }

            [Test]
            public void WhenRegisterBehaviorAndSetAsActiveIsTrue_ThenBehaviorRegisteredAndBecomesActiveBehavior()
            {
                _testControllerObject.SetActive(false);
                var behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                
                _testController.RegisterBehavior(behavior, true);
                
                Assert.AreEqual(behavior, _testController.ActiveBehavior);
            }

            [Test]
            public void WhenRegisterBehaviorAndSetAsActiveIsFalse_ThenBehaviorRegisteredAndIsNotActiveBehavior()
            {
                _testControllerObject.SetActive(false);
                var behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                
                _testController.RegisterBehavior(behavior, false);
                
                Assert.IsNull(_testController.ActiveBehavior);
            }

            [Test]
            public void WhenUnregisterBehavior_ThenBehaviorUnregistered()
            {
                var behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                _testController.RegisterBehavior(behavior);
                
                _testController.UnregisterBehavior(behavior);
                var reregistered = _testController.RegisterBehavior(behavior);
                
                Assert.IsTrue(reregistered);
            }

            [Test]
            public void WhenUnregisterBehaviorAndBehaviorIsNull_ThenNoBehaviorUnregistered()
            {
                var behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                _testController.RegisterBehavior(behavior);
                
                _testController.UnregisterBehavior(null);
                var reregistered = _testController.RegisterBehavior(behavior);
                
                Assert.IsFalse(reregistered);
            }

            [Test]
            public void WhenUnregisterBehaviorAndIsNotRegistered_ThenNoBehaviorUnregistered()
            {
                var p300Behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                var miBehavior = _testControllerObject.AddComponent<MIControllerBehavior>();
                _testController.RegisterBehavior(p300Behavior);
                
                _testController.UnregisterBehavior(miBehavior);
                var reregistered = _testController.RegisterBehavior(p300Behavior);
                
                Assert.IsFalse(reregistered);
            }

            [Test]
            public void WhenUnregisterBehaviorAndIsNotActiveBehavior_ThenActiveBehaviorUnchanged()
            {
                var p300Behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                _testController.RegisterBehavior(p300Behavior, true);
                var miBehavior = _testControllerObject.AddComponent<MIControllerBehavior>();
                _testController.RegisterBehavior(miBehavior, false);
                
                _testController.UnregisterBehavior(miBehavior);
                
                Assert.AreEqual(p300Behavior, _testController.ActiveBehavior);
            }

            [Test]
            public void WhenUnregisterBehaviorAndIsActiveBehavior_ThenActiveBehaviorRemoved()
            {
                var behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                _testController.RegisterBehavior(behavior, true);
                
                _testController.UnregisterBehavior(behavior);
                
                UnityEngine.Assertions.Assert.IsNull(_testController.ActiveBehavior);
            }

            [Test]
            public void WhenChangeBehavior_ThenActiveBehaviorChanged()
            {
                var behavior = _testControllerObject.AddComponent<P300ControllerBehavior>();
                _testController.RegisterBehavior(behavior);
                
                _testController.ChangeBehavior(BehaviorType.P300);
                
                Assert.AreEqual(_testController.ActiveBehavior, behavior);
            }
        }
    }
}