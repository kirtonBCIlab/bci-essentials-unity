using System;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    /// <summary>
    /// Instantiates a matrix of BCI objects.
    /// All objects are given the tag "BCI" so that they can be collected by the controller script.
    /// </summary>
    [CreateAssetMenu(menuName = "BCI Essentials/SPO Grid Factory", fileName = "SPO Grid Factory")]
    public class SPOGridFactory: SPOFactory
    {
        [Space]
        [SerializeField] private int _numRows = 1;
        [SerializeField] private int _numColumns = 1;
        [SerializeField] private Vector2 _spacing = Vector2.one;


        public static SPOGridFactory CreateInstance()
        => CreateInstance<SPOGridFactory>();

        public static SPOGridFactory CreateInstance
        (
            SPO prefab,
            int columns, int rows,
            Vector2 spacing
        )
        {
            var instance = CreateInstance<SPOGridFactory>();
            instance.Init(prefab, columns, rows, spacing);
            return instance;
        }

        public void Init
        (
            SPO prefab,
            int columns, int rows,
            Vector2 spacing
        )
        {
            base.Init(prefab);
            _numColumns = Math.Max(1, columns);
            _numRows = Math.Max(1, rows);
            _spacing = spacing;
        }


        protected override void InstantiateConfiguredObjects(Transform objectParent = null)
        {
            for (int rowIndex = 0; rowIndex < _numRows; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < _numColumns; columnIndex++)
                {
                    //Instantiating prefabs
                    var spo = InstantiateObject(objectParent);

                    //Setup SPO
                    spo.StopStimulus();

                    //Setup GameObject
                    var spoGameObject = spo.gameObject;
                    spoGameObject.SetActive(true);
                    spoGameObject.name = $"Object {FabricatedObjects.Count}";

                    //Setting position of object
                    var position = (Vector3)_spacing;
                    position.x *= columnIndex;
                    position.y *= rowIndex;

                    spoGameObject.transform.position = position;
                }
            }

            CenterCameraToMatrix();
        }

        private void CenterCameraToMatrix()
        {
            var totalPosition = Vector3.zero;
            foreach (var spo in FabricatedObjects)
            {
                totalPosition += spo.transform.position;
            }

            var centerPosition = totalPosition / FabricatedObjects.Count;
            centerPosition.z = -10f;

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No Main Camera found.");
                return;
            }

            mainCamera.transform.position = centerPosition;
            mainCamera.orthographicSize = _numRows > _numColumns ? _numRows : _numColumns;

            Debug.Log($"Camera Position set to: ({centerPosition.x}, {centerPosition.y}, {centerPosition.z})");
        }
    }
}