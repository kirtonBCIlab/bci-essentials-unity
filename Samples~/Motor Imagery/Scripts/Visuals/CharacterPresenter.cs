using BCIEssentials;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterPresenter : SpritePresenter
{
    [StartFoldoutGroup("Sprites")]
    public Sprite IdleSprite;
    public Sprite ChargeSprite;
    public Sprite ThrowSprite;
    public Sprite ReloadSprite;
    public Sprite SitSprite;
    public Sprite SnoozeSprite;

    [StartFoldoutGroup("Timing")]
    public float ReloadPeriod = 0.25f;
    public float ThrowPeriod = 0.1f;

    private bool _loaded;




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
        _loaded = false;
    }

    public void DisplaySitAfterThrow()
    {
        StartSpriteChain(ThrowSprite, (SitSprite, ThrowPeriod));
        _loaded = false;
    }
}