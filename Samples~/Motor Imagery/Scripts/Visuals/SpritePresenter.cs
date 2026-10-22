using System.Collections;
using BCIEssentials;
using BCIEssentials.Extensions;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePresenter : MonoBehaviourWithFoldoutGroups
{
    private Coroutine _delayedSpriteChainRoutine;
    private SpriteRenderer Renderer
    => this.CoalesceComponentReference(ref _renderer);
    private SpriteRenderer _renderer;


    public void Hide() => Renderer.enabled = false;
    public void Show() => Renderer.enabled = true;


    public void SetSprite(Sprite sprite)
    {
        Renderer.sprite = sprite;
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