using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Base class for the Stimulus Presenting Objects (SPOs)

namespace BCIEssentials.StimulusObjects
{
   /// <summary>
    /// Base class for the Stimulus Presenting Objects (SPOs)
    /// </summary>
    public class SPO : MonoBehaviour
    {
        [Space(20)]
        [Tooltip("Invoked when the SPO Controller requests this stimulus to start.")]
        public UnityEvent StartStimulusEvent = new();

        [Tooltip("Invoked when the SPO Controller requests this stimulus to stop.")]
        public UnityEvent StopStimulusEvent = new();

        [Tooltip("Invoked when the SPO Controller selects this SPO")]
        public UnityEvent OnSelectedEvent = new();

        [Tooltip("Invoked when the SPO Controller requests training to start")]
        public UnityEvent StartTrainingStimulusEvent = new();

        [Tooltip("Invoked when the SPO Controller requests training to stop.")]
        public UnityEvent StopTrainingStimulusEvent = new();

        /// <summary>
        /// Determines if this object is available to be selected
        /// by the <see cref="Controller"/>;
        /// </summary>
        public bool Selectable = true;

        /// <summary>
        /// Assigned by the SPO Controller, this represents the
        /// index of this SPO in the controllers pool of selectables. 
        /// </summary>
        public int SelectablePoolIndex;

        /// <summary>
        /// Assigned in editor or by the PopulateObjectIDList method, this 
        /// is a unique identifier for this SPO. It defaults to -100.
        /// </summary>
        public int ObjectID = -100;

        /// <summary>
        /// Request this SPO stimulus to begin.
        /// </summary>
        /// <returns>The time at the beginning of this frame using <see cref="Time.time"/></returns>
        public virtual float StartStimulus()
        {
            StartStimulusEvent?.Invoke();

            //Stimulus request time
            return Time.time;
        }

        /// <summary>
        /// Request this SPO stimulus to end.
        /// </summary>
        public virtual void StopStimulus()
        {
            StopStimulusEvent?.Invoke();
        }

        /// <summary>
        /// When this SPO has been selected.
        /// </summary>
        public virtual void Select()
        {
            OnSelectedEvent?.Invoke();
        }

        //TODO: Remove when refactored training out
        // What to do when targeted for training selection
        public virtual void OnTrainTarget()
        {
            try
            {
                //TODO - If it evaluates as null, try to invoke the default StartStimulus option.
                StartTrainingStimulusEvent?.Invoke();
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
                StopTrainingStimulusEvent?.Invoke();
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


    }
}
