using System.Collections;
using System.Collections.Generic;

namespace BCIEssentials.Stimulus.Collections
{
    using Presentation;

    public class StimulusPresenterCollectionEnumerator : IEnumerator<StimulusPresenter>, IEnumerator
    {
        private readonly StimulusPresenterCollection _source;
        private int _cursorIndex = -1;

        public StimulusPresenter Current => _source[_cursorIndex];
        object IEnumerator.Current => Current;

        public StimulusPresenterCollectionEnumerator(StimulusPresenterCollection collection)
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
