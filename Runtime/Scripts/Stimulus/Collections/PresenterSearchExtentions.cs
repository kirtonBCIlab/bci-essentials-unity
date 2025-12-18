using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BCIEssentials.Stimulus.Presentation;
using BCIEssentials.Utilities;

namespace BCIEssentials.Stimulus.Collections
{
    using static DynamicStimulusPresenterCollection;
    public static class PresenterSearchExtensions
    {
        public static List<IStimulusPresenter> GetSelectablePresentersByType
        (
            this MonoBehaviour caller,
            Scope scope = Scope.Scene,
            bool includeInactive = false
        )
        {
            IStimulusPresenter[] presenters = scope switch
            {
                Scope.Children
                    => caller.GetComponentsInChildren<IStimulusPresenter>(includeInactive)
                ,
                Scope.ChildrenOfParent
                    => caller.transform.parent switch
                    {
                        null => GetComponentsInScene<IStimulusPresenter>()
                        ,
                        Transform parent => parent.GetComponentsInChildren<IStimulusPresenter>(includeInactive)
                    }
                ,
                _
                    => GetComponentsInScene<IStimulusPresenter>()
            };
            return presenters.WhereSelectable();
        }

        public static T[] GetComponentsInScene<T>
        (
            bool includeInactive = false
        )
        {
            Scene scene = SceneManager.GetActiveScene();
            List<T> results = new();

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                results.AddRange(rootObject.GetComponentsInChildren<T>(includeInactive));
            }
            return results.ToArray();
        }


        public static List<IStimulusPresenter> GetSelectablePresentersByTag
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

            List<IStimulusPresenter> selectablePresenters = new();
            foreach (GameObject o in taggedObjects)
            {
                if (o.TryGetComponent(out IStimulusPresenter presenter) && presenter.IsSelectable)
                {
                    selectablePresenters.Add(presenter);
                }
            }
            return selectablePresenters;
        }
    }
}