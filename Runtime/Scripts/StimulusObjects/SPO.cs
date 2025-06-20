using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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
        public UnityEvent OnSelected = new();

        [Header("Start stimulus presentation")]
        public UnityEvent OnStimulusTriggered = new();

        [Header("Stop stimulus presentation")]
        public UnityEvent OnStimulusEndTriggered = new();

        [Header("Indicate training target")]
        public UnityEvent OnSetAsTrainingTarget = new();
        public UnityEvent OnRemovedAsTrainingTarget = new();


        /// <summary>
        /// Select this SPO.
        /// </summary>
        public void Select()
        => TriggerEventAndRoutine
        (OnSelected, RunSelectedRoutine);
        protected virtual IEnumerator RunSelectedRoutine()
        { yield return null; }

        /// <summary>
        /// Request this SPO stimulus to begin.
        /// </summary>
        public void StartStimulus()
        => TriggerEventAndRoutine
        (OnStimulusTriggered, RunStartStimulusRoutine);
        protected virtual IEnumerator RunStartStimulusRoutine()
        { yield return null; }

        /// <summary>
        /// Request this SPO stimulus to end.
        /// </summary>
        public void StopStimulus()
        => TriggerEventAndRoutine
        (OnStimulusEndTriggered, RunStopStimulusRoutine);
        protected virtual IEnumerator RunStopStimulusRoutine()
        { yield return null; }


        /// <summary>
        /// Target this SPO for training.
        /// </summary>
        public void OnTrainTarget()
        => TriggerEventAndRoutine
        (OnSetAsTrainingTarget, RunTargetedRoutine);
        protected virtual IEnumerator RunTargetedRoutine()
        { yield return null; }

        /// <summary>
        /// Untarget this SPO
        /// </summary>
        public void OffTrainTarget()
        => TriggerEventAndRoutine
        (OnRemovedAsTrainingTarget, RunUntargetedRoutine);
        protected virtual IEnumerator RunUntargetedRoutine()
        { yield return null; }


        private void TriggerEventAndRoutine
        (
            UnityEvent targetEvent,
            Func<IEnumerator> targetRoutine
        )
        {
            targetEvent?.Invoke();
            StartCoroutine(targetRoutine());
        }


        public void DefaultScaleEffectOn()
        {
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue,
                objectScale.z * scaleValue);
        }

        public void DefaultScaleEffectOff()
        {
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x / scaleValue, objectScale.y / scaleValue,
                    objectScale.z / scaleValue);
        }

        public void DefaultColorEffectOn()
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.color = Color.yellow;
        }

        public void DefaultColorEffectOff()
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.color = Color.white;
        }
    }
}
