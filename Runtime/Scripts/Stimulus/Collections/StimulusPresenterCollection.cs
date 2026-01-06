using System;
using System.Collections;
using System.Collections.Generic;
using BCIEssentials.Behaviours.Training;
using BCIEssentials.Stimulus.Presentation;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    public class StimulusPresenterCollection : TargetIndicationBehaviour, ICollection<StimulusPresentationBehaviour>
    {
        public override int OptionCount => _stimulusPresenters.Count;
        [SerializeField] protected List<StimulusPresentationBehaviour> _stimulusPresenters;

        private int? _targetIndex;


        public override void MakeSelection(int index)
        => GetPresenter(index).Select();

        public override void BeginTargetIndication(int index)
        {
            GetPresenter(index)?.StartTargetIndication();
            _targetIndex = index;
        }
        public override void EndTargetIndication()
        {
            if (!_targetIndex.HasValue)
            {
                Debug.LogWarning("No item has been targetted for training.");
                return;
            }
            GetPresenter(_targetIndex.Value)?.EndTargetIndication();
            _targetIndex = null;
        }


        public StimulusPresentationBehaviour this[int index] => GetPresenter(index);
        protected virtual StimulusPresentationBehaviour GetPresenter(int index)
        {
            if (OptionCount == 0)
            {
                Debug.Log("Can't index an empty collection");
                return null;
            }

            if (index < 0 || index >= OptionCount)
            {
                throw new IndexOutOfRangeException();
            }

            StimulusPresentationBehaviour presenter = _stimulusPresenters[index];
            if (presenter == null)
            {
                Debug.LogWarning("Stimulus presenter is null and can't be selected");
            }

            return presenter;
        }


        public virtual List<StimulusPresentationBehaviour> GetSelectable()
        => _stimulusPresenters.WhereSelectable();
        public virtual List<StimulusPresentationBehaviour> GetVisible()
        => _stimulusPresenters.WhereVisibleFromMainCamera();
        public virtual List<StimulusPresentationBehaviour> GetVisibleAndSelectable()
        => GetSelectable().WhereVisibleFromMainCamera();


        public virtual bool IsReadOnly => false;
        public int Count => _stimulusPresenters.Count;
        public void Clear() => _stimulusPresenters.Clear();

        public void Add(StimulusPresentationBehaviour presenter)
        => _stimulusPresenters.Add(presenter);
        public bool Remove(StimulusPresentationBehaviour presenter)
        => _stimulusPresenters.Remove(presenter);
        public bool Contains(StimulusPresentationBehaviour presenter)
        => _stimulusPresenters.Contains(presenter);
        public void CopyTo(StimulusPresentationBehaviour[] array, int arrayIndex)
        => _stimulusPresenters.CopyTo(array, arrayIndex);

        public IEnumerator<StimulusPresentationBehaviour> GetEnumerator()
        => new StimulusPresenterCollectionEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}