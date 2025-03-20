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
                (0, KeyCode.Alpha0), (1, KeyCode.Alpha1),
                (2, KeyCode.Alpha2), (3, KeyCode.Alpha3),
                (4, KeyCode.Alpha4), (5, KeyCode.Alpha5),
                (6, KeyCode.Alpha6), (7, KeyCode.Alpha7),
                (8, KeyCode.Alpha8), (9, KeyCode.Alpha9)
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

        public IndexedKeyBind this[int index]
        {
            get => Bindings[index];
            set => Bindings[index] = value;
        }

        public IndexedKeyBindSet
        (params (int, KeyCode)[] tuples)
        {
            int count = tuples.Length;
            Bindings = new IndexedKeyBind[count];
            for (int i = 0; i < count; i++)
            {
                (int index, KeyCode keyCode) = tuples[i];
                Bindings[i] = new(index, keyCode);
            }
        }

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