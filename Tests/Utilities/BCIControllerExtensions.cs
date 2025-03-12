using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;

namespace BCIEssentials.Tests.Utilities
{
    public static class BCIControllerExtensions
    {
        public class Properties
        {
            public LSLMarkerStream _lslMarkerStream;
            public LSLResponseStream _lslResponseStream;
            public bool? _dontDestroyActiveInstance;
        }

        public static T AssignInspectorProperties<T>(this T component, Properties properties) where T : BCIController
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