using System.Collections;
using System.Collections.Generic;
using BCIEssentials.Stimulus.Presentation;

namespace BCIEssentials.Stimulus.Collections
{
    public class StimulusPresenterCollectionEnumerator : IEnumerator<StimulusPresentationBehaviour>, IEnumerator
    {
        private readonly StimulusPresenterCollection _source;
        private int _cursorIndex = -1;

        public StimulusPresentationBehaviour Current => _source[_cursorIndex];
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
