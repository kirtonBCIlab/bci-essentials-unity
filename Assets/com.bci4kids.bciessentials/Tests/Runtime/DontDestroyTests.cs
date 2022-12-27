using System.Collections;
using BCIEssentials.Utilities;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;

namespace BCIEssentials.Tests.Utilities
{
    internal class DontDestroyTests : PlayModeTestRunnerBase
    {
        private DontDestroy _dontDestroy;
        
        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _dontDestroy = new GameObject().AddComponent<DontDestroy>();
        }

        [UnityTest]
        public IEnumerator WhenNewSceneLoad_ThenObjectNotDestroyed()
        {
            yield return LoadEmptySceneAsync();

            var survivingObject = Object.FindObjectOfType<DontDestroy>();
            Assert.IsNotNull(survivingObject);
            Assert.AreEqual(_dontDestroy, survivingObject);
        }

        [UnityTest]
        public IEnumerator WhenMultipleInScene_ThenPreviousInstancesDestroyed()
        {
            var newInstance = new GameObject().AddComponent<DontDestroy>();
            
            yield return new WaitForEndOfFrame();

            Assert.IsNull(_dontDestroy);
            Assert.IsNotNull(newInstance);
        }
    }
}