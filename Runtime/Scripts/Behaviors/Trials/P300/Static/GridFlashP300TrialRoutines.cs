using System.Collections;
using System.Collections.Generic;

namespace BCIEssentials
{
    using Extensions;
    using LSLFramework;
    using Stimulus.Presentation;
    using Utilities;

    public static partial class P300TrialRoutines
    {
        public static IEnumerator RunRowColumnFlashRoutine
        (
            MarkerWriter markerWriter,
            int rows, int columns, int flashesPerOption,
            float onTime, float offTime,
            List<IStimulusPresenter> stimulusPresenters,
            int? trainingTarget = null
        )
        {
            int[,] gridMatrix = new int[rows, columns];
            int gridIndex = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    gridMatrix[i, j] = gridIndex++;
                }
            }

            int totalColumnFlashes = flashesPerOption * columns;
            int totalRowFlashes = flashesPerOption * rows;

            int[] columnStimulusOrder = RNRAUtilities.GenerateRNRA_FisherYates
            (totalColumnFlashes, 0, columns - 1);
            int[] rowStimulusOrder = RNRAUtilities.GenerateRNRA_FisherYates
            (totalRowFlashes, 0, rows - 1);

            for (int i = 0; i < totalColumnFlashes; i++)
            {
                int columnIndex = columnStimulusOrder[i];
                int[] column = gridMatrix.GetColumn(columnIndex);

                yield return RunMultiFlash(
                    stimulusPresenters, column, markerWriter,
                    onTime, offTime, trainingTarget
                );

                if (i < totalRowFlashes)
                {
                    int rowIndex = rowStimulusOrder[i];
                    int[] row = gridMatrix.GetRow(rowIndex);

                    yield return RunMultiFlash(
                        stimulusPresenters, row, markerWriter,
                        onTime, offTime, trainingTarget
                    );
                }
            }
        }


        public static IEnumerator RunCheckerboardFlashRoutine<T>
        (
            MarkerWriter markerWriter,
            int rows, int columns, int flashesPerOption,
            float onTime, float offTime,
            List<IStimulusPresenter> stimulusPresenters,
            int? trainingTarget = null
        )
        {
            IEnumerator RunBoundMultiFlash(int[] stimulusIndices)
            => RunMultiFlash(
                stimulusPresenters, stimulusIndices,
                markerWriter, onTime, offTime, trainingTarget
            );

            BlackWhiteMatrixFactory bwMatrixFactory = new(rows, columns);
            int[,] blackMatrix, whiteMatrix;

            for (int i = 0; i < flashesPerOption; i++)
            {
                (blackMatrix, whiteMatrix) = bwMatrixFactory.CreateShuffledMatrices();

                yield return blackMatrix.RunForEachRow(RunBoundMultiFlash);
                yield return whiteMatrix.RunForEachRow(RunBoundMultiFlash);
                yield return blackMatrix.RunForEachColumn(RunBoundMultiFlash);
                yield return whiteMatrix.RunForEachColumn(RunBoundMultiFlash);
            }
        }
    }
}