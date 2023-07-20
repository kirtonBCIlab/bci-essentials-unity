using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Controllers.Editor
{
    [CustomEditor(typeof(BCIController))]
    public class BCIControllerDebugInspector : UnityEditor.Editor
    {
        private BCIBehaviorType _requestedBehavior;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            EditorGUILayout.TextField("Debug Controls", EditorStyles.boldLabel);
            _requestedBehavior =  (BCIBehaviorType)EditorGUILayout.EnumPopup("Behavior Type:", _requestedBehavior);
            if (GUILayout.Button("Request Behavior"))
            {
                BCIController.ChangeBehavior(_requestedBehavior);
            }
        }
    }
}