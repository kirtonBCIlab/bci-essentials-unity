using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus
{
    [System.Serializable]
    public class ColourFlashBehaviour
    {
        public bool IsFlashing { get; private set; }

        public Renderer _renderer;
        public Color OnColour = Color.red;
        public Color OffColour = Color.white;

        [Header("Target Indication"), Min(0)]
        public float TargetIndicationFlashPeriod = 0.2f;
        public int TargetIndicationFlashCount = 3;

        [Header("Selection Indication"), Min(0)]
        public float SelectionFlashPeriod = 0.1f;
        public int SelectionFlashCount = 5;

        private HostedCoroutine _targetIndicationRoutine;
        private HostedCoroutine _selectionIndicationRoutine;


        public virtual void SetUp() => SetColour(OffColour);


        public virtual void StartSelectionIndication(MonoBehaviour executionHost)
        => _selectionIndicationRoutine = StartFlashRoutine
        (SelectionFlashPeriod, SelectionFlashCount, executionHost);
        public virtual void StopSelectionIndication()
        => _selectionIndicationRoutine?.Interrupt();

        public virtual void StartTargetIndication(MonoBehaviour executionHost)
        => _targetIndicationRoutine = StartFlashRoutine
        (TargetIndicationFlashPeriod, TargetIndicationFlashCount, executionHost);
        public virtual void EndTargetIndication()
        => _targetIndicationRoutine?.Interrupt();

        private HostedCoroutine StartFlashRoutine(
            float period, int count,
            MonoBehaviour executionHost
        )
        => new(executionHost, RunFlashes(period, count));

        protected virtual IEnumerator RunFlashes(float period, int count)
        {
            IsFlashing = true;
            for (int i = 0; i < count; i++)
            {
                SetColour(OnColour);
                yield return new WaitForSecondsRealtime(period);
                SetColour(OffColour);
                yield return new WaitForSecondsRealtime(period);
            }
            IsFlashing = false;
        }

        public virtual void SetColour(Color colour) => SetRendererColour(colour);
        protected void SetRendererColour(Color colour)
        {
            if (_renderer && _renderer.material)
            {
                _renderer.material.color = colour;
            }
        }
    }
}