using BCIEssentials.ControllerBehaviors;
using UnityEditor;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(BCIControllerBehavior), true)]
    public class BCIControllerBehaviourInspector: CustomInspector
    {
        public override void DrawProperties()
        {
            var selfRegisterProp = GetProperty("_selfRegister");
            DrawProperty(selfRegisterProp);
            DrawPropertyFieldIf("_selfRegisterAsActive", selfRegisterProp.boolValue);
        }
    }
}