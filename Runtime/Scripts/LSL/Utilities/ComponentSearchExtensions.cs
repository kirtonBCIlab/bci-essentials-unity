using System;
using System.Linq;
using UnityEngine;

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

        public static void ForEachComponentInChildren<T>
        (
            this Component caller,
            Action<T> action
        )
        => Array.ForEach(caller.GetComponentsInChildren<T>(), action);

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