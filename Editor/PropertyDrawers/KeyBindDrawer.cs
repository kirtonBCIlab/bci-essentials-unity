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

            label.text = TrimSuffix(label.text, "Binding");
            EditorGUI.LabelField(labelPosition, label);

            SerializedProperty boundKeyProperty = property.FindPropertyRelative(nameof(KeyBind.BoundKey));
            Rect keyBindFieldPosition = position;
            keyBindFieldPosition.xMin += position.width / 2;

            KeyCode value = (KeyCode)boundKeyProperty.enumValueFlag;
            value = KeyCodeField(keyBindFieldPosition, value);
            boundKeyProperty.enumValueFlag = (int)value;
        }


        public static KeyCode KeyCodeField(Rect position, KeyCode currentValue)
        {
            int keyboardControlId = GUIUtility.GetControlID(FocusType.Keyboard);
            Event currentEvent = Event.current;

            switch(currentEvent.GetTypeForControl(keyboardControlId))
            {
                case EventType.Repaint:
                    DrawKeyCodeField(
                        position, currentValue,
                        GUIUtility.keyboardControl == keyboardControlId
                    );
                break;
                case EventType.MouseDown:
                    if (
                        position.Contains(currentEvent.mousePosition) &&
                        currentEvent.button == 0 && GUIUtility.hotControl == 0
                    )
                    {
                        GUIUtility.hotControl = keyboardControlId;
                        GUIUtility.keyboardControl = keyboardControlId;
                        currentEvent.Use();
                    }
                break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == keyboardControlId)
                    {
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == keyboardControlId)
                    {
                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        currentEvent.Use();
                        return currentEvent.keyCode;
                    }
                break;
            }

            return currentValue;
        }

        public static void DrawKeyCodeField
        (
            Rect position, KeyCode displayValue,
            bool isEditing = false
        )
        {
            GUIStyle style = EditorStyles.miniButton;
            string valueString = displayValue.ToString();

            bool isHover = position.Contains(Event.current.mousePosition);
            
            string displayString = isEditing? "Press a Key to Bind" : valueString;
            style.Draw(position, displayString, isHover, isEditing, isEditing, isEditing);
        }

        private static string TrimSuffix(string s, string suffix)
        {
            if (s.EndsWith(suffix))
                return s[..s.IndexOf(suffix)];
            return s;
        }
    }
}