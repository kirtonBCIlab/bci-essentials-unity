using System;
using System.Collections;
using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;
using BCIEssentials.StimulusObjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests.Utilities
{
    public class InClassName
    {
        public InClassName(GameObject gameObject, IEnumerator coroutine, Action onComplete = null)
        {
            GameObject = gameObject;
            Coroutine = coroutine;
            OnComplete = onComplete;
        }

        public GameObject GameObject { get; private set; }
        public IEnumerator Coroutine { get; private set; }
        public Action OnComplete { get; private set; }
    }

    public class PlayModeTestRunnerBase
    {
        protected static string CurrentTestName
        => TestContext.CurrentContext.Test.Name;

        [UnitySetUp]
        public virtual IEnumerator TestSetup()
        {
            LogTestName();
            yield return LoadEmptySceneAsync();
        }

        protected void LogTestName()
        {
            Debug.Log($"<color=green>Test Started: '{CurrentTestName}'</color>");
        }

        protected static BCIController CreateController(BCIControllerExtensions.Properties inspectorProperties = null,
            bool setActive = false)
        {
            var gameObject = new GameObject();
            gameObject.SetActive(false);

            gameObject.AddComponent<LSLMarkerStreamWriter>();
            gameObject.AddComponent<LSLResponseProvider>().PollingPeriod = 0.02f;

            var controller = gameObject.AddComponent<BCIController>();
            controller.AssignInspectorProperties(inspectorProperties);

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
            spo.Selectable = includeMe;

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

        protected static T AddComponent<T>(Action<T> setup = null) where T : MonoBehaviour
        {
            return AddComponent<T>(new GameObject(), setup);
        }

        protected static T AddComponent<T>(GameObject gameObject, Action<T> setup = null) where T : MonoBehaviour
        {
            var t = gameObject.AddComponent<T>();
            setup?.Invoke(t);
            return t;
        }

        public static void Destroy(Component component)
        {
            if (component.gameObject)
                Object.Destroy(component.gameObject);
            else if (component)
                Object.Destroy(component);
        }
        
        protected static CoroutineRunner RepeatForSeconds(Action onRepeat, int repeatCount, float repeatDelay = 0, Action onComplete = null)
        {
            repeatCount = repeatCount > 0 ? repeatCount : 1;
            repeatDelay = repeatDelay >= 0 ? repeatDelay : 0;
            
            IEnumerator Repeater()
            {
                int count = 0;
                while (count < repeatCount)
                {
                    onRepeat?.Invoke();
                    ++count;
                    yield return new WaitForSeconds(repeatDelay);
                }
            }
            
            return AddCoroutineRunner(new InClassName(new GameObject(), Repeater(), onComplete));
        }

        protected static CoroutineRunner AddCoroutineRunner(IEnumerator coroutine, Action onComplete = null)
        {
            return AddCoroutineRunner(new InClassName(new GameObject(), coroutine, onComplete));
        }

        protected static CoroutineRunner AddCoroutineRunner(InClassName inClassName)
        {
            return AddComponent<CoroutineRunner>(inClassName.GameObject, runner =>
            {
                runner.Routine = inClassName.Coroutine;
                runner.OnCompleteEvent = () => { inClassName.OnComplete?.Invoke(); };
            });
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