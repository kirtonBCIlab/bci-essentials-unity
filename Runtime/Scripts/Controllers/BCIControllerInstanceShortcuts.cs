using UnityEngine;

namespace BCIEssentials.Controllers
{
    /// <summary>
    /// Implements editable keyboard shortcuts for BCI Controller methods.
    /// <br/>Targets methods on a specified controller instance,
    /// defaulting to a component on the same object
    /// </summary>
    public class BCIControllerInstanceShortcuts: BCIControllerShortcuts
    {
        [Space]
        public BCIControllerInstance Target;

        private void OnValidate()
        {
            if (Target == null)
            {
                Target = GetComponent<BCIControllerInstance>();
            }
        }


        private void Start()
        {
            if (Target == null)
            {
                Debug.LogWarning(
                    "Controller Instance target is unset"
                    + ", shortcuts will target static controller."
                );
            }
        }

        protected override void Update()
        {
            if (Target == null)
            {
                base.Update();
                return;
            }

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