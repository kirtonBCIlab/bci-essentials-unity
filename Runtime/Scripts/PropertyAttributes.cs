using System;
using System.Linq;
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

    public class EndFoldoutGroupAttribute: PropertyAttribute {}

    public class ShowWithFoldoutGroupAttribute: PropertyAttribute
    {
        public string GroupLabel;

        public ShowWithFoldoutGroupAttribute(string groupLabel)
        {
            GroupLabel = groupLabel;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ShowIfAttribute : Attribute
    {
        public string ConditionPropertyPath;
        public int[] ValidValues;

        public ShowIfAttribute(string conditionPropertyPath)
        {
            ConditionPropertyPath = conditionPropertyPath;
        }
        public ShowIfAttribute
        (
            string conditionPropertyPath,
            params int[] validValues
        ): this(conditionPropertyPath)
        {
            ValidValues = validValues;
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
                SerializedPropertyType.Integer
                | SerializedPropertyType.Enum
                    => ValidValues.Any(v => v == conditionProperty.intValue)
                ,
                SerializedPropertyType.ObjectReference
                    => conditionProperty.objectReferenceValue != null
                ,
                    _ => true
            };
        }
    }
}