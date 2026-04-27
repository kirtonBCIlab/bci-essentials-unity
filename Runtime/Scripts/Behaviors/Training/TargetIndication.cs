using UnityEngine;

namespace BCIEssentials
{
    using Stimulus.Collections;

    public interface ITargetIndicator
    {
        public int TargetCount { get; }

        public void BeginTargetIndication(int index);
        public void EndTargetIndication();
    }


    public interface ITargetable
    {
        public void StartTargetIndication();
        public void EndTargetIndication();
    }


    public class StimulusPresenterCollectionTargetIndicator : ITargetIndicator
    {
        public virtual int TargetCount => _target.Count;

        private readonly StimulusPresenterCollection _target;
        private int? _lastIndicatedIndex;

        public StimulusPresenterCollectionTargetIndicator
            (StimulusPresenterCollection target) => _target = target;


        public virtual void BeginTargetIndication(int index)
        {
            _target[index].StartTargetIndication();
            _lastIndicatedIndex = index;
        }
        public virtual void EndTargetIndication()
        {
            if (!_lastIndicatedIndex.HasValue)
            {
                Debug.LogWarning("No item has been targetted for training.");
                return;
            }
            _target[_lastIndicatedIndex.Value].EndTargetIndication();
            _lastIndicatedIndex = null;
        }
    }
}