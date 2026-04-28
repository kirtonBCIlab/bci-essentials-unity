using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property)) return;
            EditorGUI.PropertyField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldShow(property)
                ? base.GetPropertyHeight(property, label)
                : 0;
        }

        private bool ShouldShow(SerializedProperty property)
        => attribute is ShowIfAttribute showIfAttribute &&
            showIfAttribute.ShouldShow(property);
    }
}