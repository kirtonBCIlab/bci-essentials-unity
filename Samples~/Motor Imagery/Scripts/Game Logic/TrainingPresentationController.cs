using UnityEngine;

public class TrainingPresentationController : MonoBehaviour
{
    enum CharacterState { Inactive, Sitting, Charging, Idle }

    public CharacterPresenter Character;
    public MonsterPresenter Monster;
    public ProjectilePresenter Projectile;
    [Header("Character Animation Timing")]
    public float ChargePeriod = 1.0f;
    public float IdlePeriod = 0.5f;

    private CharacterState _characterState;
    private float _characterStateTimer;


    private void Start()
    {
        Monster.Hide();
        BlockTrainTrainingConductor.OnBlockStarted += StartOnBlockDisplay;
        BlockTrainTrainingConductor.OffBlockStarted += StartOffBlockDisplay;
        BlockTrainTrainingConductor.CleanupInvoked += CleanUp;
    }

    private void StartOnBlockDisplay()
    {
        SetCharacterState(CharacterState.Charging);
        Monster.DisplayNewMonster();
    }
    private void StartOffBlockDisplay()
    {
        SetCharacterState(CharacterState.Sitting);
        Monster.Hide();
    }
    private void CleanUp()
    {
        SetCharacterState(CharacterState.Inactive);
        Monster.Hide();
    }


    private void Update()
    {
        switch (_characterState)
        {
            case CharacterState.Charging:
                CheckState(ChargePeriod, CharacterState.Idle);
                break;
            case CharacterState.Idle:
                CheckState(IdlePeriod, CharacterState.Charging);
                break;
        }
    }

    private void CheckState(float stateDuration, CharacterState nextState)
    {
        _characterStateTimer += Time.deltaTime;
        if (_characterStateTimer > stateDuration)
        {
            _characterStateTimer -= stateDuration;
            SetCharacterState(nextState);
        }
    }


    private void SetCharacterState(CharacterState newState)
    {
        _characterState = newState;
        switch (newState)
        {
            case CharacterState.Idle:
                Character.DisplayThrow();
                Projectile.DisplayProjectile();
                break;
            case CharacterState.Charging:
                Character.DisplayCharge();
                break;
            case CharacterState.Sitting:
                Character.DisplaySit();
                break;
            case CharacterState.Inactive:
                Character.DisplayIdle();
                break;
        }
    }
}