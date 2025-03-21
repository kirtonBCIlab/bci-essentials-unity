using UnityEngine;

namespace BCIEssentials.Controllers
{
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
                    + ", shortcuts will not function."
                );
            }
        }

        protected override void Update()
        {
            if (Target == null)
            {
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