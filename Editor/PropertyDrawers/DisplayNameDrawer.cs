using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    [CustomPropertyDrawer(typeof(DisplayNameAttribute))]
    public class DisplayNamePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = (attribute as DisplayNameAttribute).Label;
            EditorGUI.PropertyField(position, property, label);
        }
    }
}