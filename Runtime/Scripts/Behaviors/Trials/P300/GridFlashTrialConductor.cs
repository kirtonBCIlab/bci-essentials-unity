using UnityEngine;

namespace BCIEssentials
{
    public abstract class GridFlashTrialConductor : MultiFlashTrialConductor
    {
        public int Rows = 3;
        public int Columns = 3;
        public int[,] EmptyGridMatrix => new int[Rows, Columns];

        public GridFlashTrialConductor(MonoBehaviour executionHost) : base(executionHost) { }
    }
}