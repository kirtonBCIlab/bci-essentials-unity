using System.Collections;
using UnityEngine;

namespace BCIEssentials.Behaviours
{
    public abstract class CoroutineBehaviour : MonoBehaviourUsingExtendedAttributes
    {
        public bool IsRunning { get; private set; }
        private Coroutine _routine;


        public void Begin()
        {
            if (IsRunning)
            {
                Debug.LogWarning("Ignored attempt to start sequence a it is already running.");
                return;
            }

            StartCoroutine(RunWrapper());
        }

        public void Interrupt()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("Can't interrupt a sequence that isn't running.");
                return;
            }

            StopCoroutine(_routine);
            IsRunning = false;
        }

        protected virtual void SetUp() { }
        protected virtual void CleanUp() { }
        protected abstract IEnumerator Run();


        public IEnumerator AwaitCompletion()
        {
            while (IsRunning) yield return null;
        }


        private IEnumerator RunWrapper()
        {
            SetUp();
            _routine = StartCoroutine(Run());

            IsRunning = true;
            yield return AwaitCompletion();
            IsRunning = false;

            CleanUp();
        }
    }
}