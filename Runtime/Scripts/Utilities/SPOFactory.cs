using System.Collections.Generic;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public abstract class SPOFactory: ScriptableObject
    {
        [SerializeField] private SPO _spoPrefab;

        public readonly List<SPO> FabricatedObjects = new();

        public void Init(SPO prefab)
        {
            _spoPrefab = prefab;
        }


        [ContextMenu("Build SPOs")]
        public void CreateObjects(Transform objectParent = null)
        {
            if (_spoPrefab == null)
            {
                Debug.LogError("No SPO for Factory");
                return;
            }

            if (FabricatedObjects.Count > 0)
            {
                DestroyObjects();
            }

            InstantiateConfiguredObjects(objectParent);
        }

        protected abstract void InstantiateConfiguredObjects(Transform objectParent);
        protected SPO InstantiateObject(Transform parent)
        {
            if (parent == null) return Instantiate(_spoPrefab);
            return Instantiate(_spoPrefab, parent);
        }

        public void DestroyObjects() {
            foreach (var spo in FabricatedObjects)
            {
                if (spo != null)
                {
                    DestroyImmediate(spo.gameObject);
                }
            }

            FabricatedObjects.Clear();
        }
    }
}