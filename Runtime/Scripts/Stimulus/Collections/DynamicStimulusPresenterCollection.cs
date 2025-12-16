using System.Collections.Generic;
using BCIEssentials.Stimulus.Presentation;

namespace BCIEssentials.Stimulus.Collections
{
    public class DynamicStimulusPresenterCollection : StimulusPresenterCollection
    {
        public enum SearchMethod { Type, Tag }
        public enum Scope { Scene, Children, ChildrenOfParent }

        public SearchMethod PopulationMethod;
        public Scope PopulationScope;

        [ShowIf(nameof(PopulationMethod), (int)SearchMethod.Tag)]
        public string PresenterTag = "BCI";


        public void Repopulate()
        {
            _stimulusPresenters = PopulationMethod switch
            {
                SearchMethod.Type => this.GetSelectablePresentersByType(PopulationScope),
                SearchMethod.Tag => this.GetSelectablePresentersByTag(PresenterTag, PopulationScope),
                _ => _stimulusPresenters
            };
        }


        public List<IStimulusPresenter> VisibleStimulusPresenters
        => GetVisible();
        public List<IStimulusPresenter> VisibleAndSelectableStimulusPresenters
        => GetVisibleAndSelectable();

        public List<IStimulusPresenter> GetVisible()
        => _stimulusPresenters.WhereVisibleFromMainCamera();

        public List<IStimulusPresenter> GetVisibleAndSelectable()
        => _stimulusPresenters.WhereSelectable().WhereVisibleFromMainCamera();
    }
}