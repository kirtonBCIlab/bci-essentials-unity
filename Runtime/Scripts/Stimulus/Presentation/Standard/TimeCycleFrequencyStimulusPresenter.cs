using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation.Standard
{
    public class TimeCycleFrequencyStimulusPresenter : FrequencyStimulusPresenter
    {
        public float Frequency
        {
            get => _frequency;
            set => _dutyCycleDelay = 0.5f / (_frequency = value);
        }
        private float _frequency = 8;
        private float _dutyCycleDelay = 0.5f / 8;
        private float _frameDebt;


        protected override IEnumerator RunDutyCycleDelay(bool _)
        {
            float time = GetCurrentTimestamp();
            yield return new WaitForEndOfFrame();

            float frameTime = GetCurrentTimestamp() - time;
            float remainingDelay = _dutyCycleDelay - frameTime;

            if (remainingDelay + _frameDebt > frameTime / 2)
            {
                yield return new WaitForSeconds(remainingDelay + _frameDebt);
                _frameDebt = 0;
            }
            else _frameDebt += remainingDelay;
        }

        private float GetCurrentTimestamp()
        => System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000f;
    }
}