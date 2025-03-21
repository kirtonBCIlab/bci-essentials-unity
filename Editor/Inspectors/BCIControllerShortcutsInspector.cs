using UnityEditor;
using BCIEssentials.Controllers;
using UnityEngine;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(BCIControllerShortcuts))]
    public class BCIControllerShortcutsInspector : ExtendedAttributeInspector
    {
        public override void OnInspectorGUI()
        {
            BCIControllerShortcuts shortcutsComponent
            = target as BCIControllerShortcuts;

            if (shortcutsComponent.Target == null)
            {
                GUIStyle infoLabelStyle = new(EditorStyles.miniBoldLabel)
                {
                    alignment = TextAnchor.UpperCenter,
                    margin = new(0, 0, 0, 12)
                };

                GUILayout.Label(
                    "- targetting static controller methods -",
                    infoLabelStyle
                );
            }

            base.OnInspectorGUI();
        }
    }
}