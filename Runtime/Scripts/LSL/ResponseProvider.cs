using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    using static LSLStreamResolver;

    /** <summary>
    A convenience class that pulls responses from an LSL
    outlet, providing them through typed callback subscriptions.
    </summary> **/
    [Serializable]
    public partial class ResponseProvider: LSLStreamReader
    {
        [Min(0)]
        public float PollingPeriod = 0.1f;

        private List<IResponseSubscriber> _subscribers = new();

        public bool IsPolling { get; private set; }
        private Thread _pollingThread;


        public override void CloseStream()
        {
            base.CloseStream();
            StopPolling();
        }


        public void SubscribePredictions(Action<Prediction> callback)
        => Subscribe(callback);
        public void SubscribeAll(Action<Response> callback)
        => Subscribe(callback);
        /** <summary>
        Add a method to the callback list,
        starting to poll if not already doing so.
        <br/>
        Will only send responses of the specified type.
        <br/><br/>
        Methods with invalidated targets will be automatically unsubscribed,
        enabling the use of lambdas or other anonymous delegates
        <example><code>
        provider.Subscribe((Prediction p) =>
            Debug.Log("Prediction Received: " + p.Value);
        );
        </code></example>
        </summary> **/
        public void Subscribe<T>(Action<T> callback)
        where T: Response
        {
            _subscribers.Add(new ResponseSubscriber<T>(callback));
            if (!IsPolling)
                StartPolling();
        }

        
        public bool UnsubscribePredictions(Action<Prediction> callback)
        => Unsubscribe(callback);
        public bool UnsubscribeAll(Action<Response> callback)
        => Unsubscribe(callback);
        /** <summary>
        Remove a method from the callback list,
        ceasing to poll if it is empty.
        <br/>
        Not necessary for cleanup, as any method with an
        invalidated target is automatically unsubscribed
        </summary> **/
        public bool Unsubscribe<T>(Action<T> callback)
        where T: Response
        {
            int subscribersRemoved = _subscribers.RemoveAll
            (
                subscriber => subscriber.MatchesCallback(callback)
            );

            if (_subscribers.Count == 0)
                StopPolling();
            
            return subscribersRemoved == 1;
        }


        private void StartPolling()
        {
            StopPolling();
            if (!IsConnected && !IsResolvingStream)
                FindAndOpenStream();

            IsPolling = true;
            _pollingThread = new(PollForSamples);
            _pollingThread.Start();
        }

        private void StopPolling()
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
                }
                else Reconnect();
                SleepForSeconds(PollingPeriod);
            }
        }

        protected void Reconnect()
        {
            Debug.LogWarning("Response Provider Disconnected, attempting to reconnect...");
            FindAndOpenStream();
            while (!IsConnected) SleepForSeconds(PollingPeriod);
            Debug.Log("Response Provider Reconnected");
        }

        public override Response[] PullAllResponses(int maxSamples = 50)
        {
            Response[] pulledResponses = base.PullAllResponses(maxSamples);
            Array.ForEach(pulledResponses, NotifySubscribers);
            return pulledResponses;
        }

        protected void NotifySubscribers(Response response)
        => _subscribers.ToList().ForEach
        (
            subscriber => subscriber.Notify(response)
        );

        protected int PruneSubscriberList()
        => _subscribers.RemoveAll
        (
            subscriber => !subscriber.HasValidCallbackTarget()
        );
    }
}