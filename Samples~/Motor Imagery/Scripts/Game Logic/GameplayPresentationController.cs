using System.Collections;
using BCIEssentials.Utilities;
using UnityEngine;

public class GameplayPresentationController : MonoBehaviour
{
    public ClassificationProvider InputProvider;
    public KeyBind InputKey = KeyCode.Space;
    public CharacterPresenter Character;
    public MonsterPresenter Monster;

    public float RestPeriod = 8;
    public float ChargePeriod = 2;
    public int CaptureThreshold = 3;
    [Range(0, 1)] public float DrainRate = 0.5f;

    private int _throws;
    private float _chargeLevel;
    private bool _isResting = false;


    private void Update()
    {
        if (_isResting || !InputProvider.IsRunning) return;
        if (InputProvider.InputValue || InputKey.IsPressed)
        {
            AddFrameTimeToChargeLevel();
        }
        else if (_chargeLevel >= 1) Throw();
        else
        {
            DrainFrameTimeFromChargeLevel();
        }
        _chargeLevel = Mathf.Clamp01(_chargeLevel);
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


    private void AddFrameTimeToChargeLevel()
    {
        if (_chargeLevel == 0) Character.DisplayCharge();
        _chargeLevel += Time.deltaTime / ChargePeriod;
    }
    private void DrainFrameTimeFromChargeLevel()
    {
        float oldChargeLevel = _chargeLevel;
        _chargeLevel -= DrainRate * Time.deltaTime / ChargePeriod;
        if (_chargeLevel <= 0 && oldChargeLevel > 0) Character.DisplayIdle();
    }
}