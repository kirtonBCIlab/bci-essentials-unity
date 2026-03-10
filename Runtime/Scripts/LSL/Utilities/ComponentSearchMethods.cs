using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BCIEssentials.Utilities
{
    public static class ComponentSearchMethods
    {
        public static bool TryFindComponents<T>
        (
            out T[] result,
            FindObjectsInactive findMode = FindObjectsInactive.Exclude,
            FindObjectsSortMode sortMode = FindObjectsSortMode.None
        ) where T : Component
        {
            result = UnityEngine.Object.FindObjectsByType<T>(findMode, sortMode);
            return result?.Length > 0;
        }

        public static bool TryFindComponent<T>
        (
            out T Result,
            FindObjectsInactive findMode = FindObjectsInactive.Exclude
        ) where T : Component
        => Result = UnityEngine.Object.FindAnyObjectByType<T>(findMode);


        public static T[] GetComponentsInScene<T>
        (
            bool includeInactive = false
        )
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = scene.GetRootGameObjects();
            return rootObjects.SelectMany(
                rootObject => rootObject.GetComponentsInChildren<T>(includeInactive)
            ).ToArray();
        }


        public static void ForEachComponentInScene<T>
        (
            Action<T> action,
            bool includeInactive = false
        )
        => Array.ForEach(GetComponentsInScene<T>(includeInactive), action);
    }
}