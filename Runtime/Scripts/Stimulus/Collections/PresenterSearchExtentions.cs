using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BCIEssentials.Stimulus.Presentation;
using UnityEngine.SceneManagement;

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
            FindObjectsInactive findObjectsInactive = includeInactive
            ? FindObjectsInactive.Include
            : FindObjectsInactive.Exclude;

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
            return presenters.Where(presenter => presenter.IsSelectable).ToList();
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


        public static GameObject[] GetChildObjectsWithTag
        (
            this MonoBehaviour caller, string tag
        )
        => caller.transform.GetChildObjectsWithTag(tag);

        public static GameObject[] GetChildObjectsWithTag
        (
            this Transform caller, string tag
        )
        => caller.GetChildrenWithTag(tag).SelectGameObjects();

        public static List<Transform> GetChildrenWithTag
        (
            this Transform caller, string tag
        )
        {
            List<Transform> result = new();
            foreach (Transform child in caller)
            {
                if (child.CompareTag(tag)) result.Add(child);
                result.AddRange(child.GetChildrenWithTag(tag));
            }
            return result;
        }

        private static GameObject[] SelectGameObjects
        (
            this IEnumerable<Transform> caller
        )
        => caller.Select(t => t.gameObject).ToArray();

        public static List<GameObject> SelectGameObjects
        (
            this IEnumerable<Component> caller
        )
        => caller.Select(c => c.gameObject).ToList();
    }
}