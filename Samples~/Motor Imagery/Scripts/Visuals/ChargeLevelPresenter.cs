using BCIEssentials.Extensions;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ChargeLevelPresenter : MonoBehaviour
{
    private RectTransform Rect
    => this.CoalesceComponentReference(ref _rect);
    private RectTransform _rect;


    public void DisplayChargeLevel(float t)
    {
        Rect.anchorMax = new(t, 1);
        Rect.ForceUpdateRectTransforms();
    }
}