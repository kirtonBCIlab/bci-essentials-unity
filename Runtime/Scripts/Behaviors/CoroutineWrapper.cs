using System.Collections;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public abstract class CoroutineWrapper : MonoBehaviourUsingExtendedAttributes
    {
        public bool IsRunning => _routine != null;
        private Coroutine _routine;


        public void Begin()
        {
            if (IsRunning)
            {
                Debug.LogWarning("Ignored attempt to start sequence a it is already running.");
                return;
            }

            SetUp();
            _routine = StartCoroutine(Run());
            CleanUp();
        }

        public void Interrupt()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("Can't interrupt a sequence that isn't running.");
                return;
            }

            StopCoroutine(_routine);
            CleanUp();
        }

        protected virtual void SetUp() { }
        protected virtual void CleanUp() { }

        protected abstract IEnumerator Run();


        public IEnumerator AwaitCompletion()
        {
            while (IsRunning) yield return null;
        }
    }
}