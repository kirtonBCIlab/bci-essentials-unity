using System;
using System.Collections.Generic;
using System.Linq;

namespace BCIEssentials.LSLFramework
{
    public class LSLResponseProvider: LSLStreamReader
    {
        private List<IResponseSubscriber> _subscribers = new();


        public void SubscribePredictions(Action<Prediction> callback)
        => Subscribe(callback);
        public void SubscribeAll(Action<LSLResponse> callback)
        => Subscribe(callback);
        public void Subscribe<T>(Action<T> callback)
        where T: LSLResponse
        => _subscribers.Add(new ResponseSubscriber<T>(callback));

        
        public bool Unsubscribe<T>(Action<T> callback)
        where T: LSLResponse
        => _subscribers.RemoveAll
        (
            subscriber => subscriber.MatchesCallback(callback)
        ) == 1;


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