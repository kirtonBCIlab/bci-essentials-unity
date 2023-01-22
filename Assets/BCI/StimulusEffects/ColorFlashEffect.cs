using System.Collections;
using UnityEngine;

namespace BCIEssentials.StimulusEffects
{
    /// <summary>
    /// Assign or Flash a renderers material color.
    /// </summary>
    public class ColorFlashEffect : StimulusEffect
    {
        [SerializeField]
        [Tooltip("The renderer to assign the material color to")]
        private Renderer _renderer;

        [Header("Flash Settings")]
        [SerializeField]
        [Tooltip("Material Color to assign while flashing is on")]
        private Color _flashOnColor;
        
        [SerializeField]
        [Min(0)]
        private float _flashDurationSeconds = 0.2f;

        [SerializeField]
        [Min(1)]
        private int _flashAmount = 3;

        public bool IsPlaying => _effectRoutine != null;


        private Color _originalColor;
        private Coroutine _effectRoutine;

        private void Awake()
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
        }

        public override void SetOn()
        {
            if (_renderer == null || _renderer.material == null)
            {
                return;
            }

            _originalColor = _renderer.material.color;
            AssignMaterialColor(_flashOnColor);
        }

        public override void SetOff()
        {
            if (!IsOn || _renderer == null || _renderer.material == null)
            {
                return;
            }
            
            AssignMaterialColor(_originalColor);
            IsOn = false;
        }

        public void Play()
        {
            Stop();
            _effectRoutine = StartCoroutine(RunEffect());
        }

        private void Stop()
        {
            if (!IsPlaying)
            {
                return;
            }

            SetOff();
            StopCoroutine(_effectRoutine);
            _effectRoutine = null;
        }

        private IEnumerator RunEffect()
        {
            if (_renderer != null && _renderer.material != null)
            {
                IsOn = true;
                
                for (var i = 0; i < _flashAmount; i++)
                {
                    //Deliberately not using SetOn and SetOff here
                    //to avoid excessive null checking
                    
                    AssignMaterialColor(_flashOnColor);
                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);

                    AssignMaterialColor(_originalColor);
                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);
                }
            }

            SetOff();
            _effectRoutine = null;
        }

        private void AssignMaterialColor(Color color)
        {
            _renderer.material.color = color;
        }
    }
}