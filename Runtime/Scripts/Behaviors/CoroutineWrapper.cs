using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    public abstract class CoroutineWrapper
    {
        public bool IsRunning { get; private set; }
        protected readonly MonoBehaviour _executionHost;
        private Coroutine _routine;

        public CoroutineWrapper(MonoBehaviour executionHost)
        => _executionHost = executionHost;


        public void Begin()
        {
            if (IsRunning)
            {
                Debug.LogWarning("Ignored attempt to start sequence a it is already running.");
                return;
            }

            _executionHost.StartCoroutine(RunWrapper());
        }

        public void Interrupt()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("Can't interrupt a sequence that isn't running.");
                return;
            }

            _executionHost.StopCoroutine(_routine);
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
            _routine = _executionHost.StartCoroutine(RunWithTrackedStatus());
            yield return AwaitCompletion();
            CleanUp();
        }

        private IEnumerator RunWithTrackedStatus()
        {
            IsRunning = true;
            yield return Run();
            IsRunning = false;
        }
    }
}