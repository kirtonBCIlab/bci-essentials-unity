using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    using Extensions;
    using Stimulus.Presentation;
    using static Utilities.ComponentSearchMethods;

    public static class PresenterSearchExtensions
    {
        public static List<StimulusPresenter> GetSelectablePresentersByType
        (
            this MonoBehaviour caller,
            Scope scope = Scope.Scene,
            bool includeInactive = false
        )
        {
            StimulusPresenter[] presenters = scope switch
            {
                Scope.Children
                    => caller.GetComponentsInChildren<StimulusPresenter>(includeInactive)
                ,
                Scope.ChildrenOfParent
                    => caller.transform.parent switch
                    {
                        null => GetComponentsInScene<StimulusPresenter>()
                        ,
                        Transform parent => parent.GetComponentsInChildren<StimulusPresenter>(includeInactive)
                    }
                ,
                _
                    => GetComponentsInScene<StimulusPresenter>()
            };
            return presenters.WhereSelectable();
        }


        public static List<StimulusPresenter> GetSelectablePresentersByTag
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

            List<StimulusPresenter> selectablePresenters = new();
            foreach (GameObject o in taggedObjects)
            {
                if (o.TryGetComponent(out StimulusPresenter presenter) && presenter.IsSelectable)
                {
                    selectablePresenters.Add(presenter);
                }
            }
            return selectablePresenters;
        }
    }
}