namespace BCIEssentials.Stimulus.Presentation
{
    public interface IStimulusPresenter
    {
        public void StartStimulusDisplay();
        public void EndStimulusDisplay();
    }

    public interface ISelectable
    {
        public bool IsSelectable { get; }

        public void Select();
    }

    public abstract class StimulusPresentationBehaviour : MonoBehaviourUsingExtendedAttributes, IStimulusPresenter, ISelectable, ITargetable
    {
        public virtual bool IsSelectable => enabled;

        public abstract void StartStimulusDisplay();
        public abstract void EndStimulusDisplay();

        public abstract void Select();

        public abstract void StartTargetIndication();
        public abstract void EndTargetIndication();
    }
}