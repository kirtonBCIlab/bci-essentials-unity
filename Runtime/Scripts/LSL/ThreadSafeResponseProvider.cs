using System;

namespace BCIEssentials.LSLFramework
{
    /** <summary>
    Manually invoked variant of the ResponseProvider.
    Avoids the potential need for subscribers to step
    callback results into an update method to interact
    with engine objects on the main thread.
    </summary> **/
    [Serializable]
    public class ThreadSafeResponseProvider : ResponseProvider
    {
        /** <summary>
        Pulls responses from internal inlet, distributing
        them to subscribers. Also handles connection.
        <br/>
        Use in Update()
        </summary> **/
        public void ProcessResponseProvision()
        {
            if (IsConnected)
            {
                PruneSubscriberList();
                PullAllResponses();
            }
            else if (!IsResolvingStream)
            {
                FindAndOpenStream();
            }
        }

        protected override void StartPolling()
        {
            if (IsConnected || IsResolvingStream) return;
            FindAndOpenStream();
        }
        protected override void StopPolling() { }
    }
}