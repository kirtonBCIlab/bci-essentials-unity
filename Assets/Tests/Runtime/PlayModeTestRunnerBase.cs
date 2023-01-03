using System;
using System.Collections;
using System.Reflection;
using BCIEssentials.Controllers;
using BCIEssentials.LSL;
using BCIEssentials.StimulusObjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests.Utilities
{
    public class PlayModeTestRunnerBase
    {
        [UnitySetUp]
        public virtual IEnumerator TestSetup()
        {
            yield return LoadEmptySceneAsync();
        }

        public static BCIController CreateController(bool dontDestroyInstance = false, bool setActive = false)
        {
            var gameObject = new GameObject();
            gameObject.SetActive(false);

            gameObject.AddComponent<LSLMarkerStream>();
            gameObject.AddComponent<LSLResponseStream>();

            var controller = gameObject.AddComponent<BCIController>();
            controller.TestInitializable(dontDestroyInstance);

            if (setActive)
            {
                gameObject.SetActive(true);
            }

            return controller;
        }

        public static SPO AddSPOToScene(string tag = "BCI", bool includeMe = true)
        {
            return AddSPOToScene<SPO>(tag, includeMe);
        }
        
        public static T AddSPOToScene<T>(string tag = "BCI", bool includeMe = true) where T : SPO
        {
            var spo = new GameObject { tag = tag }.AddComponent<T>();
            spo.includeMe = includeMe;

            return spo;
        }

        public IEnumerator LoadEmptySceneAsync()
        {
            yield return EditorSceneLoader.LoadEmptySceneAsync();
        }

        public IEnumerator LoadDefaultSceneAsync()
        {
            yield return EditorSceneLoader.LoadDefaultSceneAsync();
        }

        public static void SetField<T>(T component, string name, object value)
        {
            var info = component.GetType()
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (info == null)
            {
                Assert.Fail($"No field for name {name} on object");
                return;
            }

            info.SetValue(component, value);
        }

        public static T AddComponent<T>() where T : MonoBehaviour
        {
            return AddComponent<T>(new GameObject());
        }

        public static T AddComponent<T>(GameObject gameObject) where T : MonoBehaviour
        {
            return gameObject.AddComponent<T>();
        }

        public static CoroutineRunner AddCoroutineRunner(IEnumerator coroutine, Action onComplete = null,
            bool destroyOnComplete = true)
        {
            return AddCoroutineRunner(new GameObject(), coroutine, onComplete, destroyOnComplete);
        }

        public static CoroutineRunner AddCoroutineRunner(GameObject gameObject, IEnumerator coroutine, Action onComplete = null,
            bool destroyOnComplete = true)
        {
            var runner = AddComponent<CoroutineRunner>(gameObject);
            runner.Routine = coroutine;
            runner.OnCompleteEvent = () =>
            {
                onComplete?.Invoke();
                if (destroyOnComplete && runner != null)
                {
                    Object.Destroy(runner.gameObject);
                }
            };

            return runner;
        }
    }
}