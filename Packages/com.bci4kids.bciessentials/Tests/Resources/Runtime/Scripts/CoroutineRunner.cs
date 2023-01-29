using System;
using System.Collections;
using UnityEngine;

namespace BCIEssentials.Tests.TestResources
{
    public class CoroutineRunner : MonoBehaviour
    {
        public IEnumerator Routine;
        public Action OnCompleteEvent;

        public bool IsRunning => _runningRoutine != null;

        private Coroutine _runningRoutine;
        
        public void StartRun()
        {
            StopRun();
            _runningRoutine = StartCoroutine(DoRun());
        }

        public void StopRun()
        {
            if (!IsRunning) return;
            StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }

        private IEnumerator DoRun()
        {
            if (Routine != null)
            {
                yield return Routine;
            }

            OnCompleteEvent?.Invoke();
            StopRun();
        }
    }
}