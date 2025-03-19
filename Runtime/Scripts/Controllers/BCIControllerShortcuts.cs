using System;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.Controllers
{
    public class BCIControllerShortcuts: MonoBehaviourUsingExtendedAttributes
    {
        public KeyBind ToggleStimulusRunBinding = KeyCode.S;

        [FoldoutGroup("Start Training")]
        public KeyBind StartAutomatedTrainingBinding = KeyCode.T;
        public KeyBind StartIterativeTrainingBinding = KeyCode.I;
        public KeyBind StartUserTrainingBinding = KeyCode.U;
        public KeyBind StartSingleTrainingBinding = KeyCode.Semicolon;
        public KeyBind UpdateClassifierBinding = KeyCode.Backspace;

        [FoldoutGroup("Object Selection")]
        public IndexedKeyBind[] ObjectSelectionBindings
        = new IndexedKeyBind[] {
            new(KeyCode.Alpha0, 0), new(KeyCode.Alpha1, 1),
            new(KeyCode.Alpha2, 2), new(KeyCode.Alpha3, 3),
            new(KeyCode.Alpha4, 4), new(KeyCode.Alpha5, 5),
            new(KeyCode.Alpha6, 6), new(KeyCode.Alpha7, 7),
            new(KeyCode.Alpha8, 8), new(KeyCode.Alpha9, 9),
        };


        protected KeyboardShortcut[] BoundShortCuts;


        private void Start()
        {
            UpdateBoundShortcuts();
        }

        private void Update()
        {
            foreach(KeyboardShortcut shortcut in BoundShortCuts)
                shortcut.InvokeIfKeyPressed();
        }


        public void UpdateBoundShortcuts()
        {
            BoundShortCuts = GetStaticShortcuts();
            BoundShortCuts.Concat(GetObjectSelectionShortcuts());
        }

        protected KeyboardShortcut[] GetStaticShortcuts()
        => new KeyboardShortcut[]
        {
            new(ToggleStimulusRunBinding, BCIController.StartStopStimulus),
            new(StartAutomatedTrainingBinding, BCIController.StartAutomatedTraining),
            new(StartUserTrainingBinding, BCIController.StartUserTraining),
            new(StartIterativeTrainingBinding, BCIController.StartIterativeTraining),
            new(StartSingleTrainingBinding, BCIController.StartSingleTraining),
            new(UpdateClassifierBinding, BCIController.UpdateClassifier)
        };

        protected KeyboardShortcut[] GetObjectSelectionShortcuts()
        => (KeyboardShortcut[])ObjectSelectionBindings.Select(
            indexedBinding => new KeyboardShortcut(
                indexedBinding, BindSelectionMethod(indexedBinding)
            )
        );
        protected Action BindSelectionMethod(IndexedKeyBind binding)
        => () => BCIController.SelectSPOAtEndOfRun(binding.Index);        
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
        public IndexedKeyBind(KeyCode keyCode, int index)
        : base(keyCode) { Index = index; }
    }

    public class KeyboardShortcut
    {
        public KeyBind Binding;
        public Action Method;

        public KeyboardShortcut(KeyBind binding, Action method)
        {
            Binding = binding;
            Method = method;
        }

        public void InvokeIfKeyPressed()
        {
            if (Binding.WasPressedThisFrame) Method?.Invoke();
        }
    }
}