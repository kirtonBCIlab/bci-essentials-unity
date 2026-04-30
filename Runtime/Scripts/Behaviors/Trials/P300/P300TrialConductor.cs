using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;
    using Stimulus.Collections;
    using Stimulus.Presentation;

    [Serializable]
    public class P300TrialConductor : TrialConductor
    {
        public enum FlashingPattern
        {
            Random, ContextAware,
            RowColumn, Checkerboard, ContextAwareGroups
        }
        public FlashingPattern pattern;
        public int FlashesPerOption = 8;
        public float OnTime = 0.1f;
        public float OffTime = 0.075f;

        [ShowIf(nameof(pattern), (int)FlashingPattern.RowColumn, (int)FlashingPattern.Checkerboard)]
        public int Rows, Columns;

        [Space]
        public DynamicStimulusPresenterCollection PresenterCollection;

        protected override IEnumerator Run()
        => pattern switch
        {
            FlashingPattern.Random
            => StartFlashRoutine(P300TrialRoutines.RunSingleFlashTrialRoutine),
            FlashingPattern.ContextAware
            => StartFlashRoutine(P300TrialRoutines.RunContextAwareSingleFlashRoutine),

            FlashingPattern.RowColumn
            => StartGridFlashRoutine(P300TrialRoutines.RunRowColumnFlashRoutine),
            FlashingPattern.Checkerboard
            => StartGridFlashRoutine(P300TrialRoutines.RunCheckerboardFlashRoutine),
            FlashingPattern.ContextAwareGroups
            => StartFlashRoutine(P300TrialRoutines.RunContextAwareMultiFlashRoutine),

            _ => throw new NotImplementedException()
        };


        private IEnumerator StartFlashRoutine
        (
            Func<
                MarkerWriter, int, float, float,
                List<IStimulusPresenter>, int?, IEnumerator
            > routineMethod
        )
        => routineMethod(
            MarkerWriter,
            FlashesPerOption, OnTime, OffTime,
            GetSelectablePresenters(), TrainingTarget
        );

        private IEnumerator StartGridFlashRoutine
        (
            Func<
                MarkerWriter, int, int, int, float, float,
                List<IStimulusPresenter>, int?, IEnumerator
            > routineMethod
        )
        => routineMethod(
            MarkerWriter, Rows, Columns,
            FlashesPerOption, OnTime, OffTime,
            GetSelectablePresenters(), TrainingTarget
        );


        protected List<IStimulusPresenter> GetSelectablePresenters()
        => PresenterCollection.GetSelectable().Cast<IStimulusPresenter>().ToList();
    }
}