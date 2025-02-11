using LSL;
using static LSL.LSL;
using UnityEngine;
using System.Collections;
using System;

namespace BCIEssentials.LSLFramework
{
    /** <summary>
    Utility class for finding LSL streams
    <br/><br/>
    Provides both synchronous and asynchronous methods
    <code>
    StartCoroutine(LSLStreamResolver.RunResolveByType("BCI", UseStreamInfo));
    </code> or
    <code>
    if (LSLStreamResolver.TryResolveByType("BCI", out StreamInfo resolvedStream))
    {
        UseStreamInfo(resolvedStreamInfo);
    }
    </code>
    </summary> **/
    public static class LSLStreamResolver
    {
        public static bool TryResolveByType
        (
            string type, out StreamInfo resolvedStreamInfo
        )
        => TryResolveByProperty("type", type, out resolvedStreamInfo);

        public static bool TryResolveByName
        (
            string name, out StreamInfo resolvedStreamInfo
        )
        => TryResolveByProperty("name", name, out resolvedStreamInfo);
        
        public static bool TryResolveByProperty
        (
            string propertyName, string propertyValue, 
            out StreamInfo resolvedStreamInfo
        ) => TryResolveByPredicate
        (
            BuildPredicate(propertyName, propertyValue),
            out resolvedStreamInfo
        );

        public static bool TryResolveByPredicate
        (
            string predicate, out StreamInfo resolvedStreamInfo
        )
        {
            resolvedStreamInfo = null;
            if (!PredicateIsValid(predicate)) return false;
            return TryResolve(predicate, out resolvedStreamInfo);
        }


        public static IEnumerator RunResolveByType
        (
            string type, Action<StreamInfo> callback,
            float period = 0.1f
        )
        => RunResolveByProperty("type", type, callback, period);

        public static IEnumerator RunResolveByName
        (
            string name, Action<StreamInfo> callback,
            float period = 0.1f
        )
        => RunResolveByProperty("name", name, callback, period);

        public static IEnumerator RunResolveByProperty
        (
            string propertyName, string propertyValue,
            Action<StreamInfo> callback,
            float period = 0.1f
        )
        => RunResolveByPredicate
        (
            BuildPredicate(propertyName, propertyValue),
            callback, period
        );

        public static IEnumerator RunResolveByPredicate
        (
            string predicate, Action<StreamInfo> callback,
            float period = 0.1f
        )
        {
            if (!PredicateIsValid(predicate)) yield break;

            StreamInfo resolvedStreamInfo;
            while (!TryResolve(predicate, out resolvedStreamInfo))
                yield return new WaitForSeconds(period);

            callback(resolvedStreamInfo);
        }


        private static bool TryResolve
        (
            string predicate, out StreamInfo resolvedStreamInfo
        )
        {
            resolvedStreamInfo = resolve_stream(predicate, 1, 0) switch 
            {
                StreamInfo[] resolvedStreamInfos and {Length: > 0}
                    => resolvedStreamInfos[0]
                ,
                _ => null
            };
            return resolvedStreamInfo is not null;
        }


        private static string BuildPredicate
        (
            string propertyName, string propertyValue
        )
        => $"{propertyName}='{propertyValue}'";

        private static bool PredicateIsValid(string predicate)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                Debug.LogWarning("Can't resolve a stream with no information.");
                return false;
            }
            return true;
        }
    }
}