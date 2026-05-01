using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Stimulus
{
    using PresenterList = List<StimulusPresenter>;

    [Serializable]
    public class StimulusPresenterCollection: ICollection<StimulusPresenter>
    {
        public PresenterList LatestSubset => _latestSubset ?? GetSelectable();
        private PresenterList _latestSubset;
        
        [SerializeField] protected PresenterList _stimulusPresenters;


        public StimulusPresenter this[int index] => GetPresenter(index);
        protected virtual StimulusPresenter GetPresenter(int index)
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

            StimulusPresenter presenter = _stimulusPresenters[index];
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

        public void Add(StimulusPresenter presenter)
        => _stimulusPresenters.Add(presenter);
        public bool Remove(StimulusPresenter presenter)
        => _stimulusPresenters.Remove(presenter);
        public bool Contains(StimulusPresenter presenter)
        => _stimulusPresenters.Contains(presenter);
        public void CopyTo(StimulusPresenter[] array, int arrayIndex)
        => _stimulusPresenters.CopyTo(array, arrayIndex);

        public IEnumerator<StimulusPresenter> GetEnumerator()
        => new StimulusPresenterCollectionEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}