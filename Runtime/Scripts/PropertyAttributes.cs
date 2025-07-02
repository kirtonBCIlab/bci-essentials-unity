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

    public class StartFoldoutGroupAttribute : PropertyAttribute
    {
        public string Label;
        public int FontSize;
        public float TopMargin, BottomMargin;

        public StartFoldoutGroupAttribute
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

        public bool IsSiblingTo(StartFoldoutGroupAttribute a)
        => a != null && Label == a.Label;
    }

    public class EndFoldoutGroupAttribute: PropertyAttribute {}

    public class AppendToFoldoutGroupAttribute: PropertyAttribute
    {
        public string GroupLabel;

        public AppendToFoldoutGroupAttribute(string groupLabel)
        {
            GroupLabel = groupLabel;
        }
    }

    /// <summary>
    /// Triggers conditional inspector rendering on a serialized script field
    /// if used from a child of <see cref="MonoBehaviourUsingExtendedAttributes"/>
    /// <br/>
    /// Multiple instances can be used to require multiple conditions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute : Attribute
    {
        public string ConditionPropertyPath;
        public int[] ValidValues;

        public ShowIfAttribute(string conditionPropertyPath)
        {
            ConditionPropertyPath = conditionPropertyPath;
        }
        /// <param name="validValues">show field if the condition property matches any of the specified values</param>
        public ShowIfAttribute
        (
            string conditionPropertyPath,
            params int[] validValues
        ) : this(conditionPropertyPath)
        {
            ValidValues = validValues;
        }

# if UNITY_EDITOR
        public bool ShouldShow(SerializedObject target)
        {
            SerializedProperty conditionProperty
            = target.FindProperty(ConditionPropertyPath);

            if (conditionProperty == null)
            {
                Debug.LogWarning($"Property not found for {nameof(ShowIfAttribute)}");
                return false;
            }

            return conditionProperty.propertyType switch
            {
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
# endif
    }


    public class DisplayNameAttribute : PropertyAttribute
    {
        public string Label;

        public DisplayNameAttribute(string label)
        {
            Label = label;
        }
    }
}