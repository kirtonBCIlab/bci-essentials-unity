using BCIEssentials.Extensions;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MonsterPresenter : SpritePresenter
{
    public Sprite[] MonsterSprites;
    private Sprite _lastShownMonster;

    public void ShowNewMonster()
    {
        Sprite newMonster = MonsterSprites.PickRandomExcluding(_lastShownMonster);
        SetSprite(newMonster);
        _lastShownMonster = newMonster;
    }
}