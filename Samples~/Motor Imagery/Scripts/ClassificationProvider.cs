using System;
using System.Collections;
using BCIEssentials.Behaviours;
using BCIEssentials.LSLFramework;
using BCIEssentials.Selection;
using UnityEngine;

public class ClassificationProvider : CoroutineBehaviour, IBCIMarkerSource, IPredictionSink
{
    public event Action ClassificationStarted;
    public event Action ClassificationEnded;

    public MarkerWriter MarkerWriter { get; set; }
    public bool InputValue { get; private set; }
    public float EpochLength = 0.5f;


    public void OnPrediction(Prediction prediction)
    {
        InputValue = prediction.Index > 0;
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


    protected override void SetUp() => ClassificationStarted?.Invoke();
    protected override void CleanUp() => ClassificationEnded?.Invoke();
}