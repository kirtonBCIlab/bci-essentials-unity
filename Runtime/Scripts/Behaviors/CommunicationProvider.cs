using System;

namespace BCIEssentials.Behaviours
{
    using Extensions;
    using LSLFramework;
    using Selection;
    using static Utilities.ComponentSearchMethods;

    public class CommunicationProvider : MonoBehaviourUsingExtendedAttributes
    {
        public enum Scope { Scene, Children, SameObject }
        public Scope provisionScope;

        protected MarkerWriter MarkerWriter;
        protected ResponseProvider ResponseProvider;


        /// <summary>
        /// Create or fetch reference to required LSL components,
        /// connecting any marker sources or selectors
        /// found on the host object or it's children
        /// </summary>
        protected void ProvideCommunication()
        {
            gameObject.GetOrAddComponent(ref MarkerWriter, true);
            gameObject.GetOrAddComponent(ref ResponseProvider, true);

            (
                IMarkerSource[] sources,
                IPredictionSink[] sinks
            ) = provisionScope switch
            {
                Scope.Scene => (
                    GetComponentsInScene<IMarkerSource>(),
                    GetComponentsInScene<IPredictionSink>()
                ),
                Scope.Children => (
                    GetComponentsInChildren<IMarkerSource>(),
                    GetComponentsInChildren<IPredictionSink>()
                ),
                _ => (
                    GetComponents<IMarkerSource>(),
                    GetComponents<IPredictionSink>()
                )
            };

            Array.ForEach(sources, ProvideMarkerWriter);
            Array.ForEach(sinks, ProvideResponseSubscription);
        }

        private void Awake() => ProvideCommunication();

        public void UpdateClassifier() => MarkerWriter.PushUpdateClassifierMarker();


        private void ProvideMarkerWriter(IMarkerSource source)
        => source.MarkerWriter = MarkerWriter;
        private void ProvideResponseSubscription(IPredictionSink sink)
        => ResponseProvider.SubscribePredictions(sink.OnPrediction);
    }


    public interface IMarkerSource
    {
        public MarkerWriter MarkerWriter { set; }
    }
}