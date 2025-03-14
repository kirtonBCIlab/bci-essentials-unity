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
            FoldoutGroupAttribute currentFoldoutGroupAttribute = null;
            List<SerializedProperty> foldoutProperties = new();

            serializedObject.ForEachProperty(property => {
                if (property.TryGetAttribute<FoldoutGroupAttribute>(out var foldoutAttribute))
                {
                    if (foldoutAttribute.IsSiblingTo(currentFoldoutGroupAttribute))
                    {
                        foldoutProperties.Add(property);
                    }
                    else
                    {
                        DrawFoldoutGroupFromAttribute
                        (
                            currentFoldoutGroupAttribute,
                            foldoutProperties
                        );

                        currentFoldoutGroupAttribute = foldoutAttribute;
                        foldoutProperties = new() {property};
                    }
                }
                else {
                    DrawFoldoutGroupFromAttribute
                    (
                        currentFoldoutGroupAttribute,
                        foldoutProperties
                    );

                    currentFoldoutGroupAttribute = null;
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

        
        private void DrawFoldoutGroupFromAttribute
        (
            FoldoutGroupAttribute attribute,
            IEnumerable<SerializedProperty> properties
        )
        {
            if (attribute == null) return;
            string label = attribute.Label;

            if (!foldoutGroupToggles.ContainsKey(label))
                foldoutGroupToggles.Add(label, false);

            foldoutGroupToggles[label]
            = DrawPropertiesInFoldoutGroup(
                foldoutGroupToggles[label],
                label, properties, attribute.FontSize,
                attribute.TopMargin, attribute.BottomMargin
            );
        }
    }
}