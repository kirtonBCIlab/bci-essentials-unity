

using System;
using System.Collections;
using BCIEssentials.Behaviours.Training;
using UnityEngine;

public class BlockTrainTrainingBehaviour: TrainingBehaviour
{
    public static event Action ActivePeriodStarted;
    public static event Action RestPeriodStarted;
    public static event Action OnBlockStarted;
    public static event Action OffBlockStarted;
    public static event Action CleanupInvoked;

    public float OffBlockDuration = 8.0f;
    public float OnBlockDuration => (ActivePeriodDuration + RestPeriodDuration) * ActivePeriodsPerOnBlock;
    public float ActivePeriodDuration = 2.0f;
    public float RestPeriodDuration = 2.0f;
    public int ActivePeriodsPerOnBlock = 3;

    public float Iterations = 4;


    protected override IEnumerator Run()
    {
        float epochLength = GetCommonDivisor(ActivePeriodDuration, OffBlockDuration);
        WaitForSeconds epochDelay = new(epochLength);

        int offBlockEpochCount = Mathf.FloorToInt(OffBlockDuration / epochLength);
        int activePeriodEpochCount = Mathf.FloorToInt(ActivePeriodDuration / epochLength);

        for (int i = 0; i < Iterations; i++)
        {
            OffBlockStarted?.Invoke();
            yield return RunTrainingEpochs(0, epochLength, offBlockEpochCount);

            OnBlockStarted?.Invoke();
            for (int c = 0; c < ActivePeriodsPerOnBlock; c++)
            {
                ActivePeriodStarted?.Invoke();
                yield return RunTrainingEpochs(1, epochLength, activePeriodEpochCount);

                RestPeriodStarted?.Invoke();
                yield return new WaitForSeconds(RestPeriodDuration);
            }
        }
    }

    protected override void CleanUp() => CleanupInvoked?.Invoke();


    private IEnumerator RunTrainingEpochs
    (int trainingTarget, float epochLength, int epochCount)
    {
        WaitForSeconds epochDelay = new(epochLength);
        for (int i = 0; i < epochCount; i++)
        {
            MarkerWriter.PushMITrainingMarker(2, trainingTarget, epochLength);
            yield return epochDelay;
        }
    }


    public static float GetCommonDivisor(float a, float b)
    {
        if (a == b)
            return a;

        float max = Mathf.Max(a, b);
        float min = Mathf.Min(a, b);

        if (max % min == 0) return min;
        if (1 / min % max == 0) return 1 / min;
        if (1 / max % min == 0) return 1 / max;
        return 1 / (min * max);
    }
}