using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    [CustomPropertyDrawer(typeof(KeyCode))]
    public class KeyCodeDrawer: PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect labelPosition = position;
            labelPosition.xMax -= position.width / 2;
            EditorGUI.LabelField(labelPosition, label);
            
            position.xMin += position.width / 2;
            property.enumValueIndex = (int)KeyCodeField(position, (KeyCode)property.enumValueIndex);
        }


        public static KeyCode KeyCodeField(Rect position, KeyCode currentValue)
        {
            int keyboardControlId = GUIUtility.GetControlID(FocusType.Keyboard);
            Event currentEvent = Event.current;

            switch(currentEvent.GetTypeForControl(keyboardControlId))
            {
                case EventType.Repaint:
                    GUIStyle style = EditorStyles.miniButton;// GUI.skin.GetStyle("TextField");
                    string s = currentValue.ToString();
                    if (GUIUtility.keyboardControl == keyboardControlId)
                        s = "Press a Key to Bind";
                    style.Draw(position, new GUIContent(s), keyboardControlId);
                break;
                case EventType.MouseDown:
                    if (
                        position.Contains(currentEvent.mousePosition) &&
                        currentEvent.button == 0 && GUIUtility.hotControl == 0
                    ) {
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
    }
}