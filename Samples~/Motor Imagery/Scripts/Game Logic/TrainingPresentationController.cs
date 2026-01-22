using UnityEngine;

public class TrainingPresentationController : MonoBehaviour
{
    public CharacterPresenter Character;
    public MonsterPresenter Monster;

    private void Start()
    {
        BlockTrainTrainingBehaviour.ActivePeriodStarted += Character.DisplayCharge;
        BlockTrainTrainingBehaviour.RestPeriodStarted += Character.DisplayThrow;
        BlockTrainTrainingBehaviour.OffBlockStarted += Character.DisplaySit;
        BlockTrainTrainingBehaviour.CleanupInvoked += Character.DisplayIdle;

        BlockTrainTrainingBehaviour.OnBlockStarted += Monster.DisplayNewMonster;
        BlockTrainTrainingBehaviour.OffBlockStarted += Monster.Hide;
        BlockTrainTrainingBehaviour.CleanupInvoked += Monster.Hide;
    }
}