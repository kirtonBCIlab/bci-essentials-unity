using System;
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
        [SerializeField] private SPO _spoPrefab;

        [Space] [SerializeField] private int _numColumns = 1;
        [SerializeField] private int _numRows = 1;
        [SerializeField] private Vector2 _spacing = Vector2.one;

        public readonly List<SPO> MatrixObjects = new();

        public void Initialize(SPO prefab, int columns, int rows, Vector2 spacing)
        {
            _spoPrefab = prefab;
            _numColumns = Math.Max(1, columns);
            _numRows = Math.Max(1, rows);
            _spacing = spacing;
        }

        [ContextMenu("Set Up Matrix")]
        public void SetUpMatrix()
        {
            if (_spoPrefab == null)
            {
                Debug.LogError("No SPO for matrix");
                return;
            }

            if (MatrixObjects.Count > 0)
            {
                DestroyMatrix();
            }

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
            for (int rowIndex = 0; rowIndex < _numRows; rowIndex++)
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
                    spoGameObject.SetActive(true);
                    spoGameObject.name = $"Object {MatrixObjects.Count}";

                    //Setting position of object
                    var position = (Vector3)_spacing;
                    position.x *= columnIndex;
                    position.y *= rowIndex;

                    spoGameObject.transform.position = position;
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