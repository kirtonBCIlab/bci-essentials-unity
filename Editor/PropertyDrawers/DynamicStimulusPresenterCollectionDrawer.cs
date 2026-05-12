using UnityEditor;
using UnityEngine;

namespace BCIEssentials.Editor
{
    using Stimulus;

    [CustomPropertyDrawer(typeof(DynamicStimulusPresenterCollection))]
    public class DynamicStimulusPresenterCollectionDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DynamicStimulusPresenterCollection target = GetTarget(property);

            if (GUILayout.Button("Locate Presenters"))
            {
                target.RepopulateSerialized(property);
                property.isExpanded = true;
            }

            EditorGUILayout.PropertyField(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;

        DynamicStimulusPresenterCollection GetTarget(SerializedProperty property)
        => property.boxedValue as DynamicStimulusPresenterCollection;
    }
}