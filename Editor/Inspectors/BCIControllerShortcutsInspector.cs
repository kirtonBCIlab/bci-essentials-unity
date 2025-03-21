using UnityEditor;
using BCIEssentials.Controllers;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(BCIControllerShortcuts))]
    public class BCIControllerShortcutsInspector : ExtendedAttributeInspector
    {
        public override void DrawInspector()
        {
            if (GetTargetAs<BCIControllerShortcuts>().Target == null)
                DrawNotice("targetting static controller methods");

            base.DrawInspector();
        }
    }
}