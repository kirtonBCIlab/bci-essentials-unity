using BCIEssentials.Selection;
using BCIEssentials.Behaviours.Training;

namespace BCIEssentials.Stimulus.Presentation
{
    public abstract class StimulusPresentationBehaviour : MonoBehaviourUsingExtendedAttributes, ISelectable, ITargetable
    {
        public virtual bool IsSelectable => enabled;

        public abstract void StartStimulusDisplay();
        public abstract void EndStimulusDisplay();

        public abstract void Select();

        public abstract void StartTargetIndication();
        public abstract void EndTargetIndication();
    }
}