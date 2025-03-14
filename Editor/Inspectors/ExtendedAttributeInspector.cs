using UnityEditor;
using System.Collections.Generic;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(MonoBehaviourUsingExtendedAttributes), true)]
    public class ExtendedAttributeInspector: CustomInspector
    {
        Dictionary<string, bool> foldoutGroupToggles = new();

        public override void DrawInspector()
        {
            string foldoutLabel = null;
            List<SerializedProperty> foldoutProperties = new();

            serializedObject.ForEachProperty(property => {
                if (property.TryGetAttribute<FoldoutGroupAttribute>(out var foldoutAttribute))
                {
                    if (foldoutAttribute.Label == foldoutLabel)
                    {
                        foldoutProperties.Add(property);
                    }
                    else
                    {
                        DrawFoldoutGroupIfHasLabel(foldoutLabel, foldoutProperties);

                        foldoutLabel = foldoutAttribute.Label;
                        foldoutProperties = new() {property};
                    }
                }
                else {
                    DrawFoldoutGroupIfHasLabel(foldoutLabel, foldoutProperties);

                    foldoutLabel = null;
                    foldoutProperties.Clear();

                    // TODO: refactor to support showif attributes inside a foldout group
                    if (property.TryGetAttribute<ShowIfAttribute>(out var showIfAttribute))
                    {
                        DrawPropertyIf(
                            showIfAttribute.ShouldShow(serializedObject),
                            property
                        );
                    }
                    else
                    {
                        DrawProperty(property);
                    }
                }
            });
        }

        
        private void DrawFoldoutGroupIfHasLabel
        (
            string label,
            IEnumerable<SerializedProperty> properties
        )
        {
            if (label == null) return;

            if (!foldoutGroupToggles.ContainsKey(label))
                foldoutGroupToggles.Add(label, false);

            foldoutGroupToggles[label]
            = DrawPropertiesInFoldoutGroup(
                foldoutGroupToggles[label],
                label, properties
            );
        }
    }
}