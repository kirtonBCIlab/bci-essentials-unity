namespace BCIEssentials.Behaviours.Trials.P300
{
    public abstract class GridFlashTrialBehaviour : MultiFlashTrialBehaviour
    {
        public int Rows = 3;
        public int Columns = 3;
        public int[,] EmptyGridMatrix => new int[Rows,Columns];
    }
}