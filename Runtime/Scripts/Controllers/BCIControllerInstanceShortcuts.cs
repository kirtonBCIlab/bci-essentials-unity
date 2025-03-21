using UnityEngine;

namespace BCIEssentials.Controllers
{
    public class BCIControllerInstanceShortcuts: BCIControllerShortcuts
    {
        [Space]
        public BCIControllerInstance Target;

        private void OnValidate()
        => Target = GetComponent<BCIControllerInstance>();

        protected override void Update()
        {
            if (ToggleStimulusRunBinding.WasPressedThisFrame)
                Target.StartStopStimulus();

            if (StartAutomatedTrainingBinding.WasPressedThisFrame)
                Target.StartAutomatedTraining();
            if (StartUserTrainingBinding.WasPressedThisFrame)
                Target.StartUserTraining();
            if (StartIterativeTrainingBinding.WasPressedThisFrame)
                Target.StartIterativeTraining();
            if (StartSingleTrainingBinding.WasPressedThisFrame)
                Target.StartSingleTraining();
            
            if (UpdateClassifierBinding.WasPressedThisFrame)
                Target.UpdateClassifier();

            ObjectSelectionBindings.Process(Target.SelectSPOAtEndOfRun);
        }  
    }
}