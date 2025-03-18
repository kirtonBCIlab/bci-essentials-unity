using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BCIEssentials.Editor
{
    public static class SerializedObjectExtensions
    {
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
                action(iterator.Copy());
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
        where T : Attribute
        => target.FindProperty(propertyPath).GetAttribute<T>();
        
        public static bool TryGetAttribute<T>
        (
            this SerializedProperty property,
            out T attribute
        )
        where T : Attribute
        {
            attribute = GetAttribute<T>(property);
            return attribute != null;
        }
        public static bool TryGetAttributes<T>
        (
            this SerializedProperty property,
            out T[] attributes
        )
        where T : Attribute
        {
            attributes = GetAttributes<T>(property);
            return attributes != null;
        }

        public static T GetAttribute<T>
        (
            this SerializedProperty property
        )
        where T : Attribute
        => GetAttributeFromTypeAndParents<T>
        (
            property.name,
            property.serializedObject.targetObject.GetType()
        );
        public static T[] GetAttributes<T>
        (
            this SerializedProperty property
        )
        where T : Attribute
        => GetAttributesFromTypeAndParents<T>
        (
            property.name,
            property.serializedObject.targetObject.GetType()
        );


        const BindingFlags MemberFlags
            = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        private static T GetAttributeFromTypeAndParents<T>
        (
            string name, Type type
        )
        where T : Attribute
        {
            if (type == typeof(MonoBehaviour) || type == typeof(object))
                return null;

            FieldInfo fieldInfo = type.GetField(name, MemberFlags);
            if (fieldInfo != null)
                return fieldInfo.GetCustomAttribute<T>();

            PropertyInfo propertyInfo = type.GetProperty(name, MemberFlags);
            if (propertyInfo != null)
                return propertyInfo.GetCustomAttribute<T>();

            return GetAttributeFromTypeAndParents<T>(name, type.BaseType);
        }

        private static T[] GetAttributesFromTypeAndParents<T>
        (
            string name, Type type
        )
        where T : Attribute
        {
            if (type == typeof(MonoBehaviour) || type == typeof(object))
                return null;

            FieldInfo fieldInfo = type.GetField(name, MemberFlags);
            if (fieldInfo != null)
                return (T[])fieldInfo.GetCustomAttributes<T>();

            PropertyInfo propertyInfo = type.GetProperty(name, MemberFlags);
            if (propertyInfo != null)
                return (T[])propertyInfo.GetCustomAttributes<T>();

            return GetAttributesFromTypeAndParents<T>(name, type.BaseType);
        }
    }
}