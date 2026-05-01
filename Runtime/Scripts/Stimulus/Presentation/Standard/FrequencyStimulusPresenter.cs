using System.Threading;
using UnityEngine;

namespace BCIEssentials.Stimulus
{
    public abstract class FrequencyStimulusPresenter : ColourToggleStimulusPresenter
    {
        public float Frequency => CycleEngine.Frequency;
        public DutyCycleEngine CycleEngine;

        protected bool _lastDisplayedCycleState;


        protected virtual void Update()
        {
            if (!CycleEngine.IsRunning) return;

            if (CycleEngine.StateFlag != _lastDisplayedCycleState)
            {
                ToggleDisplayState(CycleEngine.StateFlag);
                _lastDisplayedCycleState = CycleEngine.StateFlag;
            }
        }


        public override void StartStimulusDisplay()
        {
            SetUpStimulusDisplay();
            CycleEngine.StartCycle();
        }

        public override void EndStimulusDisplay()
        {
            CycleEngine.StopCycle();
            CleanUpStimulusDisplay();
        }


        protected virtual void SetUpStimulusDisplay() { }
        protected virtual void CleanUpStimulusDisplay() { }

        protected virtual void ToggleDisplayState(bool value)
        => _colourFlashBehaviour.ToggleDisplayState(value);


        [System.Serializable]
        public class DutyCycleEngine
        {
            public bool StateFlag { get; private set; }
            public bool IsRunning { get; private set; }

            [Min(1)]
            public float Frequency = 8;
            [Range(0, 1)]
            public float DutyCycle = 0.5f;

            private Thread _cycleThread;


            public void StartCycle()
            {
                if (_cycleThread?.IsAlive == true)
                {
                    IsRunning = false;
                    _cycleThread.Join();
                }
                _cycleThread = new(RunCycle);
                _cycleThread.Start();
                IsRunning = true;
            }
            public void StopCycle() => IsRunning = false;


            protected void RunCycle()
            {
                while (IsRunning)
                {
                    StateFlag = true;
                    SleepForSeconds(DutyCycle / Frequency);
                    StateFlag = false;
                    SleepForSeconds((1 - DutyCycle) / Frequency);
                }
            }

            private void SleepForSeconds(float seconds)
            => Thread.Sleep((int)(seconds * 1000));
        }
    }
}