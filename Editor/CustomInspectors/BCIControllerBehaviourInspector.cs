using BCIEssentials.ControllerBehaviors;
using UnityEditor;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(BCIControllerBehavior), true)]
    public class BCIControllerBehaviourInspector: CustomInspector
    {
        private bool _showSignalProperties;

        public override void DrawInspector()
        {
            var selfRegisterProp = DrawAndGetProperty("_selfRegister");
            DrawPropertyIf(selfRegisterProp.boolValue, "_selfRegisterAsActive");

            var setupProp = DrawAndGetProperty("_spoSetup");
            DrawPropertyIf(setupProp.objectReferenceValue, "setupRequired");

            DrawProperties(
                "targetFrameRate",
                "_hotkeysEnabled",
                "_selectableSPOs"
            );

            DrawPropertiesInFoldoutGroup(
                ref _showSignalProperties, "Signal properties",
                "windowLength", "interWindowInterval"
            );
        }
    }
}