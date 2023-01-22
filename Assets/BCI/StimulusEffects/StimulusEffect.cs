using UnityEngine;

namespace BCIEssentials.StimulusEffects
{
    /// <summary>
    /// Base class for Stimulus Effects.
    /// </summary>
    public abstract class StimulusEffect : MonoBehaviour
    {
        /// <summary>
        /// If this effect is currently on.
        /// </summary>
        public bool IsOn { get; protected set; }

        /// <summary>
        /// Requests the effect to be on.
        /// </summary>
        public virtual void SetOn()
        {
            IsOn = true;
        }

        /// <summary>
        /// Requests the effect to be off.
        /// </summary>
        public virtual void SetOff()
        {
            IsOn = false;
        }
    }
}