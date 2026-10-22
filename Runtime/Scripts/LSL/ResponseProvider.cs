using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    /** <summary>
    Convenience class to pull responses from an LSL
    outlet, providing them through typed callback
    subscriptions.
    Process method must be invoked regularly.
    </summary> **/
    [Serializable]
    public partial class ResponseProvider : LSLStreamReader
    {
        [Min(0)]
        public float PollingPeriod = 0.1f;
        protected float _lastPullTime = 0;

        protected readonly List<IResponseSubscriber> _subscribers = new();

        /** <summary>
        Pulls responses from internal inlet, distributing
        them to subscribers. Also handles connection.
        <br/>
        Use in Update()
        </summary> **/
        public virtual void ProcessResponseProvision()
        {
            if (Time.time < _lastPullTime + PollingPeriod) return;

            if (IsConnected)
            {
                PruneSubscriberList();
                PullAllResponses();
                _lastPullTime = Time.time;
            }
            else if (!IsResolvingStream)
            {
                FindAndOpenStream();
            }
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
            if (_subscribers.Count == 0) PrepareForSubscribers();
            _subscribers.Add(new ResponseSubscriber<T>(callback));
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

            CheckForSubscriberCleanup();
            return subscribersRemoved == 1;
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
        {
            int subscribersRemoved = _subscribers.RemoveAll
            (
                subscriber => !subscriber.HasValidCallbackTarget()
            );
            CheckForSubscriberCleanup();
            return subscribersRemoved;
        }

        protected virtual void PrepareForSubscribers()
        {
            if (!IsConnected && !IsResolvingStream)
            {
                FindAndOpenStream();
            }
        }

        protected void CheckForSubscriberCleanup()
        {
            if (_subscribers.Count > 0) return;
            CleanUpAfterSubscribers();
        }
        protected virtual void CleanUpAfterSubscribers() { }
    }
}