using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// Base class for the Stimulus Presenting Objects (SPOs)

namespace BCIEssentials.StimulusObjects
{
   /// <summary>
    /// Base class for the Stimulus Presenting Objects (SPOs)
    /// </summary>
    public class SPO : MonoBehaviour
    {
        /// <summary>
        /// Determines if this object is available to be selected
        /// by the <see cref="Controller"/>;
        /// </summary>
        public bool Selectable = true;


        [Header("Start stimulus presentation")]
        [FormerlySerializedAs("StartStimulusEvent")]
        public UnityEvent OnStimulusTriggered = new();

        [Header("Stop stimulus presentation")]
        [FormerlySerializedAs("StopStimulusEvent")]
        public UnityEvent OnStimulusEndTriggered = new();

        [FormerlySerializedAs("OnSelectedEvent")]
        public UnityEvent OnSelected = new();

        public UnityEvent OnSetAsTrainingTarget = new();
        public UnityEvent OnRemovedAsTrainingTarget = new();


        /// <summary>
        /// Request this SPO stimulus to begin.
        /// </summary>
        /// <returns>The time at the beginning of this frame using <see cref="Time.time"/></returns>
        public virtual float StartStimulus()
        {
            OnStimulusTriggered?.Invoke();

            //Stimulus request time
            return Time.time;
        }

        /// <summary>
        /// Request this SPO stimulus to end.
        /// </summary>
        public virtual void StopStimulus()
        {
            OnStimulusEndTriggered?.Invoke();
        }

        /// <summary>
        /// When this SPO has been selected.
        /// </summary>
        public virtual void Select()
        {
            OnSelected?.Invoke();
        }

        //TODO: Remove when refactored training out
        // What to do when targeted for training selection
        public virtual void OnTrainTarget()
        {
            try
            {
                //TODO - If it evaluates as null, try to invoke the default StartStimulus option.
                OnSetAsTrainingTarget?.Invoke();
            }
            catch(UnassignedReferenceException e)
            {
                Debug.Log(e.Message);
            }

        }

        // What to do when untargeted
        public virtual void OffTrainTarget()
        {
            try
            {
                //TODO - If evaluates as null, try to invoke the default StopStimulus option
                OnRemovedAsTrainingTarget?.Invoke();
            }
            catch(UnassignedReferenceException e)
            {
                Debug.Log(e.Message);
            }

        }

        public virtual void DefaultScaleEffectOn()
        {
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue,
                objectScale.z * scaleValue);
        }

        public virtual void DefaultScaleEffectOff()
        {
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x / scaleValue, objectScale.y / scaleValue,
                    objectScale.z / scaleValue);
        }

        public virtual void DefaultColorEffectOn()
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.color = Color.yellow;
        }

        public virtual void DefaultColorEffectOff()
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.color = Color.white;
        }
    }
}
