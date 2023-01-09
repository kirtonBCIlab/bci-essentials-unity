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
            Debug.Log("<color=green>Test Started</color>");
            yield return LoadEmptySceneAsync();
        }

        protected static BCIController CreateController(bool dontDestroyInstance = false, bool setActive = false)
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

        protected static SPO AddSPOToScene(string tag = "BCI", bool includeMe = true)
        {
            return AddSPOToScene<SPO>(tag, includeMe);
        }
        
        protected static T AddSPOToScene<T>(string tag = "BCI", bool includeMe = true) where T : SPO
        {
            var spo = new GameObject { tag = string.IsNullOrEmpty(tag) ? "Untagged" : tag }.AddComponent<T>();
            spo.includeMe = includeMe;

            return spo;
        }

        protected IEnumerator LoadEmptySceneAsync()
        {
            yield return EditorSceneLoader.LoadEmptySceneAsync();
        }

        protected IEnumerator LoadDefaultSceneAsync()
        {
            yield return EditorSceneLoader.LoadDefaultSceneAsync();
        }

        protected static void SetField<T>(T component, string name, object value)
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

        protected static T AddComponent<T>() where T : MonoBehaviour
        {
            return AddComponent<T>(new GameObject());
        }

        protected static T AddComponent<T>(GameObject gameObject) where T : MonoBehaviour
        {
            return gameObject.AddComponent<T>();
        }

        protected static CoroutineRunner AddCoroutineRunner(IEnumerator coroutine, Action onComplete = null,
            bool destroyOnComplete = true)
        {
            return AddCoroutineRunner(new GameObject(), coroutine, onComplete, destroyOnComplete);
        }

        protected static CoroutineRunner AddCoroutineRunner(GameObject gameObject, IEnumerator coroutine, Action onComplete = null,
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

        protected static void StopAllCoroutineRunners()
        {
            foreach (var runner in Object.FindObjectsOfType<CoroutineRunner>())
            {
                if (runner != null)
                {
                    runner.StopRun();
                }
            }
        }

        protected IEnumerator DelayForFrames(int frameCount, Action onContinue)
        {
            int framesRan = 0;

            while (framesRan < frameCount)
            {
                ++framesRan;
                Debug.Log(framesRan);
                yield return null;
            }
            
            onContinue?.Invoke();
        }

        protected IEnumerator DelayForSeconds(float seconds, Action onContinue)
        {
            yield return new WaitForSecondsRealtime(seconds);
            onContinue?.Invoke();
        }
    }
}