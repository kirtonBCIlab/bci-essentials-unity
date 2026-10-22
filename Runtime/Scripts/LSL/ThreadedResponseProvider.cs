using System;
using System.Threading;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    using static LSLStreamResolver;

    /** <summary>
    Threaded variant of the ResponseProvider.
    Automatically polls for responses in the background
    process started and stopped silently by subscriptions.
    </summary> **/
    [Serializable]
    public class ThreadedResponseProvider: ResponseProvider
    {
        public bool IsPolling { get; private set; }
        protected Thread _pollingThread;

        public override void ProcessResponseProvision()
        => Debug.LogWarning("Don't manually update ThreadedResponseProvider");

        public override void CloseStream()
        {
            base.CloseStream();
            CleanUpAfterSubscribers();
        }

        protected override void PrepareForSubscribers()
        {
            if (IsPolling) return;
            CleanUpAfterSubscribers();
            base.PrepareForSubscribers();

            IsPolling = true;
            _pollingThread = new(PollForSamples);
            _pollingThread.Start();
        }

        protected override void CleanUpAfterSubscribers()
        {
            if (IsPolling)
            {
                IsPolling = false;
                _pollingThread.Join((int)(PollingPeriod * 1000));
                _pollingThread = null;
            }
        }

        protected void PollForSamples()
        {
            while (!IsConnected) SleepForSeconds(PollingPeriod);
            while (IsPolling)
            {
                if (IsConnected)
                {
                    PruneSubscriberList();
                    PullAllResponses();
                    SleepForSeconds(PollingPeriod);
                }
                else Reconnect();
            }
        }

        protected void Reconnect()
        {
            Debug.LogWarning("Response Provider Disconnected, attempting to reconnect...");
            FindAndOpenStream();
            while (!IsConnected)
            {
                if (!IsPolling) return;
                SleepForSeconds(PollingPeriod);
            }
            Debug.Log("Response Provider Reconnected");
        }
    }
}