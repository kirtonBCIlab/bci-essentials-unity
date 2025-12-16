using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BCIEssentials.Selection;
using BCIEssentials.Stimulus.Presentation;
using BCIEssentials.Training;
using UnityEngine;

namespace BCIEssentials.Stimulus.Collections
{
    public class StimulusPresenterCollection : MonoBehaviourUsingExtendedAttributes, ISelector, ITrainingTargetIndicator, ICollection<IStimulusPresenter>
    {
        public int OptionCount => _stimulusPresenters.Count;
        [SerializeField] protected List<IStimulusPresenter> _stimulusPresenters;

        private int? _targetIndex;


        public void MakeSelection(int index)
        => GetPresenter(index).Select();

        public void BeginTargetIndication(int index)
        {
            GetPresenter(index)?.StartTargetIndication();
            _targetIndex = index;
        }
        public void EndTargetIndication()
        {
            if (!_targetIndex.HasValue)
            {
                Debug.LogWarning("No item has been targetted for training.");
                return;
            }
            GetPresenter(_targetIndex.Value)?.EndTargetIndication();
            _targetIndex = null;
        }


        public IStimulusPresenter this[int index] => GetPresenter(index);
        protected virtual IStimulusPresenter GetPresenter(int index)
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

            IStimulusPresenter presenter = _stimulusPresenters[index];
            if (presenter == null)
            {
                Debug.LogWarning("Stimulus presenter is null and can't be selected");
            }

            return presenter;
        }


        public List<IStimulusPresenter> SelectableStimulusPresenters
        => GetSelectable();
        public List<IStimulusPresenter> GetSelectable()
        => _stimulusPresenters.Where(p => p.IsSelectable).ToList();


        public virtual bool IsReadOnly => false;
        public int Count => _stimulusPresenters.Count;
        public void Clear() => _stimulusPresenters.Clear();

        public void Add(IStimulusPresenter presenter)
        => _stimulusPresenters.Add(presenter);
        public bool Remove(IStimulusPresenter presenter)
        => _stimulusPresenters.Remove(presenter);
        public bool Contains(IStimulusPresenter presenter)
        => _stimulusPresenters.Contains(presenter);
        public void CopyTo(IStimulusPresenter[] array, int arrayIndex)
        => _stimulusPresenters.CopyTo(array, arrayIndex);

        public IEnumerator<IStimulusPresenter> GetEnumerator()
        => new StimulusPresenterEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class StimulusPresenterEnumerator : IEnumerator<IStimulusPresenter>, IEnumerator
        {
            private readonly StimulusPresenterCollection _source;
            private int _cursorIndex = -1;

            public IStimulusPresenter Current => _source[_cursorIndex];
            object IEnumerator.Current => Current;

            public StimulusPresenterEnumerator(StimulusPresenterCollection collection)
            => _source = collection;

            public bool MoveNext()
            {
                _cursorIndex++;
                return _cursorIndex < _source.Count;
            }
            public void Reset() => _cursorIndex = -1;
            public void Dispose() { }
        }
    }
}