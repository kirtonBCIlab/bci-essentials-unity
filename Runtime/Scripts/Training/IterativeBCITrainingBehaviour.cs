using System.Collections;

namespace BCIEssentials.Training
{
    public class IterativeBCITrainingBehaviour : AutomatedBCITrainingBehaviour
    {
        public int SelectionsBeforeTraining;
        public int SelectionsBetweenTraining;

        private int _selectionCounter;


        protected override IEnumerator RunRound(int targetIndex)
        {
            int iterativeSelectionCount = _selectionCounter - SelectionsBeforeTraining;
            int iterativeSelectionIndex = iterativeSelectionCount % SelectionsBetweenTraining;

            if (iterativeSelectionCount >= 0 && iterativeSelectionIndex == 0)
            {
                MarkerWriter.PushUpdateClassifierMarker();
            }

            yield return base.RunRound(targetIndex);
            _selectionCounter++;
        }
    }
}