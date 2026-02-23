using System.Collections;
using System.Collections.Generic;
using BCIEssentials.Extensions;
using BCIEssentials.Stimulus.Presentation;
using BCIEssentials.Utilities;

namespace BCIEssentials.Behaviours.Trials.P300
{
    public class CheckerboardFlashTrialBehaviour : GridFlashTrialBehaviour
    {
        protected override IEnumerator Run()
        {
            List<StimulusPresentationBehaviour> selectablePresenters = PresenterCollection.GetSelectable();

            IEnumerator RunMultiFlashOnSelectablePresenters(int[] stimulusIndices)
            => RunMultiFlash(stimulusIndices, selectablePresenters);

            BlackWhiteMatrixFactory bwMatrixFactory = new(EmptyGridMatrix);
            int[,] blackMatrix, whiteMatrix;

            for (int i = 0; i < FlashesPerOption; i++)
            {
                (blackMatrix, whiteMatrix) = bwMatrixFactory.CreateShuffledMatrices();

                yield return blackMatrix.RunForEachRow(RunMultiFlashOnSelectablePresenters);
                yield return whiteMatrix.RunForEachRow(RunMultiFlashOnSelectablePresenters);
                yield return blackMatrix.RunForEachColumn(RunMultiFlashOnSelectablePresenters);
                yield return whiteMatrix.RunForEachColumn(RunMultiFlashOnSelectablePresenters);
            }
        }
    }
}