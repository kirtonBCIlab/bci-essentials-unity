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
        private List<string> _drawnProperties = new();

        public override void OnInspectorGUI()
        {
            _drawnProperties.Clear();
            GUI.enabled = false;
            DrawProperty("m_Script");
            GUI.enabled = true;

            DrawProperties();

            if (GetTargetFieldNames().Any(name => !_drawnProperties.Contains(name)))
            {
                DrawHeader("Properties Unhandled By Custom Inspector:", 40);
                DrawPropertiesExcluding(serializedObject, _drawnProperties.ToArray());
            }

            serializedObject.ApplyModifiedProperties();
        }

        public abstract void DrawProperties();

        protected void DrawProperty(string propertyPath)
        => DrawProperty(GetProperty(propertyPath));
        protected void DrawProperty(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
            _drawnProperties.Add(property.name);
        }

        protected void DrawPropertyFieldIf(string propertyPath, bool condition)
        {
            if (condition) DrawProperty(propertyPath);
            else _drawnProperties.Add(propertyPath);
        }

        protected SerializedProperty GetProperty(string propertyPath)
        => serializedObject.FindProperty(propertyPath);

        protected void DrawHeader(string headerText, float space = 20)
        {
            GUILayout.Space(space);
            GUILayout.Label(headerText, EditorStyles.boldLabel);
        }

        protected void DrawFoldoutGroup(ref bool toggle, string label, Action content)
        {
            toggle = EditorGUILayout.BeginFoldoutHeaderGroup(toggle, label);
            content();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected void DrawToggleGroup(ref bool toggle, string label, Action content)
        {
            toggle = EditorGUILayout.BeginToggleGroup(label, toggle);
            content();
            EditorGUILayout.EndToggleGroup();
        }


        private IEnumerable<string> GetTargetFieldNames()
        => GetFieldNames(serializedObject.targetObject.GetType());
        private IEnumerable<string> GetFieldNames(Type type)
        {
            BindingFlags fieldSearchFlags = BindingFlags.Instance;
            fieldSearchFlags |= BindingFlags.Public;
            fieldSearchFlags |= BindingFlags.NonPublic;
            FieldInfo[] fields = type.GetFields(fieldSearchFlags);

            return fields.Select(fieldInfo => fieldInfo.Name);
        }
    }
}