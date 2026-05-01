using System.Collections.Generic;

namespace BCIEssentials.Stimulus.Collections
{
    using Presentation;
    using UnityEditor;
    using UnityEngine;

    public enum SearchMethod { Type, Tag }
    public enum Scope { Scene, Children, ChildrenOfParent }

    [System.Serializable]
    public class DynamicStimulusPresenterCollection : StimulusPresenterCollection
    {
        [Header("Presenter Discovery")]
        public SearchMethod PopulationMethod;
        public Scope PopulationScope;

        [ShowIf(nameof(PopulationMethod), (int)SearchMethod.Tag)]
        public string PresenterTag = "BCI";
        public bool RepopulateWhenQueried;

        private readonly MonoBehaviour _defaultSearchAnchor;

        public DynamicStimulusPresenterCollection(MonoBehaviour defaultSearchAnchor)
        => _defaultSearchAnchor = defaultSearchAnchor;


        public void Repopulate() => Repopulate(_defaultSearchAnchor);
        public void Repopulate(MonoBehaviour searchAnchor)
        {
            _stimulusPresenters = PopulationMethod switch
            {
                SearchMethod.Type => searchAnchor.GetSelectablePresentersByType(PopulationScope),
                SearchMethod.Tag => searchAnchor.GetSelectablePresentersByTag(PresenterTag, PopulationScope),
                _ => _stimulusPresenters
            };
        }

#if UNITY_EDITOR
        public void RepopulateSerialized(SerializedProperty property)
        {
            SerializedProperty presenterListProperty
            = property.FindPropertyRelative(nameof(_stimulusPresenters));

            Repopulate(property.serializedObject.targetObject as MonoBehaviour);
            int presenterCount = _stimulusPresenters.Count;

            presenterListProperty.arraySize = presenterCount;
            for (int i = 0; i < presenterCount; i++)
            {
                SerializedProperty presenterEntryProperty
                = presenterListProperty.GetArrayElementAtIndex(i);

                presenterEntryProperty.objectReferenceInstanceIDValue = _stimulusPresenters[i].GetInstanceID();
            }

            Debug.Log($"Found {presenterCount} Stimulus Presenters Matching Search Criteria");
        }
#endif

        public override List<StimulusPresenter> GetSelectable()
        {
            if (RepopulateWhenQueried) Repopulate();
            return base.GetSelectable();
        }
        public override List<StimulusPresenter> GetVisible()
        {
            if (RepopulateWhenQueried) Repopulate();
            return base.GetVisible();
        }
    }
}