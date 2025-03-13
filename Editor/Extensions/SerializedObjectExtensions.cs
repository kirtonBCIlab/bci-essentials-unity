using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BCIEssentials.Editor
{
    public static class SerializedObjectExtensions
    {
        const BindingFlags PropertySearchFlags
            = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        public static void ForEachProperty
        (
            this SerializedObject target,
            Action<SerializedProperty> action
        )
        {
            SerializedProperty iterator = target.GetIterator();
            iterator.NextVisible(true);

            do {
                if (iterator.name == "m_Script") continue;
                action(iterator);
            } while (iterator.NextVisible(false));
        }


        public static IEnumerable<string> GetPropertyNames
        (
            this SerializedObject target
        )
        {
            List<string> propertyNames = new();
            target.ForEachProperty(
                property => propertyNames.Add(property.name)
            );
            return propertyNames;
        }


        public static T GetPropertyAttribute<T>
        (
            this SerializedObject target,
            string propertyPath
        )
        where T : PropertyAttribute
        => target.FindProperty(propertyPath).GetAttribute<T>();
        
        public static T GetAttribute<T>
        (
            this SerializedProperty property
        )
        where T : PropertyAttribute
        {
            Type type = property.serializedObject.targetObject.GetType();

            PropertyInfo propertyInfo = type.GetProperty(property.name, PropertySearchFlags);
            if (propertyInfo != null)
                return propertyInfo.GetCustomAttribute<T>();

            return null;
        }
    }
}