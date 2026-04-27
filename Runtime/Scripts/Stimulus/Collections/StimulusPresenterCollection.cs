using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    using Presenter = Presentation.StimulusPresentationBehaviour;
    using PresenterList = List<Presentation.StimulusPresentationBehaviour>;

    public class StimulusPresenterCollection: ICollection<Presenter>
    {
        public PresenterList LatestSubset => _latestSubset ?? GetSelectable();
        private PresenterList _latestSubset;
        
        [SerializeField] protected PresenterList _stimulusPresenters;


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