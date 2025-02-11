using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    public class LSLResponseProvider: LSLStreamReader
    {
        [Min(0)]
        public float PollingPeriod = 0.1f;

        private List<IResponseSubscriber> _subscribers = new();
        
        public bool IsPolling => _pollingCoroutine is not null;
        private Coroutine _pollingCoroutine;


        public override void CloseStream()
        {
            base.CloseStream();
            StopPolling();
        }


        public void SubscribePredictions(Action<Prediction> callback)
        => Subscribe(callback);
        public void SubscribeAll(Action<LSLResponse> callback)
        => Subscribe(callback);
        public void Subscribe<T>(Action<T> callback)
        where T: LSLResponse
        {
            _subscribers.Add(new ResponseSubscriber<T>(callback));
            if (!IsPolling)
                StartPolling();
        }

        
        public bool UnsubscribePredictions(Action<Prediction> callback)
        => Unsubscribe(callback);
        public bool UnsubscribeAll(Action<LSLResponse> callback)
        => Unsubscribe(callback);
        public bool Unsubscribe<T>(Action<T> callback)
        where T: LSLResponse
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
            if (!HasLiveInlet && !IsResolvingStream)
                OpenStream();

            _pollingCoroutine = StartCoroutine(RunPollForSamples());
        }

        private void StopPolling()
        {
            if (IsPolling)
            {
                StopCoroutine(_pollingCoroutine);
                _pollingCoroutine = null;
            }
        }


        protected IEnumerator RunPollForSamples()
        {
            while(true)
            {
                if (HasLiveInlet)
                {
                    PruneSubscriberList();
                    Array.ForEach(PullAllResponses(), NotifySubscribers);
                }
                yield return new WaitForSeconds(PollingPeriod);
            }
        }


        protected void NotifySubscribers(LSLResponse response)
        => _subscribers.ToList().ForEach
        (
            subscriber => subscriber.Notify(response)
        );

        protected int PruneSubscriberList()
        => _subscribers.RemoveAll
        (
            subscriber => !subscriber.HasValidCallbackTarget()
        );


        protected interface IResponseSubscriber
        {
            public void Notify<T>(T Response);

            public bool MatchesCallback<T>(Action<T> callback);

            public bool HasValidCallbackTarget();
        }

        protected struct ResponseSubscriber<TResponse>: IResponseSubscriber
        where TResponse: LSLResponse
        {
            public Action<TResponse> responseCallback;
            private bool _callbackIsStatic;

            public ResponseSubscriber(Action<TResponse> callback)
            {
                responseCallback = callback;
                _callbackIsStatic = callback.Target is null;
            }

            public void Notify<T>(T response)
            {
                if (response is TResponse typedResponse)
                    responseCallback(typedResponse);
            }

            public bool MatchesCallback<T>(Action<T> callback)
            => responseCallback.Method == callback.Method
            && responseCallback.Target == callback.Target;

            public bool HasValidCallbackTarget()
            => responseCallback.Target switch
            {
                UnityEngine.Object o => o != null,
                null => _callbackIsStatic,
                _ => true
            };
        }
    }
}