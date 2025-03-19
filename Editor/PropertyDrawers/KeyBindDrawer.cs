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
            Rect labelPosition = position;
            labelPosition.xMax -= position.width / 2;

            label.text = label.text.TrimSuffix("Binding");
            EditorGUI.LabelField(labelPosition, label);

            SerializedProperty boundKeyProperty = property.FindPropertyRelative(nameof(KeyBind.BoundKey));
            Rect keyCodeFieldPosition = position;
            keyCodeFieldPosition.xMin += position.width / 2;

            KeyCode value = (KeyCode)boundKeyProperty.enumValueFlag;
            value = GUIInputFields.KeyCodeField(keyCodeFieldPosition, value);
            boundKeyProperty.enumValueFlag = (int)value;
        }
    }
}