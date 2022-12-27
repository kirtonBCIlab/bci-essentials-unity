using System.Collections.Generic;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    /// <summary>
    /// Instantiates a matrix of BCI objects.
    /// All objects are given the tag "BCI" so that they can be collected by the controller script.
    /// </summary>
    public class MatrixSetup : MonoBehaviour
    {
        [SerializeField] private int _numColumns;
        [SerializeField] private int _numRows;
        
        [Space]
        [SerializeField] private SPO _spoPrefab;
        [SerializeField] private Vector2 _spacing;

        public readonly List<SPO> MatrixObjects = new();

        public void Initialize(SPO prefab, int columns, int rows, Vector2 spacing)
        {
            _spoPrefab = prefab;
            _numColumns = columns;
            _numColumns = rows;
            _spacing = spacing;
        }

        public void SetUpMatrix()
        {
            InstantiateMatrixObjects();
            CenterCameraToMatrix();
        }

        public void DestroyMatrix()
        {
            foreach (var spo in MatrixObjects)
            {
                if (spo != null)
                {
                    Destroy(spo.gameObject);
                }
            }

            MatrixObjects.Clear();
        }
        
        private void InstantiateMatrixObjects()
        {
            for (int rowIndex = _numRows - 1; rowIndex > -1; rowIndex--)
            {
                for (int columnIndex = 0; columnIndex < _numColumns; columnIndex++)
                {
                    //Instantiating prefabs
                    var spo = Instantiate(_spoPrefab);

                    //Setup SPO
                    MatrixObjects.Add(spo);
                    spo.TurnOff();

                    //Setup GameObject
                    var spoGameObject = spo.gameObject;
                    spoGameObject.name = $"Object {MatrixObjects.Count}";
                    spoGameObject.tag = "BCI";

                    //Setting position of object
                    var startingPosition = Vector3.zero;
                    startingPosition.x += columnIndex;
                    startingPosition.y += rowIndex;
                    startingPosition += (Vector3)_spacing;

                    spoGameObject.transform.position = startingPosition;

                    //Activating objects
                    spoGameObject.SetActive(true);
                }
            }
        }

        private void CenterCameraToMatrix()
        {
            var totalPosition = Vector3.zero;
            foreach (var spo in MatrixObjects)
            {
                totalPosition += spo.transform.position;
            }

            var centerPosition = totalPosition / MatrixObjects.Count;
            centerPosition.z = -10f ;

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