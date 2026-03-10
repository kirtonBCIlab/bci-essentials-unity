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
    }
}

namespace BCIEssentials.Extensions
{
    using static Utilities.ComponentSearchMethods;

    public static class ComponentSearchExtensions
    {
        public static void GetOrAddComponent<T>
        (
            this GameObject gameObject,
            ref T componentReference,
            bool searchEntireScene = false
        ) where T : Component
        {
            if (
                componentReference == null &&
                !gameObject.TryGetComponent(out componentReference) &&
                (
                    !searchEntireScene ||
                    !TryFindComponent(out componentReference)
                )
            )
            {
                Debug.Log($"No {typeof(T).Name} found, creating one...");
                componentReference = gameObject.AddComponent<T>();
            }
        }


        public static bool AnyOther<T>
        (
            this T caller,
            Func<T, bool> predicate,
            bool includeInactive = false
        ) where T : Component
        {
            FindObjectsInactive findMode = includeInactive
            ? FindObjectsInactive.Include
            : FindObjectsInactive.Exclude;

            TryFindComponents(out T[] allComponentsInScene, findMode);
            return allComponentsInScene.Any(c => c != caller && predicate(c));
        }
    }
}