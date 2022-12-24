using System.Collections.Generic;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    /// <summary>
    /// Instantiates a matrix of BCI objects.
    /// All objects are given the tag "BCI" so that they can be collected by the controller script.
    /// </summary>
    public class MatrixSetup : MatrixSetupBase
    {
        [SerializeField] private SPO _spoPrefab;

        [Space]
        [SerializeField] private int _numColumns;
        [SerializeField] private int _numRows;
        [SerializeField] private Vector2 _startDistance;

        public readonly List<SPO> MatrixObjects = new();

        public override void SetUpMatrix()
        {
            InstantiateMatrixObjects();
            CenterCameraToMatrix();
        }

        public override void DestroyMatrix()
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
                    startingPosition =
                        Vector3.Scale(startingPosition, new Vector3(_startDistance.x, _startDistance.y, 1));

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

    public abstract class MatrixSetupBase : MonoBehaviour
    {
        public abstract void SetUpMatrix();
        public abstract void DestroyMatrix();
    }
}