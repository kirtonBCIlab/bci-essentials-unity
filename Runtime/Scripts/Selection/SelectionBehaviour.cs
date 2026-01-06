namespace BCIEssentials.Selection
{
    public abstract class SelectionBehaviour : MonoBehaviourUsingExtendedAttributes, ISelector
    {
        public abstract void MakeSelection(int index);
    }

    public interface ISelector
    {
        public void MakeSelection(int index);
    }
}