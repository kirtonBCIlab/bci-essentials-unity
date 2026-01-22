using System.Collections;
using BCIEssentials.Behaviours;
using BCIEssentials.LSLFramework;
using BCIEssentials.Selection;
using UnityEngine;

public class ClassificationProvider : CoroutineBehaviour, IBCIMarkerSource, ISelector
{
    public LSLMarkerWriter MarkerWriter { get; set; }
    public bool InputValue { get; private set; }
    public float EpochLength = 0.5f;


    public void MakeSelection(int index)
    {
        InputValue = index > 0;
    }


    protected override IEnumerator Run()
    {
        WaitForSeconds epochDelay = new(EpochLength);
        while (true)
        {
            MarkerWriter.PushMIClassificationMarker(2, EpochLength);
            yield return epochDelay;
        }
    }
}