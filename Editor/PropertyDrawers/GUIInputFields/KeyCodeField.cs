using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace BCIEssentials.Editor
{
    public static partial class GUIInputFields
    {
        const float NormalizedLabelSize = 0.6f;
        const float FieldSpacing = 4f;

        public static Key KeyCodeField(Rect position, string label, Key value)
        {
            (Rect labelPosition, Rect valueFieldPosition)
            = position.SplitHorizontally(NormalizedLabelSize, FieldSpacing);
            label = label.TrimSuffix("Binding");
            EditorGUI.LabelField(labelPosition, label);

            return KeyCodeField(valueFieldPosition, value);
        }

        public static Key KeyCodeField(Rect position, Key currentValue)
        {
            int keyboardControlId = GUIUtility.GetControlID(FocusType.Keyboard);
            Event currentEvent = Event.current;

            switch (currentEvent.GetTypeForControl(keyboardControlId))
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
                        return KeyCodeToInputSystemKey(currentEvent.keyCode);
                    }
                    break;
            }

            return currentValue;
        }

        private static void DrawKeyCodeField
        (
            Rect position, Key displayValue,
            bool isEditing = false
        )
        {
            GUIStyle style = EditorStyles.miniButton;
            string valueString = displayValue.ToString();

            bool isHover = position.Contains(Event.current.mousePosition);

            string displayString = isEditing ? "Press any Key" : valueString;
            GUIContent content = new(displayString, "Click to Rebind");
            style.Draw(position, content, isHover, isEditing, isEditing, isEditing);
        }

        public static string TrimSuffix(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
                return s[..s.IndexOf(suffix)];
            return s;
        }


        public static Key KeyCodeToInputSystemKey(KeyCode keyCode)
        => keyCode switch
        {
            KeyCode.None => Key.None,
            KeyCode.Keypad0 => Key.Numpad0,
            KeyCode.Keypad1 => Key.Numpad1,
            KeyCode.Keypad2 => Key.Numpad2,
            KeyCode.Keypad3 => Key.Numpad3,
            KeyCode.Keypad4 => Key.Numpad4,
            KeyCode.Keypad5 => Key.Numpad5,
            KeyCode.Keypad6 => Key.Numpad6,
            KeyCode.Keypad7 => Key.Numpad7,
            KeyCode.Keypad8 => Key.Numpad8,
            KeyCode.Keypad9 => Key.Numpad9,
            KeyCode.KeypadPeriod => Key.NumpadPeriod,
            KeyCode.KeypadDivide => Key.NumpadDivide,
            KeyCode.KeypadMultiply => Key.NumpadMultiply,
            KeyCode.KeypadMinus => Key.NumpadMinus,
            KeyCode.KeypadPlus => Key.NumpadPlus,
            KeyCode.KeypadEnter => Key.NumpadEnter,
            KeyCode.KeypadEquals => Key.NumpadEquals,
            KeyCode.Alpha0 => Key.Digit0,
            KeyCode.Alpha1 => Key.Digit1,
            KeyCode.Alpha2 => Key.Digit2,
            KeyCode.Alpha3 => Key.Digit3,
            KeyCode.Alpha4 => Key.Digit4,
            KeyCode.Alpha5 => Key.Digit5,
            KeyCode.Alpha6 => Key.Digit6,
            KeyCode.Alpha7 => Key.Digit7,
            KeyCode.Alpha8 => Key.Digit8,
            KeyCode.Alpha9 => Key.Digit9,
            _ => GetKeyByLabel(keyCode.ToString())
        };

        public static Key GetKeyByLabel(string label)
        {
            foreach (Key key in System.Enum.GetValues(typeof(Key)))
            {
                if (key.ToString() == label) return key;
            }
            Debug.LogWarning("Unable to resolve key press");
            return Key.None;
        }
    }
}