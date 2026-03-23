using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BCIEssentials.Behaviours
{
    using System.Linq;
    using Extensions;
    using LSLFramework;
    using static Utilities.ComponentSearchMethods;

    public class CommunicationProvider : MonoBehaviourUsingExtendedAttributes
    {
        [Flags]
        public enum ProvisionOccasion { Manual, Awake, SceneLoad = 3 }
        public enum Scope { Scene, Children, SameObject }

        public ProvisionOccasion ProvisionTriggers = ProvisionOccasion.Awake;
        public Scope ProvisionScope = Scope.Scene;

        protected MarkerWriter MarkerWriter;
        protected ResponseProvider ResponseProvider;

        private readonly HashSet<int> _servicedComponentIds = new();


        private void Awake()
        {
            if (ProvisionTriggers.HasFlag(ProvisionOccasion.SceneLoad))
            {
                SceneManager.activeSceneChanged += (_, _) => ProvideCommunication();
            }
            else if (ProvisionTriggers.HasFlag(ProvisionOccasion.Awake))
            {
                ProvideCommunication();
            }
        }


        /// <summary>
        /// Create or fetch reference to required LSL components,
        /// connecting any marker sources or selectors
        /// found in the configured scope
        /// </summary>
        public void ProvideCommunication()
        {
            WarnIfProvisionScopeOverlaps();
            gameObject.GetOrAddComponent(ref MarkerWriter, true);
            gameObject.GetOrAddComponent(ref ResponseProvider, true);

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