using BCIEssentials.Selection;
using BCIEssentials.Training;

namespace BCIEssentials.Stimulus.Presentation
{
    public interface IStimulusPresenter : ISelectable, ITargetable
    {
        public void TriggerStimulusDisplay();
        public void EndStimulusDisplay();
    }
}