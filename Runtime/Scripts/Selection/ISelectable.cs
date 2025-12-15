namespace BCIEssentials.Selection
{
    public interface ISelectable
    {
        public bool IsSelectable { get; }

        public void Select();

        public void StartTargetIndication();
        public void EndTargetIndication();
    }
}