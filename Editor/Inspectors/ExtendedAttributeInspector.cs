using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Editor
{
    [CustomEditor(typeof(MonoBehaviourUsingExtendedAttributes), true)]
    public class ExtendedAttributeInspector: CustomInspector
    {
        public override void DrawInspector()
        {
            List<ParsedField> parsedFields = new();
            FoldoutGroup currentFoldoutGroup = null;

            serializedObject.ForEachProperty(property => {
                if (property.TryGetAttribute<FoldoutGroupAttribute>(out var foldoutAttribute))
                {
                    currentFoldoutGroup = new(foldoutAttribute, property);
                    parsedFields.Add(currentFoldoutGroup);
                }
                else if (currentFoldoutGroup != null)
                {
                    currentFoldoutGroup.AddProperty(property);
                    if (property.TryGetAttribute<EndFoldoutGroupAttribute>(out _))
                    {
                        currentFoldoutGroup = null;
                    }
                }
                else
                {
                    if (TryFindMatchingGroup(property, parsedFields, out FoldoutGroup group))
                    {
                        group.AddProperty(property);
                    }
                    else parsedFields.Add(new SingleProperty(property));
                }
            });

            foreach (ParsedField field in parsedFields)
            switch (field)
            {
                case SingleProperty single:
                    DrawProperty(single.Property);
                break;
                case FoldoutGroup group:
                    DrawFoldoutGroup(group);
                break;
            }
        }


        protected override void DrawProperty(SerializedProperty property)
        {
            bool shouldHideProperty = false;
            if (property.TryGetAttributes<ShowIfAttribute>(out var showIfAttributes))
            {
                foreach(ShowIfAttribute attribute in showIfAttributes)
                    shouldHideProperty |= !attribute.ShouldShow(serializedObject);
            }

            if (shouldHideProperty)
                HideProperty(property);
            else
                base.DrawProperty(property);
        }


        private void DrawFoldoutGroup(FoldoutGroup group)
        {
            string label = group.Label;

            string prefsKey = $"{target.name}/{label}";
            bool savedFoldoutState = EditorPrefs.GetBool(prefsKey, false);

            (int fontSize, float topMargin, float bottomMargin)
            = group.GetStyleAttributes();

            bool foldoutState = DrawPropertiesInFoldoutGroup(
                savedFoldoutState, label, group.Properties,
                fontSize, topMargin, bottomMargin
            );

            if (foldoutState != savedFoldoutState)
            {
                EditorPrefs.SetBool(prefsKey, foldoutState);
            }
        }


        private bool TryFindMatchingGroup
        (
            SerializedProperty property,
            List<ParsedField> fieldList,
            out FoldoutGroup group
        )
        {
            if (property.TryGetAttribute<ShowWithFoldoutGroupAttribute>(out var showWithGroupAttribute))
            {
                string groupLabel = showWithGroupAttribute.GroupLabel;
                group = (FoldoutGroup)fieldList.Find
                (
                    ParsedField => ParsedField is FoldoutGroup group
                    && group.Label == groupLabel
                );
                if (group != null) return true;
                else
                Debug.LogWarning(
                    $"Failed to find matching foldout group {groupLabel}"
                    + $" for property {property.name}"
                );
            }
            group = null;
            return false;
        }


        public abstract class ParsedField {}

        public class SingleProperty: ParsedField
        {
            public SerializedProperty Property;

            public SingleProperty
            (
                SerializedProperty property
            ) { Property = property; }
        }

        public class FoldoutGroup: ParsedField
        {
            public FoldoutGroupAttribute Attribute;
            public string Label => Attribute.Label;

            public List<SerializedProperty> Properties;

            public FoldoutGroup
            (
                FoldoutGroupAttribute attribute,
                SerializedProperty leadingProperty
            )
            {
                Attribute = attribute;
                Properties = new() {leadingProperty};
            }

            public void AddProperty(SerializedProperty property)
            => Properties.Add(property);

            public (int, float, float) GetStyleAttributes()
            => (
                Attribute.FontSize,
                Attribute.TopMargin,
                Attribute.BottomMargin
            );
        }
    }
}