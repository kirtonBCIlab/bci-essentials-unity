using System.Collections.Generic;
using BCIEssentials.Behaviours;
using BCIEssentials.Training;
using UnityEngine;

namespace BCIEssentials.Selection.Collections
{
    public abstract class SelectableCollection : MonoBehaviour, IBCISelector, IBCITrainingTargetIndicator
    {
        public int OptionCount => _selectables.Count;
        protected List<ISelectable> _selectables;

        private int? _targetIndex;


        public void MakeSelection(int index)
        => GetSelectable(index).Select();

        public void BeginTargetIndication(int index)
        {
            GetSelectable(index)?.StartTargetIndication();
            _targetIndex = index;
        }
        public void EndTargetIndication()
        {
            if (!_targetIndex.HasValue)
            {
                Debug.LogWarning("No item has been targetted for training.");
                return;
            }
            GetSelectable(_targetIndex.Value)?.EndTargetIndication();
            _targetIndex = null;
        }


        protected virtual ISelectable GetSelectable(int index)
        {
            if (OptionCount == 0)
            {
                Debug.Log("Nothing to Select");
                return null;
            }

            if (index < 0 || index >= OptionCount)
            {
                Debug.LogWarning($"Invalid Selection. Index {index} must be between (0 and {OptionCount})");
                return null;
            }

            ISelectable selectable = _selectables[index];
            if (selectable == null)
            {
                Debug.LogWarning("Selectable is null and can't be selected");
            }

            return selectable;
        }
    }
}