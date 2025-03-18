using UnityEditor;
using System.Collections.Generic;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(MonoBehaviourUsingExtendedAttributes), true)]
    public class ExtendedAttributeInspector: CustomInspector
    {
        Dictionary<string, bool> foldoutGroupToggles = new();

        bool IsTraversingFoldoutGroup
        => currentFoldoutGroupAttribute != null;
        FoldoutGroupAttribute currentFoldoutGroupAttribute;
        List<SerializedProperty> currentFoldoutGroupProperties;


        public override void DrawInspector()
        {
            currentFoldoutGroupAttribute = null;
            currentFoldoutGroupProperties = new();

            serializedObject.ForEachProperty(property => {
                if (property.TryGetAttribute<FoldoutGroupAttribute>(out var foldoutAttribute))
                {
                    DrawCurrentFoldoutGroupIfItExists();
                    currentFoldoutGroupAttribute = foldoutAttribute;
                    currentFoldoutGroupProperties = new() {property};
                }
                else if (IsTraversingFoldoutGroup)
                {
                    currentFoldoutGroupProperties.Add(property);
                    if (property.TryGetAttribute<EndFoldoutGroupAttribute>(out _))
                    {
                        DrawCurrentFoldoutGroupIfItExists();
                        ClearCurrentFoldoutGroup();
                    }
                }
                else
                {
                    DrawProperty(property);
                }
            });

            DrawCurrentFoldoutGroupIfItExists();
            ClearCurrentFoldoutGroup();
        }


        protected override void DrawProperty(SerializedProperty property)
        {
            bool shouldHideProperty = false;
            if (property.TryGetAttributes<ShowIfAttribute>(out var showIfAttributes))
            {
                foreach(ShowIfAttribute attribute in showIfAttributes)
                    shouldHideProperty |= !attribute.ShouldShow(serializedObject);
            }
            
            if (property.TryGetAttribute<ShowWithFoldoutGroupAttribute>(out var showWithGroupAttribute))
            {
                string groupLabel = showWithGroupAttribute.GroupLabel;
                if (foldoutGroupToggles.ContainsKey(groupLabel))
                    shouldHideProperty |= !foldoutGroupToggles[groupLabel];
            }

            if (shouldHideProperty)
                HideProperty(property);
            else
                base.DrawProperty(property);
        }


        private void DrawCurrentFoldoutGroupIfItExists()
        {
            if (!IsTraversingFoldoutGroup) return;
            string label = currentFoldoutGroupAttribute.Label;

            if (!foldoutGroupToggles.ContainsKey(label))
                foldoutGroupToggles.Add(label, false);

            foldoutGroupToggles[label]
            = DrawPropertiesInFoldoutGroup(
                foldoutGroupToggles[label],
                label, currentFoldoutGroupProperties,
                currentFoldoutGroupAttribute.FontSize,
                currentFoldoutGroupAttribute.TopMargin,
                currentFoldoutGroupAttribute.BottomMargin
            );
        }

        private void ClearCurrentFoldoutGroup()
        {
            currentFoldoutGroupAttribute = null;
            currentFoldoutGroupProperties.Clear();
        }
    }
}