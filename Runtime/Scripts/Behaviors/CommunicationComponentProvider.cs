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
                GetComponentsInChildren<IBCIMarkerSource>(),
                source => source.MarkerWriter = MarkerWriter
            );
            Array.ForEach(
                GetComponentsInChildren<ISelector>(),
                selector => ResponseProvider.SubscribePredictions(
                    prediction => selector.MakeSelection(prediction.Value)
                )
            );
        }

        private void Awake() => ProvideMarkerComponentsToChildren();

        public void UpdateClassifier() => MarkerWriter.PushUpdateClassifierMarker();
    }


    public interface IBCIMarkerSource
    {
        public MarkerWriter MarkerWriter { set; }
    }
}