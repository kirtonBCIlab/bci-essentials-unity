using UnityEngine;

namespace BCIEssentials.Utilities
{
    public class DontDestroy : MonoBehaviour
    {
        private static DontDestroy instance = null;

        private void Awake()
        {
            if (instance != null && instance != this && instance.gameObject != null)
            {
                //Replace this with the updated one....see if this makes everything worse.
                Destroy(instance.gameObject);
            }

            //Set instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}