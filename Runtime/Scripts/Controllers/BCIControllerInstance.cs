using System.Collections.Generic;
using UnityEngine;
using BCIEssentials.LSLFramework;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Utilities;
using System.Linq;
using System;

namespace BCIEssentials.Controllers
{
    public class BCIControllerInstance: MonoBehaviour
    {
        [SerializeField]
        private LSLMarkerWriter _markerWriter;
        [SerializeField]
        private LSLResponseProvider _responseProvider;

        [Space, SerializeField]
        private bool _persistBetweenScenes;

        public BCIControllerBehavior ActiveBehavior
        { get; private set; }

        private Dictionary<BCIBehaviorType, BCIControllerBehavior>
        _registeredBehaviors = new();


        private void Awake() => Initialize();

        public void Initialize()
        {
            gameObject.FindOrCreateComponent(ref _markerWriter);
            gameObject.FindOrCreateComponent(ref _responseProvider);

            if (_persistBetweenScenes) DontDestroyOnLoad(gameObject);

            BCIController.NotifyInstanceCreated(this);
        }

        private void OnDestroy()
        => BCIController.NotifyInstanceDestroyed(this);


        public void ChangeBehavior
        (
            BCIBehaviorType behaviorType
        )
        {
            if (ActiveBehavior != null) ActiveBehavior.CleanUp();

            if (_registeredBehaviors.TryGetValue
                (behaviorType, out var requestedBehavior)
            ) {
                ActiveBehavior = requestedBehavior;
                ActiveBehavior.Initialize(_markerWriter, _responseProvider);
                Debug.Log($"New BCI Controller active of type {behaviorType}");
            }
            else {
                Debug.LogError(
                    "Unable to find a registered "
                    + $"behavior for type {behaviorType}"
                );
            }
        }


        public bool RegisterBehavior
        (
            BCIControllerBehavior behavior,
            bool setAsActive = false
        )
        {
            if (behavior == null)
            throw new ArgumentNullException(nameof(behavior));

            if (_registeredBehaviors.TryAdd
                (behavior.BehaviorType, behavior)
            ) {
                if (setAsActive) ChangeBehavior(behavior.BehaviorType);
                return true;
            }

            Debug.LogWarning(
                "Was unable to register a new controller "
                +$"behavior for type {behavior.BehaviorType}"
            );
            return false;
        }

        public void UnregisterBehavior
        (
            BCIControllerBehavior behavior
        )
        {
            if (behavior == null)
            throw new ArgumentNullException(nameof(behavior));

            if (!_registeredBehaviors.TryGetValue
                (behavior.BehaviorType, out var foundBehavior)
                || foundBehavior != behavior
            ) return;

            _registeredBehaviors.Remove(behavior.BehaviorType);
            Debug.Log(
                "Unregistered behavior for type"
                + behavior.BehaviorType
            );

            if (ActiveBehavior == behavior)
            {
                ActiveBehavior.CleanUp();
                ActiveBehavior = null;
                Debug.Log("The active behavior was also removed.");
            }
        }


        public bool HasBehaviorForType(BCIBehaviorType behaviorType)
        => _registeredBehaviors.ContainsKey(behaviorType);

        public bool HasBehaviorOfType<T>() where T: BCIControllerBehavior
        => _registeredBehaviors.Values.Any(value => value is T);


        #region Behavior Passthroughs

        /// <summary>
        /// <para>Invokes <see cref="StartStimulusRun"/> if <see cref="ActiveBehavior.StimulusRunning"/> is false.</para>
        /// <para>Invokes <see cref="StopStimulusRun"/> if <see cref="ActiveBehavior.StimulusRunning"/> is true.</para>
        /// </summary>
        public void StartStopStimulus()
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.StartStopStimulusRun();
        }

        /// <summary>
        /// Start a new stimulus run. Will end an active stimulus run if present.
        /// </summary>
        /// <param name="sendConstantMarkers">
        /// If true will also write to the marker stream until
        /// the stimulus run ends or the number of markers sent equals <see cref="ActiveBehavior.trainTarget"/>.
        /// </param>
        public void StartStimulusRun(bool sendConstantMarkers = true)
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.StartStimulusRun(sendConstantMarkers);
        }

        /// <summary>
        /// Stops the current stimulus run.
        /// </summary>
        public void StopStimulusRun()
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.StopStimulusRun();
        }

        /// <summary>
        /// Select an object from <see cref="ActiveBehavior.SelectableSPOs"/>.
        /// </summary>
        /// <param name="objectIndex">The index value of the object to select.</param>
        /// <param name="stopStimulusRun">If true will end the current stimulus run.</param>
        public void SelectSPO(int objectIndex, bool stopStimulusRun = false)
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.SelectSPO(objectIndex, stopStimulusRun);
        }
        
        /// <summary>
        /// Select an object from <see cref="ActiveBehavior.SelectableSPOs"/> if no objects were
        /// selected during a stimulus run.
        /// </summary>
        /// <param name="objectIndex"></param>
        public void SelectSPOAtEndOfRun(int objectIndex)
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.SelectSPOAtEndOfRun(objectIndex);
        }

        /// <summary>
        /// Start the training behavior for the requested type.
        /// </summary>
        /// <param name="trainingType">
        /// The training behavior type.
        /// Not all behaviors may be implemented by a controller behavior type
        /// </param>
        public void StartTraining(BCITrainingType trainingType)
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.StartTraining(trainingType);
        }
        
        public void StartAutomatedTraining() => StartTraining(BCITrainingType.Automated);
        public void StartUserTraining() => StartTraining(BCITrainingType.User);
        public void StartIterativeTraining() => StartTraining(BCITrainingType.Iterative);
        public void StartSingleTraining() => StartTraining(BCITrainingType.Single);

        /// <summary>
        /// Stops the current training run.
        /// </summary>
        public void StopTraining()
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.StopTraining();
        }

        public void UpdateClassifier()
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.UpdateClassifier();
        }

        public void PassBessyPythonMessage(string message)
        {
            if (ActiveBehavior == null)
            throw new NullReferenceException("No Active Behavior set");

            ActiveBehavior.PassBessyPythonMessage(message);
        }

        #endregion
    }
}