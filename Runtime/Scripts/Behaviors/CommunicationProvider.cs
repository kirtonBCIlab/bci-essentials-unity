using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BCIEssentials.Behaviours
{
    using System.Linq;
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

        private readonly HashSet<int> _servicedComponentIds = new();


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

            IMarkerSource[] unservicedSources = GetUnservicedComponents<IMarkerSource>();
            IPredictionSink[] unservicedSinks = GetUnservicedComponents<IPredictionSink>();

            Array.ForEach(unservicedSources, ProvideMarkerWriter);
            Array.ForEach(unservicedSinks, ProvideResponseSubscription);
        }
        
        T[] GetUnservicedComponents<T>() where T : IHasInstanceID
        {
            T[] found = ProvisionScope switch
            {
                Scope.Scene => GetComponentsInScene<T>(),
                Scope.Children => GetComponentsInChildren<T>(),
                _ => GetComponents<T>(),
            };

            return found.Where(
                c => !_servicedComponentIds.Contains(c.GetInstanceID())
            ).ToArray();
        }

        public void UpdateClassifier() => MarkerWriter.PushUpdateClassifierMarker();


        private void ProvideMarkerWriter(IMarkerSource source)
        {
            source.MarkerWriter = MarkerWriter;
            _servicedComponentIds.Append(source.GetInstanceID());
        }
        private void ProvideResponseSubscription(IPredictionSink sink)
        {
            ResponseProvider.SubscribePredictions(sink.OnPrediction);
            _servicedComponentIds.Append(sink.GetInstanceID());
        }


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