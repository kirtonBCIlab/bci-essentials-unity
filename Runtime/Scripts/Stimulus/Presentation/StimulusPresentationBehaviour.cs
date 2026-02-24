namespace BCIEssentials.Stimulus.Presentation
{
    using Selection;
    using Behaviours.Training;

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