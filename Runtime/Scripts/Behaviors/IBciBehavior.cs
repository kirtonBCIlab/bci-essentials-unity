using BCIEssentials.Controllers;
using BCIEssentials.LSLFramework;

namespace BCIEssentials.ControllerBehaviors
{
    public interface IBciBehavior
    {
        public BCIBehaviorType Type { get; }
        
        public void Initialize(LSLMarkerStreamWriter markerStream, LSLResponseProvider responseStream);

        public void CleanUp();

        public void StartStopStimulus();
        
        public void StimulusOn(bool sendConstantMarkers);

        public void StimulusOff();

        public void SelectObject(int objectIndex);

        public void StartUserTraining();
        
        public void StartAutomatedTraining();

        public void StartIterativeTraining();

        public void StartSingleTraining();
    }
}