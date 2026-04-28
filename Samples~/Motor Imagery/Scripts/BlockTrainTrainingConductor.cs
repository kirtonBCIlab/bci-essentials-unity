using System;
using System.Collections;
using BCIEssentials;
using BCIEssentials.LSLFramework;
using UnityEngine;

[Serializable]
public class BlockTrainTrainingConductor : CoroutineWrapper, IMarkerSource
{
    public static event Action OnBlockStarted;
    public static event Action OffBlockStarted;
    public static event Action CleanupInvoked;

    public MarkerWriter MarkerWriter { get; set; }

    [Space]
    public float EpochLength = 2.0f;
    public int Iterations = 4;
    public float BlockDuration = 8.0f;

    private WaitForSeconds _epochDelay;


    protected override IEnumerator Run()
    {
        _epochDelay = new(EpochLength);

        int epochCount = Mathf.FloorToInt(BlockDuration / EpochLength);
        float unmarkedBlockTime = BlockDuration - epochCount * EpochLength;
        WaitForSeconds blockTimeBufferDelay = new(unmarkedBlockTime);

        for (int i = 0; i < Iterations; i++)
        {
            OffBlockStarted?.Invoke();
            MarkerWriter.PushTrialStartedMarker();
            yield return RunTrainingEpochs(0, epochCount);
            yield return blockTimeBufferDelay;

            OnBlockStarted?.Invoke();
            yield return RunTrainingEpochs(1, epochCount);
            yield return blockTimeBufferDelay;
            MarkerWriter.PushTrialEndsMarker();
        }
    }

    protected override void CleanUp()
    {
        MarkerWriter.PushTrainingCompleteMarker();
        CleanupInvoked?.Invoke();
    }


    private IEnumerator RunTrainingEpochs
    (int trainingTarget, int epochCount)
    {
        for (int i = 0; i < epochCount; i++)
        {
            MarkerWriter.PushMITrainingMarker(2, trainingTarget, EpochLength);
            yield return _epochDelay;
        }
    }
}