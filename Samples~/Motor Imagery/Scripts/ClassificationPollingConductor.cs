using System;
using System.Collections;
using BCIEssentials.LSLFramework;
using BCIEssentials;
using UnityEngine;

[Serializable]
public class ClassificationPollingConductor : CoroutineWrapper, IMarkerSource, IPredictionSink
{
    public event Action ClassificationStarted;
    public event Action ClassificationEnded;

    public MarkerWriter MarkerWriter { get; set; }
    public float InputValue { get; private set; }
    public float EpochLength = 2.0f;

    public ClassificationPollingConductor(MonoBehaviour executionHost) : base(executionHost) { }


    public void OnPrediction(Prediction prediction)
    {
        InputValue = prediction.Probabilities[1];
    }


    protected override IEnumerator Run()
    {
        InputValue = 0;
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