using UnityEngine;

namespace BCIEssentials.StimulusEffects
{
    public class TransformScaleEffect: StimulusEffect
    {
        [SerializeField]
        [Tooltip("Value to multiply or divide the local scale value by.")]
        private float _scaleValue = 1;

        [SerializeField]
        [Tooltip("Duration of the scale effect for selection")]
        private float _scaleDuration = 0.2f;
        
        /// <summary>
        /// Multiplies the objects local scale by <see cref="_scaleValue"/>
        /// </summary>
        public override void SetOn()
        {
            base.SetOn();
            transform.localScale *= _scaleValue;
        }

        /// <summary>
        /// Divides the objects local scale by <see cref="_scaleValue"/>
        /// </summary>
        public override void SetOff()
        {
            base.SetOff();
            
            transform.localScale /= _scaleValue;
        }

        /// <summary>
        /// Scales up then down the object's scale quickly. This still has some issues at the moment.
        /// </summary>
        public void PlayEffect()
        {
            base.SetOn();
            transform.localScale *= _scaleValue;
            new WaitForSeconds(_scaleDuration);
            base.SetOff();
            transform.localScale /= _scaleValue;
        }
    }
}