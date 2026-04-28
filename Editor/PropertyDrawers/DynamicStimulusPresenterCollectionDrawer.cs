using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    using Stimulus.Collections;

    [CustomPropertyDrawer(typeof(DynamicStimulusPresenterCollection))]
    public class DynamicStimulusPresenterCollectionDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (GUILayout.Button("Locate Presenters"))
            {
                (property.boxedValue as DynamicStimulusPresenterCollection).RepopulateSerialized(property);
                property.isExpanded = true;
            }

            EditorGUILayout.PropertyField(property);
        }
    }
}