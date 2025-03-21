using UnityEditor;
using UnityEngine;
using BCIEssentials.Controllers;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(BCIControllerInstance))]
    public class BCIControllerInstanceInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BCIControllerInstance controllerInstance
            = target as BCIControllerInstance;

            BCIBehaviorType behaviorType = BCIBehaviorType.Unset;
            if (controllerInstance.ActiveBehavior != null)
            {
                behaviorType = controllerInstance.ActiveBehavior.BehaviorType;
            }
            
            if (EditorApplication.isPlaying)
            {
                GUILayout.Space(16);
                EditorGUILayout.TextField("Debug Controls", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                behaviorType = (BCIBehaviorType)EditorGUILayout
                    .EnumPopup("Behavior Type:", behaviorType);
                if (EditorGUI.EndChangeCheck())
                {
                    controllerInstance.ChangeBehavior(behaviorType);
                }
            }
        }
    }
}