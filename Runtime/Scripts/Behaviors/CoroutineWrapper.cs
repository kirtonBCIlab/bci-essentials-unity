using System;
using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    public abstract class CoroutineWrapper
    {
        public bool IsRunning => _routine?.IsRunning == true;
        private HostedCoroutine _routine;

        protected MonoBehaviour _lastExecutionHost;


        public void Begin(MonoBehaviour executionHost)
        {
            if (IsRunning)
            {
                Debug.LogWarning("Ignored attempt to start sequence a it is already running.");
                return;
            }

            _lastExecutionHost = executionHost;
            SetUp();
            _routine = new(executionHost, Run());
            executionHost.StartCoroutine(RunCompletionCallback(CleanUp));
        }

        public void Interrupt()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("Can't interrupt a sequence that isn't running.");
                return;
            }

            _routine.Interrupt();
        }

        protected virtual void SetUp() { }
        protected virtual void CleanUp() { }
        protected abstract IEnumerator Run();


        public IEnumerator AwaitCompletion()
        {
            while (IsRunning) yield return null;
        }

        private IEnumerator RunCompletionCallback(Action callback)
        {
            yield return AwaitCompletion();
            callback();
        }
    }
}