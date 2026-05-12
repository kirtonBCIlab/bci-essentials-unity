namespace BCIEssentials
{
    public interface ITargetIndicator
    {
        public int TargetCount { get; }

        public void BeginTargetIndication(int index);
        public void EndTargetIndication();
    }


    public interface ITargetable
    {
        public void StartTargetIndication();
        public void EndTargetIndication();
    }
}