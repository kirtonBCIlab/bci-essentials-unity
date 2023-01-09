using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;

namespace BCIEssentials.Tests
{
    public class MockBCIControllerBehavior : BCIControllerBehavior
    {
        public override BehaviorType BehaviorType => BehaviorType.Unset;
    }
}