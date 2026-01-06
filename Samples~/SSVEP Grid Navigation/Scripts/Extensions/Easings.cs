using UnityEngine;
using System;

public static class Easings
{
    public enum TransitionType {
        Linear,
        Sine,
        Cubic,
        Expo,
        Back,
        Elastic
    }

    public static Type Transition = typeof(TransitionType);

    public enum EaseType
    {
        EaseOut,
        EaseIn,
        EaseInOut
    }


    public static Func<float, float> GetInterpolationMethod
    (
        TransitionType transition, EaseType easing
    )
    {
        Func<float, float> interpolationMethod = transition switch
        {
            TransitionType.Linear => EaseOutLinear,
            TransitionType.Sine => EaseOutSine,
            TransitionType.Cubic => EaseOutCubic,
            TransitionType.Expo => EaseOutExpo,
            TransitionType.Back => EaseOutBack,
            TransitionType.Elastic => EaseOutElastic,
            _ => EaseOutLinear
        };

        return easing switch
        {
            EaseType.EaseIn => GetEaseInMethod(interpolationMethod),
            EaseType.EaseInOut => GetEaseInOutMethod(interpolationMethod),
            _ => interpolationMethod,
        };
    }

    public static Func<float, float> GetEaseInMethod(Func<float, float> easeOutMethod)
    => t =>  1 - easeOutMethod(1 - t);

    public static Func<float, float> GetEaseInOutMethod(Func<float, float> easeOutMethod)
    => t => (t < 0.5)
            ? (1 - easeOutMethod(1 - 2 * t) / 2)
            : (1 + easeOutMethod(2 * t - 1) / 2);


    public static float EaseOutLinear(float t) => t;
    public static float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI / 2);
    public static float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
    public static float EaseOutExpo(float t) => t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
    public static float EaseOutBack(float t)
    {
        const float constant = 1.70158f;

        float v1 = (constant + 1) * Mathf.Pow(t - 1, 3);
        float v2 = constant * Mathf.Pow(t - 1, 2);
        return 1 + v1 + v2;
    }
    public static float EaseOutElastic(float t)
    {
        const float constant = 2 * Mathf.PI / 3;

        return t == 0
            ? 0
            : t == 1
            ? 1
            : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * constant) + 1;
    }
}