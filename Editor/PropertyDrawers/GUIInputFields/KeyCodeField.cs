using UnityEngine;
using UnityEditor;

namespace BCIEssentials.Editor
{
    public static partial class GUIInputFields
    {
        const float NormalizedLabelSize = 0.6f;
        const float FieldSpacing = 4f;

        public static KeyCode KeyCodeField(Rect position, string label, KeyCode value)
        {
            (Rect labelPosition, Rect valueFieldPosition)
            = position.SplitHorizontally(NormalizedLabelSize, FieldSpacing);
            label = label.TrimSuffix("Binding");
            EditorGUI.LabelField(labelPosition, label);

            return KeyCodeField(valueFieldPosition, value);
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
                        GUI.changed = true;
                        currentEvent.Use();
                        return currentEvent.keyCode;
                    }
                break;
            }

            return currentValue;
        }

        private static void DrawKeyCodeField
        (
            Rect position, KeyCode displayValue,
            bool isEditing = false
        )
        {
            GUIStyle style = EditorStyles.miniButton;
            string valueString = displayValue.ToString();

            bool isHover = position.Contains(Event.current.mousePosition);
            
            string displayString = isEditing? "Press any Key" : valueString;
            GUIContent content = new(displayString);
            content.tooltip = "Click to Rebind";
            style.Draw(position, content, isHover, isEditing, isEditing, isEditing);
        }

        public static string TrimSuffix(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
                return s[..s.IndexOf(suffix)];
            return s;
        }
    }
}