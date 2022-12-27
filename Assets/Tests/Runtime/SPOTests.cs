using System;
using System.Collections;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

internal class SPOTests : PlayModeTestRunnerBase
{
    private SPO _testSpo;
    private MeshRenderer _testSpoRenderer;
    
    [UnitySetUp]
    public override IEnumerator TestSetup()
    {
        yield return base.TestSetup();

        _testSpo = new GameObject().AddComponent<SPO>();
        _testSpoRenderer = _testSpo.GetComponent<MeshRenderer>();
    }

    [Test]
    public void WhenTurnOn_ThenReturnsTime()
    {
        var expectedResult = Time.time;
        
        var result = _testSpo.TurnOn();
        
        Assert.AreEqual(expectedResult, result);
    }

    [Test]
    public void WhenTurnOn_ThenMaterialColorIsOnColor()
    {
        _testSpo.onColour = Color.blue;
        _testSpoRenderer.material.color = Color.green;

        _testSpo.TurnOn();
        
        Assert.AreEqual(Color.blue, _testSpoRenderer.material.color);
    }

    [Test]
    public void WhenTurnOff_ThenMaterialColorIsOffColor()
    {
        _testSpo.offColour = Color.blue;
        _testSpoRenderer.material.color = Color.green;

        _testSpo.TurnOff();
        
        Assert.AreEqual(Color.blue, _testSpoRenderer.material.color);
    }

    [Test]
    public void WhenOnTrainTarget_ThenLocalScaleIncreased()
    {
        _testSpo.transform.localScale = new Vector3(5, 5, 5);
        var expectedScale = Vector3.one * 5 * 1.4f; //1.4 is a magic number used by the SPO
        
        _testSpo.OnTrainTarget();
        
        Assert.AreEqual(expectedScale, _testSpo.transform.localScale);
    }

    [Test]
    public void WhenOnTrainTarget_ThenLocalScaleDecreased()
    {
        _testSpo.transform.localScale = new Vector3(5, 5, 5);
        var expectedScale = Vector3.one * 5 / 1.4f; //1.4 is a magic number used by the SPO
        
        _testSpo.OffTrainTarget();
        
        Assert.AreEqual(expectedScale, _testSpo.transform.localScale);
    }

    [Test]
    public void WhenOnSelection_ThenStimulusEffectRan()
    {
        _testSpo.onColour = Color.red;
        _testSpo.offColour = Color.blue;
        _testSpoRenderer.material.color = Color.green;
            
        _testSpo.OnSelection();

        Assert.AreEqual(Color.blue, _testSpoRenderer.material.color);
    }

    [UnityTest]
    public IEnumerator WhenQuickFlash_ThenStimulusEffectRan()
    {
        _testSpo.onColour = Color.red;
        _testSpo.offColour = Color.blue;
        _testSpoRenderer.material.color = Color.green;

        yield return _testSpo.QuickFlash();

        Assert.AreEqual(Color.blue, _testSpoRenderer.material.color);
    }
}
