using System;
using System.Threading;
using LSL;
using UnityEngine;
using static LSL.LSL;

namespace BCIEssentials.LSLFramework
{
    /** <summary>
    Utility class for finding LSL streams
    <br/><br/>
    Provides both synchronous and asynchronous methods
    <code>
    LSLStreamResolver.StartTypeResolutionThread("BCI_Essentials_Predictions", UseStreamInfo));
    </code> or
    <code>
    if (LSLStreamResolver.TryResolveByType("BCI_Essentials_Predictions", out StreamInfo resolvedStream))
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


        public static Thread StartTypeResolutionThread
        (
            string type, Action<StreamInfo> callback,
            float period = 0.1f
        )
        => StartPropertyResolutionThread("type", type, callback, period);

        public static Thread RunResolveByName
        (
            string name, Action<StreamInfo> callback,
            float period = 0.1f
        )
        => StartPropertyResolutionThread("name", name, callback, period);

        public static Thread StartPropertyResolutionThread
        (
            string propertyName, string propertyValue,
            Action<StreamInfo> callback,
            float period = 0.1f
        )
        => StartPredicateResolutionThread
        (
            BuildPredicate(propertyName, propertyValue),
            callback, period
        );

        public static Thread StartPredicateResolutionThread
        (
            string predicate, Action<StreamInfo> callback,
            float period = 0.1f
        )
        {
            if (!PredicateIsValid(predicate)) return null;

            Thread resolutionThread = new(() => RunResolutionThread(predicate, period, callback));
            resolutionThread.Start();
            return resolutionThread;
        }

        private static void RunResolutionThread
        (
            string predicate, float period,
            Action<StreamInfo> callback
        )
        {
            StreamInfo resolvedStreamInfo;
            while (!TryResolve(predicate, out resolvedStreamInfo)) SleepForSeconds(period);
            callback(resolvedStreamInfo);
        }

        public static void SleepForSeconds(float seconds)
        => Thread.Sleep((int)(seconds * 1000));


        private static bool TryResolve
        (
            string predicate, out StreamInfo resolvedStreamInfo
        )
        {
            resolvedStreamInfo = resolve_stream(predicate, 1, 0) switch
            {
                StreamInfo[] resolvedStreamInfos and { Length: > 0 }
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