using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation.Standard
{
    public abstract class ColourFlashBehaviour: MonoBehaviourUsingExtendedAttributes
    {
        public bool IsFlashing { get; private set; }
        [SerializeField]
        private Renderer _renderer;

        public Color OnColour = Color.red;
        public Color OffColour = Color.white;

        [StartFoldoutGroup("Target Indication"), Min(0)]
        public float TargetIndicationFlashPeriod = 0.2f;
        public int TargetIndicationFlashCount = 3;

        [StartFoldoutGroup("Selection Indication"), Min(0)]
        public float SelectionFlashPeriod = 0.1f;
        [EndFoldoutGroup]
        public int SelectionFlashCount = 5;

        private Coroutine _targetIndicationRoutine;
        private Coroutine _selectionIndicationRoutine;


        protected virtual void Awake()
        {
            if (_renderer == null && !gameObject.TryGetComponent(out _renderer))
            {
                Debug.LogWarning($"No Renderer component found for {gameObject.name}");
                return;
            }

            if (_renderer.material == null)
            {
                Debug.LogWarning($"No material assigned to renderer component on {gameObject.name}.");
            }

            SetColour(OffColour);
        }


        public virtual void StartSelectionIndication()
        => _selectionIndicationRoutine = StartFlashRoutine(
            SelectionFlashPeriod,
            SelectionFlashCount
        );
        public virtual void StopSelectionIndication()
        {
            if (_selectionIndicationRoutine != null)
            {
                StopCoroutine(_selectionIndicationRoutine);
            }
        }

        public virtual void StartTargetIndication()
        => _targetIndicationRoutine = StartFlashRoutine(
            TargetIndicationFlashPeriod,
            TargetIndicationFlashCount
        );
        public virtual void EndTargetIndication()
        {
            if (_targetIndicationRoutine != null)
            {
                StopCoroutine(_targetIndicationRoutine);
            }
        }

        private Coroutine StartFlashRoutine(float period, int count)
        => StartCoroutine(RunFlashes(period, count));
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