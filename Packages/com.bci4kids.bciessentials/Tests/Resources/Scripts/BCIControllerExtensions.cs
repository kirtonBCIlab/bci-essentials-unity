using BCIEssentials.LSL;
using UnityEngine;

namespace BCIEssentials.Tests.TestResources
{
    public static class BCIControllerExtensions
    {
        public class Properties
        {
            public LSLMarkerStream _lslMarkerStream;
            public LSLResponseStream _lslResponseStream;
            public bool? _dontDestroyActiveInstance;
        }

        public static T SetInspectorProperties<T>(this T component, Properties properties) where T : Component
        {
            if (properties == null)
            {
                return component;
            }
            
            if (properties._lslMarkerStream != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties._lslMarkerStream), properties._lslMarkerStream);
            }
            
            if (properties._lslResponseStream != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties._lslResponseStream), properties._lslResponseStream);
            }
            
            if (properties._dontDestroyActiveInstance != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties._dontDestroyActiveInstance), properties._dontDestroyActiveInstance);
            }

            return component;
        }
    }
}