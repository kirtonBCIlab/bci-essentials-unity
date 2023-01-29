using System.Text.RegularExpressions;
using UnityEngine;

namespace BCIEssentials.Tests.TestResources
{
    public static class LogAssert
    {
        public static void ExpectAnyContains(LogType logType, string value)
        {
            UnityEngine.TestTools.LogAssert.Expect(logType, new Regex(@$"\b{value}\b"));
        }
        
        public static void ExpectStartingWith(LogType logType, string value)
        {
            UnityEngine.TestTools.LogAssert.Expect(logType, new Regex(@$"^{value}"));
        }
    }
}