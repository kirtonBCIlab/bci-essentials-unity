using System;
using System.Collections;
using UnityEngine;

using static Easings;

public static class TweenExtensions
{
    public static Coroutine StartTween<TValue>
    (
        this MonoBehaviour caller,
        TValue initialValue, TValue finalValue,
        Action<TValue> callbackMethod, float period,
        Func<TValue, TValue, float, TValue> lerpMethod,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut)
    {
        Action<float> tweenMethod = BindTweenMethod
        (
            initialValue, finalValue,
            callbackMethod, lerpMethod,
            transition, easing
        );
        return caller.StartCoroutine(DoTween(tweenMethod, period));
    }

    private static Action<float> BindTweenMethod<TValue>
    (
        TValue startValue, TValue finalValue,
        Action<TValue> callbackMethod,
        Func<TValue, TValue, float, TValue> lerpMethod,
        TransitionType transition = TransitionType.Linear,
        EaseType easing = EaseType.EaseInOut
    )
    {
        Func<float, float> interpolationMethod = GetInterpolationMethod(transition, easing);

        return t =>
        {
            float interpolatedWeight = interpolationMethod(t);
            TValue interpolatedValue = lerpMethod
            (
                startValue, finalValue,
                interpolatedWeight
            );
            callbackMethod(interpolatedValue);
        };
    }

    private static IEnumerator DoTween
    (
        Action<float> tweenMethod, float period
    )
    {
        float timer = 0;

        while (timer < period)
        {
            float t = timer / period;

            tweenMethod(t);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        tweenMethod(1);
    }
}