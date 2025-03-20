using System;
using UnityEngine;

namespace BCIEssentials.Controllers
{
    public class BCIControllerShortcuts: MonoBehaviourUsingExtendedAttributes
    {
        public KeyBind ToggleStimulusRunBinding;

        [Space(8)]
        public KeyBind StartAutomatedTrainingBinding;
        public KeyBind StartIterativeTrainingBinding;
        public KeyBind StartUserTrainingBinding;
        public KeyBind StartSingleTrainingBinding;
        public KeyBind UpdateClassifierBinding;

        [Space(12)]
        public IndexedKeyBindSet ObjectSelectionBindings;


        private void Reset()
        {
            ToggleStimulusRunBinding = KeyCode.S;
            StartAutomatedTrainingBinding = KeyCode.T;
            StartIterativeTrainingBinding = KeyCode.I;
            StartUserTrainingBinding = KeyCode.U;
            StartSingleTrainingBinding = KeyCode.Semicolon;
            UpdateClassifierBinding = KeyCode.Backspace;

            ObjectSelectionBindings = new IndexedKeyBindSet
            (
                new(0, KeyCode.Alpha0), new(1, KeyCode.Alpha1),
                new(2, KeyCode.Alpha2), new(3, KeyCode.Alpha3),
                new(4, KeyCode.Alpha4), new(5, KeyCode.Alpha5),
                new(6, KeyCode.Alpha6), new(7, KeyCode.Alpha7),
                new(8, KeyCode.Alpha8), new(9, KeyCode.Alpha9)
            );
        }

        private void Update()
        {
            if (ToggleStimulusRunBinding.WasPressedThisFrame)
                BCIController.StartStopStimulus();

            if (StartAutomatedTrainingBinding.WasPressedThisFrame)
                BCIController.StartAutomatedTraining();
            if (StartUserTrainingBinding.WasPressedThisFrame)
                BCIController.StartUserTraining();
            if (StartIterativeTrainingBinding.WasPressedThisFrame)
                BCIController.StartIterativeTraining();
            if (StartSingleTrainingBinding.WasPressedThisFrame)
                BCIController.StartSingleTraining();
            
            if (UpdateClassifierBinding.WasPressedThisFrame)
                BCIController.UpdateClassifier();

            ObjectSelectionBindings.Process(BCIController.SelectSPOAtEndOfRun);
        }       
    }


    [Serializable]
    public class KeyBind
    {
        public KeyCode BoundKey;
        public KeyBind(KeyCode keyCode) { BoundKey = keyCode; }
        public static implicit operator KeyCode(KeyBind b) => b.BoundKey;
        public static implicit operator KeyBind(KeyCode k) => new(k);

        public bool WasPressedThisFrame
        => Input.GetKeyDown(BoundKey);
    }

    [Serializable]
    public class IndexedKeyBind: KeyBind
    {
        public int Index;
        public IndexedKeyBind(int index, KeyCode keyCode)
        : base(keyCode) => Index = index;
    }

    [Serializable]
    public class IndexedKeyBindSet
    {
        public IndexedKeyBind[] Bindings;

        public IndexedKeyBindSet
        (params IndexedKeyBind[] bindings)
        => Bindings = bindings;

        public void Process(Action<int> onPressed)
        {
            foreach (IndexedKeyBind binding in Bindings)
            {
                if (binding.WasPressedThisFrame)
                    onPressed(binding.Index);
            }
        }
    }
}