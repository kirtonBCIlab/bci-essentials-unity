using BCIEssentials.Extensions;
using BCIEssentials.LSLFramework;
using BCIEssentials.Stimulus.Collections;
using UnityEngine;

namespace BCIEssentials.Selection
{
    [RequireComponent(typeof(StimulusPresenterCollection))]
    public class StimulusPresenterCollectionSelector : SelectionBehaviour
    {
        [SerializeField]
        private StimulusPresenterCollection _target;

        private void Start() => this.CoalesceComponentReference(ref _target);

        public override void OnPrediction(Prediction prediction)
        => _target[prediction.Index].Select();
    }
}