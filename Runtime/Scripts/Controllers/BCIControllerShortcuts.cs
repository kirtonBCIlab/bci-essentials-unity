using System;
using UnityEngine;

namespace BCIEssentials.Controllers
{
    /// <summary>
    /// Implements editable keyboard shortcuts for BCI Controller methods.
    /// <br/>Targets methods on a specified controller instance,
    /// otherwise defaults to static methods which use the first
    /// living controller instance or one created on demand.
    /// </summary>
    public class BCIControllerShortcuts: MonoBehaviourUsingExtendedAttributes
    {
        public KeyBind ToggleStimulusRunBinding;

        [StartFoldoutGroup("Training", 12, 8, 8)]
        [InspectorName("Start Automated")]
        public KeyBind StartAutomatedTrainingBinding;
        [InspectorName("Start Iterative")]
        public KeyBind StartIterativeTrainingBinding;
        [InspectorName("Start User")]
        public KeyBind StartUserTrainingBinding;
        [InspectorName("Start Single")]
        public KeyBind StartSingleTrainingBinding;
        [EndFoldoutGroup, Space(6)]
        public KeyBind UpdateClassifierBinding;

        public IndexedKeyBindSet SelectionBindings;
        
        [Space]
        public BCIControllerInstance Target;


        private void Reset()
        {
            ToggleStimulusRunBinding = KeyCode.S;
            StartAutomatedTrainingBinding = KeyCode.T;
            StartIterativeTrainingBinding = KeyCode.I;
            StartUserTrainingBinding = KeyCode.U;
            StartSingleTrainingBinding = KeyCode.Semicolon;
            UpdateClassifierBinding = KeyCode.Backspace;

            SelectionBindings = new IndexedKeyBindSet
            (
                (0, KeyCode.Alpha0), (1, KeyCode.Alpha1),
                (2, KeyCode.Alpha2), (3, KeyCode.Alpha3),
                (4, KeyCode.Alpha4), (5, KeyCode.Alpha5),
                (6, KeyCode.Alpha6), (7, KeyCode.Alpha7),
                (8, KeyCode.Alpha8), (9, KeyCode.Alpha9)
            );

            if (Target == null)
                Target = GetComponent<BCIControllerInstance>();
        }

        protected virtual void Update()
        {
            if (Target != null)
            {
                ProcessShortcuts(
                    new (KeyBind, Action)[] {
                        (ToggleStimulusRunBinding, Target.StartStopStimulus),
                        (StartAutomatedTrainingBinding, Target.StartAutomatedTraining),
                        (StartUserTrainingBinding, Target.StartUserTraining),
                        (StartIterativeTrainingBinding, Target.StartIterativeTraining),
                        (StartSingleTrainingBinding, Target.StartSingleTraining),
                        (UpdateClassifierBinding, Target.UpdateClassifier)
                    },
                    Target.MakeSelectionAtEndOfRun
                );
            }
            else
            {
                ProcessShortcuts(
                    new (KeyBind, Action)[] {
                        (ToggleStimulusRunBinding, BCIController.StartStopStimulus),
                        (StartAutomatedTrainingBinding, BCIController.StartAutomatedTraining),
                        (StartUserTrainingBinding, BCIController.StartUserTraining),
                        (StartIterativeTrainingBinding, BCIController.StartIterativeTraining),
                        (StartSingleTrainingBinding, BCIController.StartSingleTraining),
                        (UpdateClassifierBinding, BCIController.UpdateClassifier)
                    },
                    BCIController.MakeSelectionAtEndOfRun
                );
            }
        }

        private void ProcessShortcuts(
            (KeyBind, Action)[] shortcuts,
            Action<int> selectionMethod
        )
        {
            foreach ((KeyBind keyBind, Action method) in shortcuts)
                if (keyBind.WasPressedThisFrame) method();

            SelectionBindings.Process(selectionMethod);
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