using System;
using System.Collections;
using UnityEngine;

namespace BCIEssentials.Tests
{
    public class CoroutineRunner : MonoBehaviour
    {
        public IEnumerator Routine;
        public Action OnCompleteEvent;
        
        public bool IsRunning { get; private set; }

        public void StartRun()
        {
            StopRun();
            StartCoroutine(DoRun());
        }

        public void StopRun()
        {
            StopAllCoroutines();
        }

        private IEnumerator DoRun()
        {
            IsRunning = true;
            if (Routine != null)
            {
                yield return Routine;
            }

            IsRunning = false;
            OnCompleteEvent?.Invoke();
        }
    }
}