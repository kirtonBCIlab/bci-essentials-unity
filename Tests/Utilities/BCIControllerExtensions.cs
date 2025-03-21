using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;

namespace BCIEssentials.Tests.Utilities
{
    public static class BCIControllerExtensions
    {
        public class Properties
        {
            public LSLMarkerWriter _markerWriter;
            public LSLResponseProvider _responseProvider;
            public bool? _persistBetweenScenes;
        }

        public static BCIControllerInstance AssignInspectorProperties(this BCIControllerInstance controllerInstance, Properties properties)
        {
            if (properties == null) {
                return controllerInstance;
            }

            if (properties._markerWriter != null) {
                ReflectionHelpers.SetField(
                    controllerInstance,
                    nameof(properties._markerWriter),
                    properties._markerWriter
                );
            }
            
            if (properties._responseProvider != null) {
                ReflectionHelpers.SetField(
                    controllerInstance,
                    nameof(properties._responseProvider),
                    properties._responseProvider
                );
            }
            
            if (properties._persistBetweenScenes != null) {
                ReflectionHelpers.SetField(
                    controllerInstance,
                    nameof(properties._persistBetweenScenes),
                    properties._persistBetweenScenes
                );
            }
            
            return controllerInstance;
        }
    }
}