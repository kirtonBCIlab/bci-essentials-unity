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

    private void Reset()
    {
        _markerWriter = new();
        _responseProvider = new();
        TrainingConductor = new(this) { MarkerWriter = _markerWriter };
        ClassificationPollingConductor = new(this) { MarkerWriter = _markerWriter };
        _responseProvider.SubscribePredictions(ClassificationPollingConductor.OnPrediction);
    }

    private void Start()
    => BlockTrainTrainingConductor.CleanupInvoked += _onTrainingCompleted.Invoke;
    private void OnDestroy()
    => BlockTrainTrainingConductor.CleanupInvoked -= _onTrainingCompleted.Invoke;


    public void StartTraining() => TrainingConductor.Begin();
    public void StopTraining() => TrainingConductor.Interrupt();

    public void StartClassifying() => ClassificationPollingConductor.Begin();
    public void StopClassifying() => ClassificationPollingConductor.Interrupt();
}