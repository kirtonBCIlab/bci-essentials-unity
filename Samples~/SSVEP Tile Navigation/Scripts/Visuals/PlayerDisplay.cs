using UnityEngine;
using static Easings;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDisplay : GridSortedDisplay
{
    public float _movementPeriod = 0.75f;
    public TransitionType _movementTransition = TransitionType.Elastic;
    public EaseType _movementEasing = EaseType.EaseOut;

    private Coroutine _movementTween;

    private TraversableTilemap _map;

    private void Start()
    {
        _map = FindAnyObjectByType<TraversableTilemap>();
        MovementHandler.MovementAchieved += AnimateMovement;
        MovementHandler.MovementAchieved += UpdateSortOrder;
    }
    private void OnDestroy()
    {
        MovementHandler.MovementAchieved -= AnimateMovement;
        MovementHandler.MovementAchieved -= UpdateSortOrder;
    }


    private void AnimateMovement(Vector3Int newGridPosition)
    {
        if (_movementTween != null) StopCoroutine(_movementTween);

        Vector3 worldPosition = _map.GetCellCentre(newGridPosition);
        _movementTween = this.StartPositionTween
        (
            worldPosition, _movementPeriod,
            _movementTransition, _movementEasing
        );
    }
}