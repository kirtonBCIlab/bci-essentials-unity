using UnityEngine;

namespace BCIEssentials.Utilities
{
    public static class GameObjectExtensions
    {
        public static void FindOrCreateComponent<T>
        (
            this GameObject gameObject,
            ref T componentReference
        ) where T: Component
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
    }
}