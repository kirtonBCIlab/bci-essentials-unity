using System.Collections;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests.Utilities
{
    internal class PlayModeTestRunnerBase
    {
        [UnitySetUp]
        public virtual IEnumerator TestSetup()
        {
            yield return LoadEmptySceneAsync();
        }

        public IEnumerator LoadEmptySceneAsync()
        {
            yield return EditorSceneLoader.LoadEmptySceneAsync();
        }
    }
}