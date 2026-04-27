using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials
{
    using Extensions;
    using Stimulus.Presentation;
    using Utilities;

    [System.Serializable]
    public class CheckerboardFlashTrialConductor : GridFlashTrialConductor
    {
        public CheckerboardFlashTrialConductor(MonoBehaviour executionHost) : base(executionHost) { }

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