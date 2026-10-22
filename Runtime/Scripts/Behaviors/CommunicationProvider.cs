using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BCIEssentials
{
    using LSLFramework;
    using static Utilities.ComponentSearchMethods;

    public class CommunicationProvider : MonoBehaviourWithFoldoutGroups
    {
        public enum ProvisionOccasion { Manual, Awake, SceneLoad }
        public enum Scope { Scene, Children, SameObject }

        public ProvisionOccasion ProvisionTrigger = ProvisionOccasion.Awake;
        public Scope ProvisionScope = Scope.Children;

        [SerializeField] protected MarkerWriter MarkerWriter;
        [SerializeField] protected ResponseProvider ResponseProvider;

        private readonly List<IMarkerSource> _servicedSources = new();
        private readonly List<IPredictionSink> _servicedSinks = new();


        protected virtual void Awake()
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

        protected virtual void OnDestroy()
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

            IMarkerSource[] sources = FindComponentsInScope<IMarkerSource>();
            IPredictionSink[] sinks = FindComponentsInScope<IPredictionSink>();

            Array.ForEach(sources, ProvideMarkerWriter);
            Array.ForEach(sinks, ProvideResponseSubscription);
        }

        T[] FindComponentsInScope<T>()
        => ProvisionScope switch
        {
            Scope.Scene => GetComponentsInScene<T>(),
            Scope.Children => GetComponentsInChildren<T>(),
            _ => GetComponents<T>(),
        };

        public void UpdateClassifier() => MarkerWriter.PushUpdateClassifierMarker();


        protected void ProvideMarkerWriter(IMarkerSource source)
        {
            if (_servicedSources.Contains(source)) return;
            source.MarkerWriter = MarkerWriter;
            _servicedSources.Add(source);
        }
        protected void ProvideResponseSubscription(IPredictionSink sink)
        {
            if (_servicedSinks.Contains(sink)) return;
            ResponseProvider.SubscribePredictions(sink.OnPrediction);
            _servicedSinks.Add(sink);
        }


        protected void WarnIfProvisionScopeOverlaps()
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