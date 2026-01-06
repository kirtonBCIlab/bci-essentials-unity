using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests.Utilities
{
    public class PlayModeTestRunner
    {
        protected static string CurrentTestName
        => TestContext.CurrentContext.Test.Name;

        [UnitySetUp]
        public virtual IEnumerator TestSetup()
        {
            LogTestName();
            yield return null;
        }

        protected void LogTestName()
        {
            Debug.Log($"<color=green>Test Started: '{CurrentTestName}'</color>");
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
    }
}