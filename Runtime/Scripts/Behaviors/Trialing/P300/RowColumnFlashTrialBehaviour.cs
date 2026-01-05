using System.Collections;
using System.Collections.Generic;
using BCIEssentials.Extensions;
using BCIEssentials.Stimulus.Presentation;
using BCIEssentials.Utilities;

namespace BCIEssentials.Behaviours.Trialing.P300
{
    public class RowColumnFlashBehaviour : GridFlashTrialBehaviour
    {
        protected override IEnumerator Run()
        {
            int[,] gridMatrix = EmptyGridMatrix;
            int counter = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    gridMatrix[i, j] = counter++;
                }
            }

            int totalColumnFlashes = FlashesPerOption * Columns;
            int totalRowFlashes = FlashesPerOption * Rows;

            int[] columnStimulusOrder = ArrayUtilities.GenerateRNRA_FisherYates
            (totalColumnFlashes, 0, Columns - 1);
            int[] rowStimulusOrder = ArrayUtilities.GenerateRNRA_FisherYates
            (totalRowFlashes, 0, Rows - 1);

            List<IStimulusPresenter> selectablePresenters = PresenterCollection.GetSelectable();

            for (int i = 0; i < totalColumnFlashes; i++)
            {
                int columnIndex = columnStimulusOrder[i];
                int[] column = gridMatrix.GetColumn(columnIndex);
                yield return RunMultiFlash(column, selectablePresenters);

                if (i <= totalRowFlashes)
                {
                    int rowIndex = rowStimulusOrder[i];
                    int[] row = gridMatrix.GetRow(rowIndex);
                    yield return RunMultiFlash(row, selectablePresenters);
                }
            }
        }
    }
}