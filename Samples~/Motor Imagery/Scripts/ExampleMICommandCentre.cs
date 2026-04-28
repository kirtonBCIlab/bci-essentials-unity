using UnityEngine;
using UnityEngine.Events;

public class ExampleMiCommandCentre: MonoBehaviour
{
    public BlockTrainTrainingConductor TrainingConductor;
    public ClassificationPollingConductor ClassificationPollingConductor;
    [SerializeField, Space] private UnityEvent _onTrainingCompleted;

    private void Reset()
    {
        TrainingConductor = new(this);
        ClassificationPollingConductor = new(this);
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