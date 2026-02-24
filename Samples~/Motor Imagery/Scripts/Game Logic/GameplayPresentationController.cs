using System.Collections;
using BCIEssentials.Utilities;
using UnityEngine;

public class GameplayPresentationController : MonoBehaviour
{
    public ClassificationProvider InputProvider;
    public KeyBind InputKey = KeyCode.Space;
    public CharacterPresenter Character;
    public MonsterPresenter Monster;
    public ProjectilePresenter Projectile;
    public ChargeLevelPresenter ChargeBar;

    public float RestPeriod = 8;
    public float ChargePeriod = 2;
    public int CaptureThreshold = 3;
    [Range(0, 1)] public float DrainRate = 0.5f;

    private int _throws;
    private float _chargeLevel;
    private bool _isResting = false;


    private void Start()
    {
        InputProvider.ClassificationStarted += Monster.DisplayNewMonster;
        InputProvider.ClassificationStarted += ChargeBar.Show;
        InputProvider.ClassificationStarted += () => _chargeLevel = 0;

        InputProvider.ClassificationEnded += Character.DisplayIdle;
        InputProvider.ClassificationEnded += Monster.Hide;
        InputProvider.ClassificationEnded += ChargeBar.Hide;
    }


    private void Update()
    {
        if (_isResting || !InputProvider.IsRunning) return;

        float inputMultiplier = 2 * InputProvider.InputValue - 1;
        if (InputKey.IsPressed)
        {
            AddFrameTimeToChargeLevel();
        }
        else if (inputMultiplier > 0)
        {
            AddFrameTimeToChargeLevel(inputMultiplier);
        }
        else if (_chargeLevel >= 1) Throw();
        else
        {
            DrainFrameTimeFromChargeLevel(-inputMultiplier);
        }
        _chargeLevel = Mathf.Clamp01(_chargeLevel);
        ChargeBar.DisplayChargeLevel(_chargeLevel);
    }

    private void Throw()
    {
        _chargeLevel = 0;
        if (++_throws >= CaptureThreshold)
        {
            Monster.Hide();
            Character.DisplaySitAfterThrow();
            StartCoroutine(RunRestPeriod());
        }
        else Character.DisplayThrow();
        Projectile.DisplayProjectile();
    }

    private IEnumerator RunRestPeriod()
    {
        _isResting = true;
        yield return new WaitForSeconds(RestPeriod);
        _throws = 0;
        Monster.DisplayNewMonster();
        Character.DisplayIdle();
        _isResting = false;
    }


    private void AddFrameTimeToChargeLevel(float multiplier = 1)
    {
        if (_chargeLevel == 0) Character.DisplayCharge();
        _chargeLevel += multiplier * Time.deltaTime / ChargePeriod;
    }
    private void DrainFrameTimeFromChargeLevel( float multiplier = 1)
    {
        float oldChargeLevel = _chargeLevel;
        _chargeLevel -= DrainRate * multiplier * Time.deltaTime / ChargePeriod;
        if (_chargeLevel <= 0 && oldChargeLevel > 0) Character.DisplayIdle();
    }
}