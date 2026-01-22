using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Extensions
{
    public static class GameObjectExtensions
    {
        public static void Activate(this GameObject caller)
        => caller.SetActive(true);
        public static void ActivateGameObject(this Component caller)
        => caller.gameObject.Activate();
        public static void Deactivate(this GameObject caller)
        => caller.SetActive(false);
        public static void DeactivateGameObject(this Component caller)
        => caller.gameObject.Deactivate();


        public static void GetOrAddComponent<T>
        (
            this GameObject gameObject, ref T componentReference
        ) where T : Component
        {
            if (
                componentReference == null &&
                !gameObject.TryGetComponent(out componentReference)
            )
            {
                Debug.Log(
                    $"No {typeof(T).Name}"
                    + " component found, creating one..."
                );
                componentReference = gameObject.AddComponent<T>();
            }
        }

        public static T CoalesceComponentReference<T>
        (
            this Component caller, ref T componentReference
        ) where T : Component
        {
            if (componentReference == null)
            {
                if (!caller.TryGetComponent(out componentReference))
                {
                    componentReference = caller.GetComponentInChildren<T>();
                }
            }
            return componentReference;
        }


        public static GameObject[] GetChildObjectsWithTag
        (
            this MonoBehaviour caller, string tag
        )
        => caller.transform.GetChildObjectsWithTag(tag);

        public static GameObject[] GetChildObjectsWithTag
        (
            this Transform caller, string tag
        )
        => caller.GetChildrenWithTag(tag).SelectGameObjects();

        public static List<Transform> GetChildrenWithTag
        (
            this Transform caller, string tag
        )
        {
            List<Transform> result = new();
            foreach (Transform child in caller)
            {
                if (child.CompareTag(tag)) result.Add(child);
                result.AddRange(child.GetChildrenWithTag(tag));
            }
            return result;
        }


        private static GameObject[] SelectGameObjects
        (
            this IEnumerable<Transform> caller
        )
        => caller.Select(t => t.gameObject).ToArray();

        public static List<GameObject> SelectGameObjects
        (
            this IEnumerable<object> caller
        )
        {
            static GameObject CollapseAndWarn()
            {
                Debug.LogWarning("Failed to select GameObject");
                return null;
            }
            return caller.Select(o => o switch
                {
                    Component c => c.gameObject,
                    GameObject g => g,
                    _ => CollapseAndWarn()
                }
            ).ToList();
        }
    }
}