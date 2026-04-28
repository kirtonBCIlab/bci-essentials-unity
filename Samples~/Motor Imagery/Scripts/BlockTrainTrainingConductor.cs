using System;
using System.Collections;
using BCIEssentials;
using BCIEssentials.LSLFramework;
using UnityEngine;

[Serializable]
public class BlockTrainTrainingConductor : CoroutineWrapper, IMarkerSource
{
    public static event Action ActivePeriodStarted;
    public static event Action RestPeriodStarted;
    public static event Action OnBlockStarted;
    public static event Action OffBlockStarted;
    public static event Action CleanupInvoked;

    public MarkerWriter MarkerWriter { get; set; }
    public float OffBlockDuration = 8.0f;
    public float OnBlockDuration = 12.0f;
    public float ActivePeriodDuration = 1.0f;
    public float RestPeriodDuration = 0.5f;

    [Space]
    public float EpochLength = 2.0f;
    public int Iterations = 4;

    private Coroutine _onBlockCycleRoutine;
    private WaitForSeconds _epochDelay;
    private WaitForSeconds _activePeriodDelay, _restPeriodDelay;


    protected override IEnumerator Run()
    {
        _epochDelay = new(EpochLength);
        _activePeriodDelay = new(ActivePeriodDuration);
        _restPeriodDelay = new(RestPeriodDuration);

        float minimumBlockDuration = Mathf.Min(OnBlockDuration, OnBlockDuration);
        int epochCount = Mathf.FloorToInt(minimumBlockDuration / EpochLength);

        for (int i = 0; i < Iterations; i++)
        {
            OffBlockStarted?.Invoke();
            yield return RunTrainingEpochs(0, epochCount);
            yield return new WaitForSeconds(OffBlockDuration - minimumBlockDuration);

            OnBlockStarted?.Invoke();
            _onBlockCycleRoutine = _executionHost.StartCoroutine(RunOnBlockCycle());
            yield return RunTrainingEpochs(1, epochCount);
            yield return new WaitForSeconds(OnBlockDuration - minimumBlockDuration);

            _executionHost.StopCoroutine(_onBlockCycleRoutine);
            _onBlockCycleRoutine = null;
        }
    }

    protected override void CleanUp()
    {
        if (_onBlockCycleRoutine != null)
        {
            _executionHost.StopCoroutine(_onBlockCycleRoutine);
        }
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

    private IEnumerator RunOnBlockCycle()
    {
        while (IsRunning)
        {
            ActivePeriodStarted?.Invoke();
            yield return _activePeriodDelay;
            RestPeriodStarted?.Invoke();
            yield return _restPeriodDelay;
        }
    }
}