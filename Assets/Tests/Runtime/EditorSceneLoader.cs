using System.Collections;
using BCIEssentials.Tests.Resources;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace BCIEssentials.Tests
{
    public static class EditorSceneLoader
    {
        private const string k_AssetPath = "Packages/com.bci4kids.bciessentials/Tests/Resources/ResourceLibrary.asset";
        
        private static ResourceLibraryAsset _resourceLibrary;
        
        public static IEnumerator LoadEmptySceneAsync()
        {
            if (_resourceLibrary == null && !TryGetResourceLibrary(out _resourceLibrary))
            {
                yield break;
            }

            var scenePath = AssetDatabase.GetAssetPath(_resourceLibrary.EmptyScene);
            if (string.IsNullOrEmpty(scenePath))
            {
                yield break;
            }

            var loadOperation =
                EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (!loadOperation.isDone)
            {
                yield return null;
            }
        }
        
        public static IEnumerator LoadDefaultSceneAsync()
        {
            if (_resourceLibrary == null && !TryGetResourceLibrary(out _resourceLibrary))
            {
                yield break;
            }

            var scenePath = AssetDatabase.GetAssetPath(_resourceLibrary.DefaultScene);
            if (string.IsNullOrEmpty(scenePath))
            {
                yield break;
            }

            var loadOperation =
                EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (!loadOperation.isDone)
            {
                yield return null;
            }
        }

        private static bool TryGetResourceLibrary(out ResourceLibraryAsset library)
        {
            library = AssetDatabase.LoadAssetAtPath<ResourceLibraryAsset>(k_AssetPath);
            return library != null;
        }
    }
}