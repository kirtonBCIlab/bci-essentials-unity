using System;
using BCIEssentials.StimulusObjects;

namespace BCIEssentials.Tests
{
    public class MockSPO : SPO
    {
        public Action TurnOnAction;
        public Action TurnOffAction;
        public Action OnSelectionAction;
        
        public override float TurnOn()
        {
            TurnOnAction?.Invoke();

            return 0;
        }
        
        public override void TurnOff()
        {
            TurnOffAction?.Invoke();
        }
        
        public override void OnSelection()
        {
            OnSelectionAction?.Invoke();
        }
    }
}