using System.Collections.Generic;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Tests.Utilities
{
    public static class BCIControllerBehaviorExtensions
    {
        public class Properties
        {
            public bool? _selfRegister;
            public bool? _selfRegisterAsActive;
            public int? targetFrameRate;
            public List<SPO> _selectableSPOs = null;
            
            //Training
            public SPOGridFactory setup;
            public bool? setupRequired;
        }

        public static T AssignInspectorProperties<T>(this T component, Properties properties) where T : BCIControllerBehavior
        {
            if (properties == null)
            {
                return component;
            }
            
            if (properties._selfRegister != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties._selfRegister), properties._selfRegister);
            }
            
            if (properties._selfRegisterAsActive != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties._selfRegisterAsActive), properties._selfRegisterAsActive);
            }
            
            if (properties.targetFrameRate != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties.targetFrameRate), properties.targetFrameRate);
            }
            
            if (properties._selectableSPOs != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties._selectableSPOs), properties._selectableSPOs);
            }
            
            if (properties.setup != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties.setup), properties.setup);
            }
            
            if (properties.setupRequired != null)
            {
                ReflectionHelpers.SetField(component, nameof(properties.setupRequired), properties.setupRequired);
            }

            return component;
        }
    }
}