using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus
{
    public class TimeCycleFrequencyStimulusPresenter : FrequencyStimulusPresenter
    {
        [Space]
        public float Frequency = 8;

        private float _dutyCycleDelay = 0.5f / 8;
        private float _frameDebt;


        public void SetFrequency(float value)
        => UpdateDutyCycleDelay(Frequency = value);


        protected override void SetUpStimulusDisplay()
        => UpdateDutyCycleDelay(Frequency);

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

        private void UpdateDutyCycleDelay(float frequency)
        => _dutyCycleDelay = 0.5f / frequency;
    }
}