using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BCIEssentials.StimulusObjects;

namespace BCIEssentials.ControllerBehaviors
{
    public static class SPOPopulationExtensions
    {
        public static List<SPO> GetSelectableSPOsByType
        (
            this MonoBehaviour caller,
            SPOPopulationScope scope = SPOPopulationScope.Global,
            bool includeInactive = false
        )
        {
            SPO[] spos = scope switch
            {
                SPOPopulationScope.Children
                    => caller.GetComponentsInChildren<SPO>(includeInactive)
                ,
                SPOPopulationScope.ChildrenOfParent
                    => caller.transform.parent switch
                    {
                        null => Object.FindObjectsOfType<SPO>(includeInactive)
                        ,
                        _ => caller.GetComponentsInChildrenOfParent<SPO>(includeInactive)
                    }
                ,
                _
                    => Object.FindObjectsOfType<SPO>(includeInactive)
            };
            return spos.Where(spo => spo.Selectable).ToList();
        }

        public static T[] GetComponentsInChildrenOfParent<T>
        (
            this MonoBehaviour caller,
            bool includeInactive = false
        ) where T: Component
        => caller.transform.parent.GetComponentsInChildren<T>(includeInactive);


        public static List<SPO> GetSelectableSPOsByTag
        (
            this MonoBehaviour caller, string spoTag,
            SPOPopulationScope scope = SPOPopulationScope.Global
        )
        {
            GameObject[] taggedObjects = scope switch
            {
                SPOPopulationScope.Children
                    => caller.GetChildObjectsWithTag(spoTag)
                ,
                SPOPopulationScope.ChildrenOfParent
                    => caller.transform.parent switch
                    {
                        null => GameObject.FindGameObjectsWithTag(spoTag)
                        ,
                        _ => caller.transform.parent.GetChildObjectsWithTag(spoTag)
                    }
                ,
                _
                    => GameObject.FindGameObjectsWithTag(spoTag)
            };

            List<SPO> selectableSPOs = new();
            foreach (GameObject o in taggedObjects)
            {
                if (o.TryGetComponent(out SPO spo) && spo.Selectable)
                    selectableSPOs.Add(spo);
            }
            return selectableSPOs;
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
    }
}