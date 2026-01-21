using System.Collections;
using BCIEssentials;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterPresenter : MonoBehaviourUsingExtendedAttributes
{
    [StartFoldoutGroup("Sprites")]
    public Sprite IdleSprite;
    public Sprite ChargeSprite;
    public Sprite ThrowSprite;
    public Sprite ReloadSprite;
    public Sprite SitSprite;
    public Sprite SnoozeSprite;

    [StartFoldoutGroup("Timing")]
    public float ReloadPeriod = 0.1f;
    public float ThrowPeriod = 0.05f;

    private SpriteRenderer _renderer;
    private Coroutine _delayedSpriteChainRoutine;
    private bool _loaded;


    void Reset()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = IdleSprite;
    }


    public void DisplayIdle() => SetSprite(IdleSprite);
    public void DisplaySit() => SetSprite(SitSprite);
    public void DisplaySnooze() => SetSprite(SnoozeSprite);

    public void DisplayCharge()
    {
        if (!_loaded)
        {
            StartSpriteChain(ReloadSprite, (ChargeSprite, ReloadPeriod));
            _loaded = true;
        }
        else SetSprite(ChargeSprite);
    }

    public void DisplayThrow()
    {
        StartSpriteChain(ThrowSprite, (IdleSprite, ThrowPeriod));
    }


    private void SetSprite(Sprite sprite)
    {
        _renderer.sprite = sprite;
    }

    private void StartSpriteChain(Sprite immediateSprite, params (Sprite, float)[] chain)
    {
        if (_delayedSpriteChainRoutine != null)
        {
            StopCoroutine(_delayedSpriteChainRoutine);
        }
        SetSprite(immediateSprite);
        _delayedSpriteChainRoutine = StartCoroutine(RunDelayedSpriteChain(chain));
    }
    private IEnumerator RunDelayedSpriteChain(params (Sprite, float)[] chain)
    {
        foreach (var (sprite, delay) in chain)
        {
            yield return new WaitForSeconds(delay);
            SetSprite(sprite);
        }
        _delayedSpriteChainRoutine = null;
    }
}