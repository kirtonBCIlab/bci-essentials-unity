using UnityEngine;

public class TrainingPresentationController : MonoBehaviour
{
    public CharacterPresenter Character;
    public MonsterPresenter Monster;
    public ProjectilePresenter Projectile;

    private void Start()
    {
        Monster.Hide();
        BlockTrainTrainingConductor.ActivePeriodStarted += Character.DisplayCharge;
        BlockTrainTrainingConductor.RestPeriodStarted += Character.DisplayThrow;
        BlockTrainTrainingConductor.RestPeriodStarted += Projectile.DisplayProjectile;
        BlockTrainTrainingConductor.OffBlockStarted += Character.DisplaySit;
        BlockTrainTrainingConductor.CleanupInvoked += Character.DisplayIdle;

        BlockTrainTrainingConductor.OnBlockStarted += Monster.DisplayNewMonster;
        BlockTrainTrainingConductor.OffBlockStarted += Monster.Hide;
        BlockTrainTrainingConductor.CleanupInvoked += Monster.Hide;
    }
}