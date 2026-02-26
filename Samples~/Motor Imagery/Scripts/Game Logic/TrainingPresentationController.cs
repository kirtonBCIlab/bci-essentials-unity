using UnityEngine;

public class TrainingPresentationController : MonoBehaviour
{
    public CharacterPresenter Character;
    public MonsterPresenter Monster;
    public ProjectilePresenter Projectile;

    private void Start()
    {
        Monster.Hide();
        BlockTrainTrainingBehaviour.ActivePeriodStarted += Character.DisplayCharge;
        BlockTrainTrainingBehaviour.RestPeriodStarted += Character.DisplayThrow;
        BlockTrainTrainingBehaviour.OffBlockStarted += Character.DisplaySitAfterThrow;
        BlockTrainTrainingBehaviour.RestPeriodStarted += Projectile.DisplayProjectile;
        BlockTrainTrainingBehaviour.OffBlockStarted += Projectile.DisplayProjectile;
        BlockTrainTrainingBehaviour.CleanupInvoked += Character.DisplayIdle;

        BlockTrainTrainingBehaviour.OnBlockStarted += Monster.DisplayNewMonster;
        BlockTrainTrainingBehaviour.OffBlockStarted += Monster.Hide;
        BlockTrainTrainingBehaviour.CleanupInvoked += Monster.Hide;
    }
}