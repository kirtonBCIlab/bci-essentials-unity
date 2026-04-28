using BCIEssentials.LSLFramework;
using UnityEngine;
using UnityEngine.Events;

public class ExampleMiCommandCentre: MonoBehaviour
{
    public BlockTrainTrainingConductor TrainingConductor;
    public ClassificationPollingConductor ClassificationPollingConductor;

    [Header("Communication")]
    [SerializeField] private MarkerWriter _markerWriter;
    [SerializeField] private ResponseProvider _responseProvider;
    [SerializeField, Space] private UnityEvent _onTrainingCompleted;


    private void Awake()
    {
        TrainingConductor.MarkerWriter ??= _markerWriter;
        ClassificationPollingConductor.MarkerWriter ??= _markerWriter;
        _responseProvider.SubscribePredictions(ClassificationPollingConductor.OnPrediction);

        BlockTrainTrainingConductor.CleanupInvoked += _onTrainingCompleted.Invoke;
    }
    private void OnDestroy()
    {
        BlockTrainTrainingConductor.CleanupInvoked -= _onTrainingCompleted.Invoke;
    }


    public void StartTraining() => TrainingConductor.Begin(this);
    public void StopTraining() => TrainingConductor.Interrupt();

    public void StartClassifying() => ClassificationPollingConductor.Begin(this);
    public void StopClassifying() => ClassificationPollingConductor.Interrupt();
}