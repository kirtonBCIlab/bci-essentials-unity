using BCIEssentials.ControllerBehaviors;
using UnityEngine;
using System;

using static UnityEngine.Object;

namespace BCIEssentials.Controllers
{
    public static class BCIController
    {
        public static BCIControllerInstance Instance
        { get; private set; }

        public static BCIControllerBehavior ActiveBehavior
        => Instance == null? null: Instance.ActiveBehavior;

        private static bool _quitMessageConnected;
        private static bool _applicationIsQuitting;


        public static void NotifyInstanceCreated
        (BCIControllerInstance createdInstance)
        {
            if (!_quitMessageConnected)
            {
                Application.quitting += () =>
                    _applicationIsQuitting = true;
                _quitMessageConnected = true;
            }

            if (Instance == null) Instance = createdInstance;
        }

        public static void NotifyInstanceDestroyed
        (BCIControllerInstance destroyedInstance)
        {
            if (Instance == destroyedInstance) Instance = null;
        }

        
        public static void ChangeBehavior(BCIBehaviorType behaviorType)
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.ChangeBehavior(behaviorType);
        }

        public static bool RegisterBehavior(BCIControllerBehavior behavior, bool setAsActive = false)
        {
            if (Instance == null)
            {
                Instance = FindObjectOfType<BCIControllerInstance>();

                if (Instance == null)
                {
                    Debug.Log("No BCI Controller Instance set, creating one...");
                    Instance = CreateInstance();
                }
            }
            return Instance.RegisterBehavior(behavior, setAsActive);
        }

        public static void UnregisterBehavior(BCIControllerBehavior behavior)
        {
            if (_applicationIsQuitting) return;

            if (Instance == null)
            {
                Debug.LogWarning("No Instance to unregister from");
                return;
            }

            Instance.UnregisterBehavior(behavior);
        }

        public static bool HasBehaviorForType(BCIBehaviorType type)
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            return Instance.HasBehaviorForType(type);
        }
        
        public static bool HasBehaviorOfType<T>() where T : BCIControllerBehavior
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            return Instance.HasBehaviorOfType<T>();
        }


        private static BCIControllerInstance CreateInstance()
        {
            GameObject instanceHost = new("BCI Controller Instance");
            DontDestroyOnLoad(instanceHost);
            return instanceHost.AddComponent<BCIControllerInstance>();
        }


        #region Behavior Passthroughs

        /// <summary>
        /// <para>Invokes <see cref="StartStimulusRun"/> if <see cref="ActiveBehavior.StimulusRunning"/> is false.</para>
        /// <para>Invokes <see cref="StopStimulusRun"/> if <see cref="ActiveBehavior.StimulusRunning"/> is true.</para>
        /// </summary>
        public static void StartStopStimulus()
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.StartStopStimulus();
        }
        
        /// <summary>
        /// Start a new stimulus run. Will end an active stimulus run if present.
        /// </summary>
        /// <param name="sendConstantMarkers">
        /// If true will also write to the marker stream until
        /// the stimulus run ends or the number of markers sent equals <see cref="ActiveBehavior.trainTarget"/>.
        /// </param>
        public static void StartStimulusRun(bool sendConstantMarkers = true)
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.StartStimulusRun(sendConstantMarkers);
        }

        /// <summary>
        /// Stops the current stimulus run.
        /// </summary>
        public static void StopStimulusRun()
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.StopStimulusRun();
        }

        /// <summary>
        /// Select an object from <see cref="ActiveBehavior.SelectableSPOs"/>.
        /// </summary>
        /// <param name="objectIndex">The index value of the object to select.</param>
        /// <param name="stopStimulusRun">If true will end the current stimulus run.</param>
        public static void SelectSPO(int objectIndex, bool stopStimulusRun = false)
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.SelectSPO(objectIndex, stopStimulusRun);
        }
        
        /// <summary>
        /// Select an object from <see cref="ActiveBehavior.SelectableSPOs"/> if no objects were
        /// selected during a stimulus run.
        /// </summary>
        /// <param name="objectIndex"></param>
        public static void SelectSPOAtEndOfRun(int objectIndex)
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.SelectSPOAtEndOfRun(objectIndex);
        }

        /// <summary>
        /// Start the training behavior for the requested type.
        /// </summary>
        /// <param name="trainingType">
        /// The training behavior type.
        /// Not all behaviors may be implemented by a controller behavior type
        /// </param>
        public static void StartTraining(BCITrainingType trainingType)
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.StartTraining(trainingType);
        }

        public static void StartAutomatedTraining() => StartTraining(BCITrainingType.Automated);
        public static void StartUserTraining() => StartTraining(BCITrainingType.User);
        public static void StartIterativeTraining() => StartTraining(BCITrainingType.Iterative);
        public static void StartSingleTraining() => StartTraining(BCITrainingType.Single);

        /// <summary>
        /// Stops the current training run.
        /// </summary>
        public static void StopTraining()
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.StopTraining();
        }

        public static void UpdateClassifier()
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.UpdateClassifier();
        }

        public static void PassBessyPythonMessage(string message)
        {
            if (Instance == null)
            throw new NullReferenceException("No BCI Controller Instance set");

            Instance.PassBessyPythonMessage(message);
        }

        #endregion
    }
}