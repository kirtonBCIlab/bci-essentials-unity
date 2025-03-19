using System;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Controllers
{
    public class BCIControllerShortcuts: MonoBehaviourUsingExtendedAttributes
    {
        public KeyBind ToggleStimulusRunBinding = KeyCode.S;

        [Space(8)]
        public KeyBind StartAutomatedTrainingBinding = KeyCode.T;
        public KeyBind StartIterativeTrainingBinding = KeyCode.I;
        public KeyBind StartUserTrainingBinding = KeyCode.U;
        public KeyBind StartSingleTrainingBinding = KeyCode.Semicolon;
        public KeyBind UpdateClassifierBinding = KeyCode.Backspace;

        [Space(12)]
        public IndexedKeyBindSet ObjectSelectionBindings
        = new IndexedKeyBindSet {
            {0, KeyCode.Alpha0}, {1, KeyCode.Alpha1},
            {2, KeyCode.Alpha2}, {3, KeyCode.Alpha3},
            {4, KeyCode.Alpha4}, {5, KeyCode.Alpha5},
            {6, KeyCode.Alpha6}, {7, KeyCode.Alpha7},
            {8, KeyCode.Alpha8}, {9, KeyCode.Alpha9},
        };


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
    public struct KeyBind
    {
        public KeyCode BoundKey;
        public KeyBind(KeyCode keyCode) { BoundKey = keyCode; }
        public static implicit operator KeyCode(KeyBind b) => b.BoundKey;
        public static implicit operator KeyBind(KeyCode k) => new(k);

        public bool WasPressedThisFrame
        => Input.GetKeyDown(BoundKey);
    }

    [Serializable]
    public class IndexedKeyBindSet: List<(int, KeyBind)>
    {
        public void Add(int index, KeyCode keyCode)
        => Add((index, keyCode));

        public void Process(Action<int> onPressed)
        {
            foreach ((int index, KeyBind binding) in this)
            {
                if (binding.WasPressedThisFrame)
                    onPressed(index);
            }
        }
    }
}