using System.Collections;
using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    public class IterativeTrainingBehaviour : AutomatedTrainingBehaviour
    {
        [Header("Iterative Properties")]
        public int SelectionsBeforeTraining;
        public int SelectionsBetweenTraining;

        private int _selectionCounter;


        public override IEnumerator RunRound(int targetIndex)
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