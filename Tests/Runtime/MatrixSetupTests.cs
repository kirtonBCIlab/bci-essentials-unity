using System.Collections;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests.Utilities
{
    internal class MatrixSetupTests : PlayModeTestRunnerBase
    {
        private Camera _mainCamera;
        private SPOGridFactory _matrixSetup;
        private SPO _spo;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _matrixSetup = SPOGridFactory.CreateInstance();
            _spo = new GameObject().AddComponent<SPO>();
            _mainCamera = new GameObject().AddComponent<Camera>();
            _mainCamera.tag = "MainCamera";
        }

        [Test]
        public void WhenSetUpMatrixWithNullSPO_ThenNoObjectsGenerated()
        {
            Utilities.LogAssert.ExpectStartingWith(LogType.Error, "No SPO");
            
            _matrixSetup.CreateObjects();

            Assert.AreEqual(0, _matrixSetup.FabricatedObjects.Count);
        }

        [Test]
        [TestCase(1, 1, 1)]
        [TestCase(-10, 1, 1)]
        [TestCase(1, -10, 1)]
        [TestCase(5, 1, 5)]
        [TestCase(1, 5, 5)]
        [TestCase(4, 4, 16)]
        public void WhenSetUpMatrixWithDifferentRowsAndColumns_ThenMatrixGeneratedWithCorrectRowsAndColumns(
            int columnCount, int rowCount, int expectedCount)
        {
            _matrixSetup = SPOGridFactory.CreateInstance(_spo, columnCount, rowCount, Vector2.one);

            _matrixSetup.CreateObjects();

            Assert.AreEqual(expectedCount, _matrixSetup.FabricatedObjects.Count);
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(10, 1)]
        [TestCase(-10, 1)]
        public void WhenSetUpMatrixWithDifferentSpacing_ThenMatrixGeneratedWithCorrectSpacing(float spacingX,
            float spacingY)
        {
            _matrixSetup = SPOGridFactory.CreateInstance(_spo, 2, 2, new Vector2(spacingX, spacingY));
            
            var expectedPositions = new Vector3[]
            {
                new(0, 0, 0),
                new(spacingX, 0, 0),
                new(0, spacingY, 0),
                new(spacingX, spacingY, 0),
            };

            _matrixSetup.CreateObjects();

            Assert.AreEqual(_matrixSetup.FabricatedObjects.Count, 4);

            for (var index = 0; index < _matrixSetup.FabricatedObjects.Count; index++)
            {
                var actualPosition = _matrixSetup.FabricatedObjects[index].transform.position;
                Assert.AreEqual(expectedPositions[index], actualPosition);
            }
        }

        [Test]
        public void WhenSetUpMatrix_ThenCameraCenteredOnMatrix()
        {
            _mainCamera.transform.position = Vector3.zero;

            _matrixSetup = SPOGridFactory.CreateInstance(_spo, 5, 5, Vector2.one);

            _matrixSetup.CreateObjects();

            Assert.AreEqual(new Vector3(2, 2, -10), _mainCamera.transform.position);
        }

        [Test]
        public void WhenSetUpMatrixAgain_ThenPreviousObjectsDestroyed()
        {
            _matrixSetup = SPOGridFactory.CreateInstance(_spo, 1, 1, Vector2.one);
            _matrixSetup.CreateObjects();
            var previousSpo = _matrixSetup.FabricatedObjects[0];
            
            _matrixSetup.CreateObjects();
            
            Assert.AreNotEqual(_matrixSetup.FabricatedObjects[0], previousSpo);
        }

        [UnityTest]
        public IEnumerator WhenDestroyMatrix_ThenGeneratedOwnedObjectsDestroyed()
        {
            _matrixSetup = SPOGridFactory.CreateInstance(_spo, 1, 1, Vector2.one);
            _matrixSetup.CreateObjects();
            var totalSceneObjectCount = Object.FindObjectsOfType<GameObject>().Length;
            
            _matrixSetup.DestroyObjects();
            yield return null;

            var objects = Object.FindObjectsOfType<GameObject>();
            Assert.AreEqual(totalSceneObjectCount - 1, objects.Length);
        }
    }
}