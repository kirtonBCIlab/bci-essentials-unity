using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    public abstract class CustomInspector: UnityEditor.Editor
    {
        const BindingFlags FieldSearchFlags
            = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        private List<string> _drawnProperties = new();

        public override void OnInspectorGUI()
        {
            _drawnProperties.Clear();
            GUI.enabled = false;
            DrawProperty("m_Script");
            GUI.enabled = true;

            DrawInspector();

            if (GetTargetFieldNames().Any(name => !_drawnProperties.Contains(name)))
            {
                DrawHeader("Properties Unhandled By Custom Inspector:", 40);
                DrawPropertiesExcluding(serializedObject, _drawnProperties.ToArray());
            }

            serializedObject.ApplyModifiedProperties();
        }

        public abstract void DrawInspector();
        

        protected void DrawHeader(string headerText, float space = 20)
        {
            GUILayout.Space(space);
            GUILayout.Label(headerText, EditorStyles.boldLabel);
        }


        protected void DrawProperty(string propertyPath)
        => DrawProperty(GetProperty(propertyPath));
        protected void DrawProperty(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
            _drawnProperties.Add(property.propertyPath);
        }

        protected SerializedProperty DrawAndGetProperty(string propertyPath)
        {
            SerializedProperty property = GetProperty(propertyPath);
            DrawProperty(property);
            return property;
        }

        protected void DrawPropertyIf(bool condition, string propertyPath)
        => DrawPropertyIf(condition, GetProperty(propertyPath));
        protected void DrawPropertyIf(bool condition, SerializedProperty property)
        {
            if (condition) DrawProperty(property);
            else _drawnProperties.Add(property.propertyPath);
        }


        protected void DrawProperties(params string[] paths)
        {
            foreach (string propertyPath in paths)
                DrawProperty(propertyPath);
        }
        protected void DrawProperties(params SerializedProperty[] properties)
        {
            foreach (SerializedProperty property in properties)
                DrawProperty(property);
        }


        protected void DrawPropertiesIf
        (
            bool condition, params string[] paths
        )
        => Array.ForEach(paths, path => DrawPropertyIf(condition, path));
        protected void DrawPropertiesIf
        (
            bool condition, params SerializedProperty[] properties
        )
        => Array.ForEach(properties, property => DrawPropertyIf(condition, property));


        protected void DrawPropertiesInFoldoutGroup
        (
            ref bool foldOut, string label,
            params string[] paths
        )
        {
            foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(foldOut, label);
            DrawPropertiesIf(foldOut, paths);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        protected void DrawPropertiesInFoldoutGroup
        (
            ref bool foldOut, string label,
            params SerializedProperty[] properties
        )
        {
            foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(foldOut, label);
            DrawPropertiesIf(foldOut, properties);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        

        protected SerializedProperty GetProperty(string propertyPath)
        => serializedObject.FindProperty(propertyPath);


        private IEnumerable<string> GetTargetFieldNames()
        => GetFieldNames(serializedObject.targetObject.GetType());
        private IEnumerable<string> GetFieldNames(Type type)
        {
            FieldInfo[] fields = type.GetFields(FieldSearchFlags);

            return fields.Select(fieldInfo => fieldInfo.Name);
        }


        private bool PropertyHasAttribute<T>
        (SerializedProperty property) where T: Attribute
        => PropertyHasAttribute<T>(property.propertyPath);
        private bool PropertyHasAttribute<T>
        (string path) where T: Attribute
        {
            Type type = serializedObject.targetObject.GetType();

            FieldInfo fieldInfo = type.GetField(path, FieldSearchFlags);
            if (fieldInfo != null)
                return fieldInfo.GetCustomAttribute<T>() != null;

            PropertyInfo propertyInfo = type.GetProperty(path);
            if (propertyInfo != null)
                return propertyInfo.GetCustomAttribute<T>() != null;

            return false;
        }
    }
}