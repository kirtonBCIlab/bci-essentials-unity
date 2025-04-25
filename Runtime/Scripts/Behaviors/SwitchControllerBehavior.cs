using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// Adds Switch functionality to <see cref="BCIControllerBehavior"/>
    /// </summary>
    public class SwitchControllerBehavior : MIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.Switch;

        protected override void SendTrainingMarker(int trainingIndex)
        => MarkerWriter.PushSwitchTrainingMarker(SPOCount, trainingIndex, epochLength);

        protected override void SendClassificationMarker()
        => MarkerWriter.PushSwitchClassificationMarker(SPOCount, epochLength);
    }
}