using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    using Extensions;
    using Stimulus.Collections;

    [RequireComponent(typeof(StimulusPresenterCollection))]
    public class StimulusPresenterCollectionTargetIndicator : TargetIndicationBehaviour
    {
        public override int TargetCount => _target.Count;

        [SerializeField]
        private StimulusPresenterCollection _target;
        private int? _lastIndicatedIndex;


        private void Reset() => this.CoalesceComponentReference(ref _target);
        private void Start() => this.CoalesceComponentReference(ref _target);


        public override void BeginTargetIndication(int index)
        {
            _target[index].StartTargetIndication();
            _lastIndicatedIndex = index;
        }
        public override void EndTargetIndication()
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