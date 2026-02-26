using System;

namespace BCIEssentials.Behaviours
{
    using Extensions;
    using LSLFramework;
    using Selection;

    public class CommunicationComponentProvider : MonoBehaviourUsingExtendedAttributes
    {
        protected MarkerWriter MarkerWriter;
        protected ResponseProvider ResponseProvider;


        /// <summary>
        /// Create or fetch reference to required LSL components,
        /// connecting any marker sources or selectors
        /// found on the host object or it's children
        /// </summary>
        protected void ProvideMarkerComponentsToChildren()
        {
            gameObject.GetOrAddComponent(ref MarkerWriter);
            gameObject.GetOrAddComponent(ref ResponseProvider);

            Array.ForEach(
                GetComponentsInChildren<IMarkerSource>(),
                source => source.MarkerWriter = MarkerWriter
            );
            Array.ForEach(
                GetComponentsInChildren<IPredictionSink>(),
                selector => ResponseProvider.SubscribePredictions(
                    selector.OnPrediction
                )
            );
        }

        private void Awake() => ProvideMarkerComponentsToChildren();

        public void UpdateClassifier() => MarkerWriter.PushUpdateClassifierMarker();
    }


    public interface IMarkerSource
    {
        public MarkerWriter MarkerWriter { set; }
    }
}