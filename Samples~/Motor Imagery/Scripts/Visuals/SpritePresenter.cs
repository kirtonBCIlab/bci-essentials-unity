using System.Collections;
using BCIEssentials;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePresenter : MonoBehaviourUsingExtendedAttributes
{
    private Coroutine _delayedSpriteChainRoutine;
    private SpriteRenderer _renderer;
    void Reset()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }


    public void Hide() => _renderer.enabled = false;
    public void Show() => _renderer.enabled = true;


    public void SetSprite(Sprite sprite)
    {
        _renderer.sprite = sprite;
    }

    public void StartSpriteChain(Sprite immediateSprite, params (Sprite, float)[] chain)
    {
        if (_delayedSpriteChainRoutine != null)
        {
            StopCoroutine(_delayedSpriteChainRoutine);
        }
        SetSprite(immediateSprite);
        _delayedSpriteChainRoutine = StartCoroutine(RunDelayedSpriteChain(chain));
    }
    public IEnumerator RunDelayedSpriteChain(params (Sprite, float)[] chain)
    {
        foreach (var (sprite, delay) in chain)
        {
            yield return new WaitForSeconds(delay);
            SetSprite(sprite);
        }
        _delayedSpriteChainRoutine = null;
    }
}