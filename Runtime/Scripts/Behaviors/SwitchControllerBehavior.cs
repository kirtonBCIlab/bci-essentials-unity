using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// Adds Switch functionality to <see cref="BCIControllerBehavior"/>
    /// </summary>
    public class SwitchControllerBehavior : MIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.Switch;

        protected override void SendWindowMarker(int trainingIndex = -1)
        => OutStream.PushSwitchMarker(SPOCount, windowLength, trainingIndex);
    }
}