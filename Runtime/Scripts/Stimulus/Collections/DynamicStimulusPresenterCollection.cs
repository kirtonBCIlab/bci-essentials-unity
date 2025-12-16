using System.Collections.Generic;
using System.Linq;
using BCIEssentials.Stimulus.Presentation;
using BCIEssentials.Utilities;
using UnityEngine;

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


        public List<IStimulusPresenter> GetVisiblePresenters()
        => _stimulusPresenters.Where(p => p switch
            {
                Component c => TestGameObjectVisibility(c.gameObject),
                _ => true
            }
        ).ToList();
        
        public static bool TestGameObjectVisibility(GameObject gameObject)
        => (
            (
                gameObject.TryGetComponent(out CanvasRenderer canvasRenderer)
                && canvasRenderer.IsVisibleFromCanvas(Camera.main)
            )
        ||
            (
                gameObject.TryGetComponent(out Renderer renderer)
                && renderer.IsVisibleFrom(Camera.main)
            )
        );
        
    }
}