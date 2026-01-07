using UnityEngine;

using static Easings;

public static class PropertyTweenExtensions
{
    public static Coroutine StartPositionTween
    (
        this MonoBehaviour caller, Vector2 finalPosition, float period,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut
    )
    {
        return StartPositionTween
        (
            caller, (Vector3)finalPosition,
            period, transition, easing
        );
    }
    public static Coroutine StartPositionTween
    (
        this MonoBehaviour caller, Vector3 finalPosition, float period,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut
    )
    {
        Transform target = caller.transform;
        Vector3 initialPosition = target.localPosition;

        void callbackMethod(Vector3 tweenedPosition)
        => target.localPosition = tweenedPosition;

        return caller.StartTween
        (
            initialPosition, finalPosition,
            callbackMethod, period,
            Vector3.LerpUnclamped,
            transition, easing
        );
    }

    public static Coroutine StartScaleTween
    (
        this MonoBehaviour caller, float finalScale, float period,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut
    )
    {
        Vector2 finalScaleVector = Vector2.one * finalScale;

        return StartScaleTween
        (
            caller, finalScaleVector,
            period, transition, easing
        );
    }
    public static Coroutine StartScaleTween
    (
        this MonoBehaviour caller, Vector2 finalScale, float period,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut
    )
    {
        Transform target = caller.transform;
        Vector2 initialScale = target.localScale;

        void callbackMethod(Vector2 tweenedScale)
        => target.localScale = tweenedScale;

        return caller.StartTween
        (
            initialScale, finalScale,
            callbackMethod, period,
            Vector2.LerpUnclamped,
            transition, easing
        );
    }

    public static Coroutine StartRotationTween
    (
        this MonoBehaviour caller, float finalRotation, float period,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut
    )
    {
        Quaternion finalRotationQuaternion = Quaternion.Euler(new(0, 0, finalRotation));

        return StartRotationTween
        (
            caller, finalRotationQuaternion,
            period, transition, easing
        );
    }
    public static Coroutine StartRotationTween
    (
        this MonoBehaviour caller, Quaternion finalRotation, float period,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut
    )
    {
        Transform target = caller.transform;
        Quaternion initialRotation = target.localRotation;

        void callbackMethod(Quaternion tweenedRotation)
        => target.localRotation = tweenedRotation;

        return caller.StartTween
        (
            initialRotation, finalRotation,
            callbackMethod, period,
            Quaternion.LerpUnclamped,
            transition, easing
        );
    }
}