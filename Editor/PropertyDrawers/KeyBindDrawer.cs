using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    using UnityEngine.InputSystem;
    using Utilities;

    [CustomPropertyDrawer(typeof(KeyBind))]
    public class KeyBindDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty boundKeyProperty
            = property.FindPropertyRelative(nameof(KeyBind.BoundKey));

            Key value = (Key)boundKeyProperty.enumValueFlag;

            string labelText = label.text;
            if (property.TryGetAttribute<InspectorNameAttribute>(out var nameAttribute))
            {
                labelText = nameAttribute.displayName;
            }
            
            EditorGUI.BeginChangeCheck();
            value = GUIInputFields.KeyCodeField(position, labelText, value);
            if (EditorGUI.EndChangeCheck())
            {
                boundKeyProperty.enumValueFlag = (int)value;
            }
        }
    }
}