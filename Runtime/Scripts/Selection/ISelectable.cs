namespace BCIEssentials.Selection
{
    public interface ISelectable
    {
        public bool IsSelectable { get; }

        public void Select();
    }
}