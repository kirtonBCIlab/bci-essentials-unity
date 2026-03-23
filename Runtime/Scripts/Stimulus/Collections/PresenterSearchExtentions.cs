using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    using Extensions;
    using Stimulus.Presentation;
    using static Utilities.ComponentSearchMethods;
    using Scope = DynamicStimulusPresenterCollection.Scope;

    public static class PresenterSearchExtensions
    {
        public static List<StimulusPresentationBehaviour> GetSelectablePresentersByType
        (
            this MonoBehaviour caller,
            Scope scope = Scope.Scene,
            bool includeInactive = false
        )
        {
            StimulusPresentationBehaviour[] presenters = scope switch
            {
                Scope.Children
                    => caller.GetComponentsInChildren<StimulusPresentationBehaviour>(includeInactive)
                ,
                Scope.ChildrenOfParent
                    => caller.transform.parent switch
                    {
                        null => GetComponentsInScene<StimulusPresentationBehaviour>()
                        ,
                        Transform parent => parent.GetComponentsInChildren<StimulusPresentationBehaviour>(includeInactive)
                    }
                ,
                _
                    => GetComponentsInScene<StimulusPresentationBehaviour>()
            };
            return presenters.WhereSelectable();
        }


        public static List<StimulusPresentationBehaviour> GetSelectablePresentersByTag
        (
            this MonoBehaviour caller, string tag,
            Scope scope = Scope.Scene
        )
        {
            GameObject[] taggedObjects = scope switch
            {
                Scope.Children
                    => caller.GetChildObjectsWithTag(tag)
                ,
                Scope.ChildrenOfParent
                    => caller.transform.parent switch
                    {
                        null => GameObject.FindGameObjectsWithTag(tag)
                        ,
                        Transform parent => parent.GetChildObjectsWithTag(tag)
                    }
                ,
                _
                    => GameObject.FindGameObjectsWithTag(tag)
            };

            List<StimulusPresentationBehaviour> selectablePresenters = new();
            foreach (GameObject o in taggedObjects)
            {
                if (o.TryGetComponent(out StimulusPresentationBehaviour presenter) && presenter.IsSelectable)
                {
                    selectablePresenters.Add(presenter);
                }
            }
            return selectablePresenters;
        }
    }
}