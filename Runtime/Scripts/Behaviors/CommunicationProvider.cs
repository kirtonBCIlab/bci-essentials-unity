using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BCIEssentials.Behaviours
{
    using LSLFramework;
    using static Utilities.ComponentSearchMethods;

    public class CommunicationProvider : MonoBehaviourUsingExtendedAttributes
    {
        public enum ProvisionOccasion { Manual, Awake, SceneLoad }
        public enum Scope { Scene, Children, SameObject }

        public ProvisionOccasion ProvisionTrigger = ProvisionOccasion.Awake;
        public Scope ProvisionScope = Scope.Scene;

        [SerializeField] protected MarkerWriter MarkerWriter;
        [SerializeField] protected ResponseProvider ResponseProvider;


        private void Awake()
        {
            MarkerWriter.OpenStream();
            if (ProvisionTrigger == ProvisionOccasion.SceneLoad)
            {
                SceneManager.activeSceneChanged += (_, _) => ProvideCommunication();
            }
            else if (ProvisionTrigger == ProvisionOccasion.Awake)
            {
                ProvideCommunication();
            }
        }

        private void OnDestroy()
        {
            MarkerWriter.CloseStream();
            ResponseProvider.CloseStream();
        }


        /// <summary>
        /// Create or fetch reference to required LSL components,
        /// connecting any marker sources or selectors
        /// found in the configured scope
        /// </summary>
        public void ProvideCommunication()
        {
            WarnIfProvisionScopeOverlaps();

            IMarkerSource[] unservicedSources = FindComponentsInScope<IMarkerSource>();
            IPredictionSink[] unservicedSinks = FindComponentsInScope<IPredictionSink>();

            Array.ForEach(unservicedSources, ProvideMarkerWriter);
            Array.ForEach(unservicedSinks, ProvideResponseSubscription);
        }

        T[] FindComponentsInScope<T>()
        => ProvisionScope switch
        {
            Scope.Scene => GetComponentsInScene<T>(),
            Scope.Children => GetComponentsInChildren<T>(),
            _ => GetComponents<T>(),
        };

        public void UpdateClassifier() => MarkerWriter.PushUpdateClassifierMarker();


        private void ProvideMarkerWriter(IMarkerSource source)
        => source.MarkerWriter = MarkerWriter;
        private void ProvideResponseSubscription(IPredictionSink sink)
        => ResponseProvider.SubscribePredictions(sink.OnPrediction);


        private void WarnIfProvisionScopeOverlaps()
        {
            CommunicationProvider[] providersInScope = ProvisionScope switch
            {
                Scope.Scene => GetComponentsInScene<CommunicationProvider>(),
                Scope.Children => GetComponentsInChildren<CommunicationProvider>(),
                _ => GetComponents<CommunicationProvider>()
            };
            if (providersInScope.Length > 1)
            {
                Debug.LogWarning(
                    $"Another Communication Provider exists within the {ProvisionScope}"
                    + $" (selected scope), which may lead to unexpected behaviour"
                );
            }
        }
    }
}