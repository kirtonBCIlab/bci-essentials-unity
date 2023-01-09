using System;
using BCIEssentials.StimulusObjects;

namespace BCIEssentials.Tests
{
    public class MockSPO : SPO
    {
        public Action TurnOnAction;

        public override float TurnOn()
        {
            TurnOnAction?.Invoke();

            return 0;
        }


        public Action TurnOffAction;

        public override void TurnOff()
        {
            TurnOffAction?.Invoke();
        }


        public Action OnSelectionAction;

        public override void OnSelection()
        {
            OnSelectionAction?.Invoke();
        }

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