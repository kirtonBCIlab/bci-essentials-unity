using System.Collections;
using UnityEngine;
using BCIEssentials.Utilities;

namespace BCIEssentials.StimulusEffects
{
    public class WormFlash : StimulusEffect
    {
        [SerializeField]
        [Tooltip("The renderer to assign the material color to")]
        private Renderer _renderer;
        
        [SerializeField]
        [Min(0)]
        private float _flashDurationSeconds = 0.2f;

        [SerializeField]
        [Min(1)]
        private int _flashAmount = 3;

        [SerializeField]
        [Tooltip("If the flash on color is applied on start or the flash off color.")]
        private bool _startOn;

        public bool IsPlaying => _effectRoutine != null;

        private Coroutine _effectRoutine;

        // Start is called before the first frame update
        void Awake()
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

            SetTexture(0.5f, 0.2f);
        }

        public override void SetOn()
        {
            if (_renderer == null || _renderer.material == null)
            {
                return;
            }

            SetTexture(0.5f, 0.2f);
            IsOn = true;
        }


        public override void SetOff()
        {
            if (_renderer == null || _renderer.material == null)
            {
                return;
            }
            
            SetTexture(0.5f, 1f);
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
                    SetTexture(0.5f, 0.2f);
                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);

                    SetTexture(0.5f, 1f);
                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);
                }
            }

            SetOff();
            _effectRoutine = null;
        }

        private void SetTexture(float val, float val2)
        {
            _renderer.material.SetFloat("_Float", val);
            _renderer.material.SetFloat("_Float2", val2);
        }

    }
}
