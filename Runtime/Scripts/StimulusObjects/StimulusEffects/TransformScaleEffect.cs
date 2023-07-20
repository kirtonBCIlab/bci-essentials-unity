using UnityEngine;

namespace BCIEssentials.StimulusEffects
{
    public class TransformScaleEffect: StimulusEffect
    {
        [SerializeField]
        [Tooltip("Value to multiply or divide the local scale value by.")]
        private float _scaleValue = 1;
        
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
    }
}