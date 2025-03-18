using System;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.Controllers
{
    public class BCIControllerShortcuts: MonoBehaviourUsingExtendedAttributes
    {
        public KeyCode ToggleStimulusRunBinding = KeyCode.S;

        [FoldoutGroup("Start Training")]
        public KeyCode StartAutomatedTrainingBinding = KeyCode.T;
        public KeyCode StartIterativeTrainingBinding = KeyCode.I;
        public KeyCode StartUserTrainingBinding = KeyCode.U;
        public KeyCode StartSingleTrainingBinding = KeyCode.Semicolon;
        public KeyCode UpdateClassifierBinding = KeyCode.Backspace;

        [FoldoutGroup("Object Selection")]
        public IndexedKeyBind[] ObjectSelectionBindings
        = new IndexedKeyBind[] {
            new(KeyCode.Alpha0, 0), new(KeyCode.Alpha1, 1),
            new(KeyCode.Alpha2, 2), new(KeyCode.Alpha3, 3),
            new(KeyCode.Alpha4, 4), new(KeyCode.Alpha5, 5),
            new(KeyCode.Alpha6, 6), new(KeyCode.Alpha7, 7),
            new(KeyCode.Alpha8, 8), new(KeyCode.Alpha9, 9),
        };


        protected KeycodeShortcut[] BoundShortCuts;


        private void Start()
        {
            UpdateBoundShortcuts();
        }

        private void Update()
        {
            foreach(KeycodeShortcut shortcut in BoundShortCuts)
                shortcut.InvokeIfKeyPressed();
        }


        public void UpdateBoundShortcuts()
        {
            BoundShortCuts = GetStaticShortcuts();
            BoundShortCuts.Concat(GetObjectSelectionShortcuts());
        }

        protected KeycodeShortcut[] GetStaticShortcuts()
        => new KeycodeShortcut[]
        {
            new(ToggleStimulusRunBinding, BCIController.StartStopStimulus),
            new(StartAutomatedTrainingBinding, BCIController.StartAutomatedTraining),
            new(StartUserTrainingBinding, BCIController.StartUserTraining),
            new(StartIterativeTrainingBinding, BCIController.StartIterativeTraining),
            new(StartSingleTrainingBinding, BCIController.StartSingleTraining),
            new(UpdateClassifierBinding, BCIController.UpdateClassifier)
        };

        protected KeycodeShortcut[] GetObjectSelectionShortcuts()
        => (KeycodeShortcut[])ObjectSelectionBindings.Select(
            indexedBinding => new KeycodeShortcut(
                indexedBinding.Binding,
                () => BCIController.SelectSPOAtEndOfRun(indexedBinding.Index)
            )
        );


        [Serializable]
        public struct IndexedKeyBind
        {
            public int Index;
            public KeyCode Binding;

            public IndexedKeyBind(KeyCode shortcut, int index)
            {
                Binding = shortcut;
                Index = index;
            }
        }

        public struct KeycodeShortcut
        {
            public KeyCode Binding;
            public Action Method;

            public KeycodeShortcut(KeyCode binding, Action method)
            {
                Binding = binding;
                Method = method;
            }

            public readonly void InvokeIfKeyPressed()
            {
                if (Input.GetKeyDown(Binding)) Method?.Invoke();
            }
        }
    }
}