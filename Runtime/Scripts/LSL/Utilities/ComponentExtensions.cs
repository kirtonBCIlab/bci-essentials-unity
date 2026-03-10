using System;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.LSLFramework.Utilities
{
    public static class ComponentsExtensions
    {
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
    }
}