using System;
using System.Reflection;
using UnityEngine;

namespace BCIEssentials.Tests.TestResources
{
    public static class ReflectionHelpers
    {
        public static void SetField<T>(T component, string name, object value, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            var componentType = component.GetType();
            var info = GetFieldRecursive(componentType, name, bindingFlags);
            if (info == null)
            {
                return;
            }

            info.SetValue(component, value);
        }

        public static FieldInfo GetFieldRecursive(Type type, string fieldName, BindingFlags bindingFlags)
        {
            var infoRecursive = type.GetField(fieldName, bindingFlags);
            if (infoRecursive != null)
            {
                return infoRecursive;
            }
                
            var baseType = type.BaseType;
            if (baseType == null || baseType == typeof(MonoBehaviour))
            {
                return null;
            }
                
            infoRecursive = GetFieldRecursive(baseType, fieldName, bindingFlags);

            return infoRecursive;
        }
    }
}