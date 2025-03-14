using UnityEditor;
using UnityEngine;

namespace BCIEssentials
{
    /// <summary>
    /// Base class for MonoBehaviours using
    /// [FoldoutGroup] and [ShowIf] attributes
    /// </summary>
    public abstract class MonoBehaviourUsingExtendedAttributes: MonoBehaviour {}

    public class InspectorReadOnlyAttribute : PropertyAttribute { }

    public class FoldoutGroupAttribute : PropertyAttribute
    {
        public string Label;
        public int FontSize;
        public float TopMargin, BottomMargin;

        public FoldoutGroupAttribute
        (
            string label, int fontSize = 14,
            float topMargin = 12, float bottomMargin = 0
        )
        {
            Label = label;
            FontSize = fontSize;
            TopMargin = topMargin;
            BottomMargin = bottomMargin;
        }

        public bool IsSiblingTo(FoldoutGroupAttribute a)
        => a != null && Label == a.Label;
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