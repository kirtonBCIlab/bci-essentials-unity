using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    public class HostedCoroutine
    {
        public bool IsRunning { get; private set; }
        protected readonly Coroutine _routine;
        protected readonly MonoBehaviour _executionHost;

        public HostedCoroutine(MonoBehaviour host, IEnumerator routineEnumerator)
        {
            IsRunning = true;
            _executionHost = host;
            _routine = _executionHost.StartCoroutine(RunWithTrackedStatus(routineEnumerator));
        }


        public void Interrupt()
        {
            if (!IsRunning) return;

            _executionHost.StopCoroutine(_routine);
            IsRunning = false;
        }

        public IEnumerator AwaitCompletion()
        {
            while (IsRunning) yield return null;
        }


        protected IEnumerator RunWithTrackedStatus(IEnumerator target)
        {
            yield return target;
            IsRunning = false;
        }
    }
}