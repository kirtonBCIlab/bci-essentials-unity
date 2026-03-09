using System;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    public partial class ResponseProvider
    {
        protected interface IResponseSubscriber
        {
            public void Notify<T>(T Response);

            public bool MatchesCallback<T>(Action<T> callback);

            public bool HasValidCallbackTarget();
        }

        protected struct ResponseSubscriber<TResponse> : IResponseSubscriber
        where TResponse : Response
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
                {
                    try { responseCallback(typedResponse); }
                    catch (Exception e) { Debug.LogException(e); }
                }
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