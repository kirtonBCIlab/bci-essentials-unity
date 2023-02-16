using System;
using BCIEssentials.StimulusObjects;

namespace BCIEssentials.Tests
{
    public class MockSPO : SPO
    {
        public Action OnTrainTargetAction;

        public override void OnTrainTarget()
        {
            OnTrainTargetAction?.Invoke();
        }

        public Action OffTrainTargetAction;

        public override void OffTrainTarget()
        {
            OffTrainTargetAction?.Invoke();
        }
    }
}