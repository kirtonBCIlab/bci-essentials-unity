using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    using Controllers;

    [CustomPropertyDrawer(typeof(KeyBind))]
    public class KeyBindDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty boundKeyProperty
            = property.FindPropertyRelative(nameof(KeyBind.BoundKey));

            KeyCode value = (KeyCode)boundKeyProperty.enumValueFlag;
            
            EditorGUI.BeginChangeCheck();
            value = GUIInputFields.KeyCodeField(position, label.text, value);
            if (EditorGUI.EndChangeCheck())
            {
                boundKeyProperty.enumValueFlag = (int)value;
            }
        }
    }
}