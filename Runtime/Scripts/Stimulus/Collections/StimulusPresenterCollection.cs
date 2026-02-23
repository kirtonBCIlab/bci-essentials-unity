using System;
using System.Collections;
using System.Collections.Generic;
using BCIEssentials.Behaviours.Training;
using BCIEssentials.Stimulus.Presentation;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    using Presenter = StimulusPresentationBehaviour;
    using PresenterList = List<StimulusPresentationBehaviour>;
    public class StimulusPresenterCollection : TargetIndicationBehaviour, ICollection<Presenter>
    {
        public override int TargetCount => Count;
        public PresenterList LatestSubset => _latestSubset ?? _stimulusPresenters;
        private PresenterList _latestSubset;
        
        [SerializeField] protected PresenterList _stimulusPresenters;

        private int? _targetIndex;


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


        public Presenter this[int index] => GetPresenter(index);
        protected virtual Presenter GetPresenter(int index)
        {
            if (Count == 0)
            {
                Debug.Log("Can't index an empty collection");
                return null;
            }

            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            Presenter presenter = _stimulusPresenters[index];
            if (presenter == null)
            {
                Debug.LogWarning("Stimulus presenter is null and can't be selected");
            }

            return presenter;
        }


        public virtual PresenterList GetSelectable()
        => _latestSubset = _stimulusPresenters.WhereSelectable();
        public virtual PresenterList GetVisible()
        => _latestSubset = _stimulusPresenters.WhereVisibleFromMainCamera();
        public virtual PresenterList GetVisibleAndSelectable()
        => _latestSubset = GetSelectable().WhereVisibleFromMainCamera();


        public virtual bool IsReadOnly => false;
        public int Count => _stimulusPresenters.Count;
        public void Clear() => _stimulusPresenters.Clear();

        public void Add(Presenter presenter)
        => _stimulusPresenters.Add(presenter);
        public bool Remove(Presenter presenter)
        => _stimulusPresenters.Remove(presenter);
        public bool Contains(Presenter presenter)
        => _stimulusPresenters.Contains(presenter);
        public void CopyTo(Presenter[] array, int arrayIndex)
        => _stimulusPresenters.CopyTo(array, arrayIndex);

        public IEnumerator<Presenter> GetEnumerator()
        => new StimulusPresenterCollectionEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}