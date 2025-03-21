using UnityEditor;
using UnityEngine;
using BCIEssentials.Controllers;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(BCIControllerInstance))]
    public class BCIControllerInstanceInspector : ExtendedAttributeInspector
    {
        public override void DrawInspector()
        {
            var controllerInstance = GetTargetAs<BCIControllerInstance>();

            if (
                IsFieldReferenceNull("_markerWriter") ||
                IsFieldReferenceNull("_responseProvider")
            ) DrawNotice("missing stream component(s) will be created on demand");

            base.DrawInspector();

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