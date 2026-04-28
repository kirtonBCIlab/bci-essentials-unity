using System.Collections.Generic;

namespace BCIEssentials.Stimulus.Collections
{
    using Presentation;
    using UnityEngine;

    public enum SearchMethod { Type, Tag }
    public enum Scope { Scene, Children, ChildrenOfParent }

    [System.Serializable]
    public class DynamicStimulusPresenterCollection : StimulusPresenterCollection
    {
        [StartFoldoutGroup("Presenter Discovery")]
        public SearchMethod PopulationMethod;
        public Scope PopulationScope;

        [ShowIf(nameof(PopulationMethod), (int)SearchMethod.Tag)]
        public string PresenterTag = "BCI";
        [EndFoldoutGroup] public bool RepopulateWhenQueried;

        private readonly MonoBehaviour _defaultSearchAnchor;

        public DynamicStimulusPresenterCollection(MonoBehaviour defaultSearchAnchor)
        => _defaultSearchAnchor = defaultSearchAnchor;


        [ContextMenu("Locate Presenters")]
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


        public override List<StimulusPresentationBehaviour> GetSelectable()
        {
            if (RepopulateWhenQueried) Repopulate();
            return base.GetSelectable();
        }
        public override List<StimulusPresentationBehaviour> GetVisible()
        {
            if (RepopulateWhenQueried) Repopulate();
            return base.GetVisible();
        }
    }
}