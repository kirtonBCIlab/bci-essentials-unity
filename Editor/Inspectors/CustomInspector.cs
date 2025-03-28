using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    public abstract class CustomInspector: UnityEditor.Editor
    {
        private List<string> _handledProperties = new();

        public override void OnInspectorGUI()
        {
            _handledProperties.Clear();
            HideProperty("m_Script");

            DrawInspector();

            if (
                serializedObject.GetPropertyNames().Any(
                    name => !_handledProperties.Contains(name)
                )
            )
            {
                DrawHeader("Properties Unhandled By Custom Inspector:", 16, 40);
                DrawPropertiesExcluding(serializedObject, _handledProperties.ToArray());
            }

            serializedObject.ApplyModifiedProperties();
        }

        public abstract void DrawInspector();


        protected T GetTargetAs<T>()
        where T: Component
        => target as T;
        

        protected void DrawHeader
        (
            string headerText,
            int fontSize = 12,
            int topMargin = 20
        )
        {
            GUIStyle headerStyle = new(EditorStyles.boldLabel)
            {
                fontSize = fontSize,
                margin = new(0, 0, topMargin, 0)
            };
            GUILayout.Label(headerText, headerStyle);
        }

        protected void DrawSpace(float pixels)
        {
            if (pixels > 0) GUILayout.Space(pixels);
        }

        protected void DrawNotice
        (
            string label,
            int bottomMargin = 12
        )
        {
            GUIStyle noticeStyle = new(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.UpperCenter,
                margin = new(0, 0, 0, bottomMargin),
                wordWrap = true
            };
            GUILayout.Label(label, noticeStyle);
        }


        protected void DrawProperty(string propertyPath)
        => DrawProperty(GetProperty(propertyPath));
        protected virtual void DrawProperty(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
            _handledProperties.Add(property.propertyPath);
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
            else HideProperty(property);
        }

        protected void HideProperty(string propertyPath)
        => _handledProperties.Add(propertyPath);
        protected void HideProperty(SerializedProperty property)
        => HideProperty(property.propertyPath);


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


        protected bool DrawPropertiesInFoldoutGroup
        (
            bool foldout, string label,
            params string[] paths
        )
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
            DrawPropertiesIf(foldout, paths);
            EditorGUILayout.EndFoldoutHeaderGroup();
            return foldout;
        }
        protected bool DrawPropertiesInFoldoutGroup
        (
            bool foldout, string label,
            IEnumerable <SerializedProperty> properties,
            int fontSize = 14,
            float topMargin = 12, float bottomMargin = 0
        )
        {
            DrawSpace(topMargin);
            GUIStyle style = EditorStyles.foldoutHeader;
            int previousFontSize = style.fontSize;
            style.fontSize = fontSize;

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label, style);
            EditorGUILayout.EndFoldoutHeaderGroup();
            style.fontSize = previousFontSize;

            DrawPropertiesIf(foldout, properties.ToArray());
            DrawSpace(bottomMargin);
            return foldout;
        }
        

        protected bool IsFieldReferenceNull(string propertyPath)
        => GetProperty(propertyPath).objectReferenceValue == null;

        protected SerializedProperty GetProperty(string propertyPath)
        => serializedObject.FindProperty(propertyPath);
    }
}