using BCIEssentials.StimulusObjects;
using NUnit.Framework;
using UnityEngine;

namespace BCIEssentials.Tests.Editor
{
    internal class SPOTests
    {
        [Test]
        public void WhenComponentAttached_ThenRequiredComponentsAttached()
        {
            var gameObject = new GameObject();

            gameObject.AddComponent<SPO>();
            
            Assert.IsNotNull(gameObject.GetComponent<MeshRenderer>());
        }
    }
}