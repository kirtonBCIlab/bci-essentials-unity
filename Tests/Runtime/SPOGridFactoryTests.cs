using System.Collections;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests.Utilities
{
    internal class SPOGridFactoryTests : PlayModeTestRunnerBase
    {
        private Camera _mainCamera;
        private SPOGridFactory _gridFactory;
        private SPO _spo;

        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            yield return base.TestSetup();

            _gridFactory = SPOGridFactory.CreateInstance();
            _spo = new GameObject().AddComponent<SPO>();
            _mainCamera = new GameObject().AddComponent<Camera>();
            _mainCamera.tag = "MainCamera";
        }

        [Test]
        public void WhenCreateObjectsWithNullSPO_ThenNoObjectsGenerated()
        {
            Utilities.LogAssert.ExpectStartingWith(LogType.Error, "No SPO");
            
            _gridFactory.CreateObjects();

            Assert.AreEqual(0, _gridFactory.FabricatedObjects.Count);
        }

        [Test]
        [TestCase(1, 1, 1)]
        [TestCase(-10, 1, 1)]
        [TestCase(1, -10, 1)]
        [TestCase(5, 1, 5)]
        [TestCase(1, 5, 5)]
        [TestCase(4, 4, 16)]
        public void WhenCreateObjectsWithDifferentRowsAndColumns_ThenCorrectNumberOfObjectsCreated(
            int columnCount, int rowCount, int expectedCount)
        {
            _gridFactory = SPOGridFactory.CreateInstance(_spo, columnCount, rowCount, Vector2.one);

            _gridFactory.CreateObjects();

            Assert.AreEqual(expectedCount, _gridFactory.FabricatedObjects.Count);
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(10, 1)]
        [TestCase(-10, 1)]
        public void WhenCreateObjectsWithDifferentSpacing_ThenObjectsGeneratedWithCorrectSpacing(float spacingX,
            float spacingY)
        {
            _gridFactory = SPOGridFactory.CreateInstance(_spo, 2, 2, new Vector2(spacingX, spacingY));
            
            var expectedPositions = new Vector3[]
            {
                new(0, 0, 0),
                new(spacingX, 0, 0),
                new(0, spacingY, 0),
                new(spacingX, spacingY, 0),
            };

            _gridFactory.CreateObjects();

            Assert.AreEqual(_gridFactory.FabricatedObjects.Count, 4);

            for (var index = 0; index < _gridFactory.FabricatedObjects.Count; index++)
            {
                var actualPosition = _gridFactory.FabricatedObjects[index].transform.position;
                Assert.AreEqual(expectedPositions[index], actualPosition);
            }
        }

        [Test]
        public void WhenCreateObjects_ThenCameraCenteredOnObjects()
        {
            _mainCamera.transform.position = Vector3.zero;

            _gridFactory = SPOGridFactory.CreateInstance(_spo, 5, 5, Vector2.one);

            _gridFactory.CreateObjects();

            Assert.AreEqual(new Vector3(2, 2, -10), _mainCamera.transform.position);
        }

        [Test]
        public void WhenCreateObjectsAgain_ThenPreviousObjectsDestroyed()
        {
            _gridFactory = SPOGridFactory.CreateInstance(_spo, 1, 1, Vector2.one);
            _gridFactory.CreateObjects();
            var previousSpo = _gridFactory.FabricatedObjects[0];
            
            _gridFactory.CreateObjects();
            
            Assert.AreNotEqual(_gridFactory.FabricatedObjects[0], previousSpo);
        }

        [UnityTest]
        public IEnumerator WhenDestroyObjects_ThenFabricatedObjectsDestroyed()
        {
            _gridFactory = SPOGridFactory.CreateInstance(_spo, 1, 1, Vector2.one);
            _gridFactory.CreateObjects();
            var totalSceneObjectCount = Object.FindObjectsOfType<GameObject>().Length;
            
            _gridFactory.DestroyObjects();
            yield return null;

            var objects = Object.FindObjectsOfType<GameObject>();
            Assert.AreEqual(totalSceneObjectCount - 1, objects.Length);
        }
    }
}