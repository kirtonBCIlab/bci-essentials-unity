using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MonsterPresenter : SpritePresenter
{
    public Sprite[] MonsterSprites;
    private Sprite _lastShownMonster;

    public void ShowNewMonster()
    {
        Sprite[] possibleMonsters = MonsterSprites.Where(m => m != _lastShownMonster).ToArray();
        Sprite newMonster = possibleMonsters[Random.Range(0, possibleMonsters.Length)];
        SetSprite(newMonster);
        _lastShownMonster = newMonster;
    }
}