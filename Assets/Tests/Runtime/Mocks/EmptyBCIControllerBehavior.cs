using System.Collections;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;

namespace BCIEssentials.Tests
{
    public class EmptyBCIControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.Unset;
    }
}