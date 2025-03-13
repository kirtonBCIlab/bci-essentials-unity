using UnityEditor;
using UnityEngine;

namespace BCIEssentials
{
    public class InspectorReadOnlyAttribute : PropertyAttribute { }

    public class FoldoutGroupAttribute : PropertyAttribute
    {
        public string Label;

        public FoldoutGroupAttribute(string label)
        {
            Label = label;
        }
    }

    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionPropertyPath;

        public ShowIfAttribute(string conditionPropertyPath)
        {
            ConditionPropertyPath = conditionPropertyPath;
        }

        public bool ShouldShow(SerializedObject target)
        {
            SerializedProperty conditionProperty
            = target.FindProperty(ConditionPropertyPath);
            
            if (conditionProperty == null)
            {
                Debug.LogWarning($"Property not found for {nameof(ShowIfAttribute)}");
                return false;
            }

            return conditionProperty.propertyType switch {
                SerializedPropertyType.Boolean
                    => conditionProperty.boolValue
                ,
                SerializedPropertyType.ObjectReference
                    => conditionProperty.objectReferenceValue != null
                ,
                    _ => true
            };
        }
    }
}