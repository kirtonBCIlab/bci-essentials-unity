using System.Collections.Generic;
using System.Linq;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public abstract class SPOFactory: ScriptableObject
    {
        [SerializeField] private SPO _spoPrefab;

        public List<SPO> FabricatedObjects => _fabricatedObjects.ToList();
        public int FabricatedObjectCount => _fabricatedObjects.Count;
        private List<SPO> _fabricatedObjects = new();

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

            if (FabricatedObjectCount > 0)
            {
                DestroyObjects();
            }

            InstantiateConfiguredObjects(objectParent);
        }

        protected abstract void InstantiateConfiguredObjects(Transform objectParent);
        protected SPO InstantiateObject(Transform parent)
        {
            SPO newObject = (parent == null)
                ? Instantiate(_spoPrefab)
                : Instantiate(_spoPrefab, parent);
            _fabricatedObjects.Add(newObject);
            return newObject;
        }

        public void DestroyObjects() {
            foreach (var spo in _fabricatedObjects)
            {
                if (spo != null)
                {
                    DestroyImmediate(spo.gameObject);
                }
            }

            _fabricatedObjects.Clear();
        }
    }
}