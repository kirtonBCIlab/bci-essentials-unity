using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    public abstract class CustomInspector: UnityEditor.Editor
    {
        private List<string> _drawnProperties = new();

        public override void OnInspectorGUI()
        {
            _drawnProperties.Clear();
            GUI.enabled = false;
            DrawProperty("m_Script");
            GUI.enabled = true;

            DrawInspector();

            if (
                serializedObject.GetPropertyNames().Any(
                    name => !_drawnProperties.Contains(name)
                )
            )
            {
                DrawHeader("Properties Unhandled By Custom Inspector:", 40);
                DrawPropertiesExcluding(serializedObject, _drawnProperties.ToArray());
            }

            serializedObject.ApplyModifiedProperties();
        }

        public abstract void DrawInspector();
        

        protected void DrawHeader
        (
            string headerText,
            int fontSize = 12,
            float topMargin = 20
        )
        {
            DrawSpace(topMargin);
            GUIStyle style = EditorStyles.boldLabel;
            int previousFontSize = style.fontSize;
            style.fontSize = fontSize;
            GUILayout.Label(headerText, style);
            style.fontSize = previousFontSize;
        }

        protected void DrawSpace(float pixels)
        {
            if (pixels > 0) GUILayout.Space(pixels);
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
        

        protected SerializedProperty GetProperty(string propertyPath)
        => serializedObject.FindProperty(propertyPath);
    }
}