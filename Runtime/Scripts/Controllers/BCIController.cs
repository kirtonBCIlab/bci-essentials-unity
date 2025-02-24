using System.Collections.Generic;
using System.Collections;
using BCIEssentials.LSLFramework;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.StimulusObjects;
using UnityEngine;
using UnityEngine.Events;

namespace BCIEssentials.Controllers
{
    public class BCIController : MonoBehaviour
    {
        [SerializeField] private LSLMarkerStreamWriter _lslMarkerStream;
        [SerializeField] private LSLResponseProvider _lslResponseStream;
        
        [Space]
        [SerializeField] private bool _dontDestroyActiveInstance;

        public static BCIController Instance { get; private set; }
        public BCIControllerBehavior ActiveBehavior { get; private set; }

        public Dictionary<KeyCode, UnityAction> _keyBindings = new();
        private Dictionary<BCIBehaviorType, BCIControllerBehavior> _registeredBehaviors = new();
       //public bool _hotkeysEnabled {get; private set;}

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_lslMarkerStream == null && !TryGetComponent(out _lslMarkerStream))
            {
                Debug.LogError($"No component of type {typeof(LSLMarkerStreamWriter)} found");
                enabled = false;
                return;
            }

            if (_lslResponseStream == null && !TryGetComponent(out _lslResponseStream))
            {
                Debug.LogError($"No component of type {typeof(LSLResponseProvider)} found");
                enabled = false;
                return;
            }

            if (Instance != null)
            {
                Debug.Log("An existing controller instance is already assigned.");
                return;
            }

            Instance = this;

            if (_dontDestroyActiveInstance)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            //Check for key inputs
            foreach (var (keyCode, action) in _keyBindings)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    action?.Invoke();
                }
            }
        }

        private void RegisterKeyBindings()
        {
            _keyBindings.TryAdd(KeyCode.S, StartStopStimulus);

            //TODO: Refactor out training
            _keyBindings.TryAdd(KeyCode.T, () => { StartTraining(BCITrainingType.Automated);});
            _keyBindings.TryAdd(KeyCode.I, () => { StartTraining(BCITrainingType.Iterative);});
            _keyBindings.TryAdd(KeyCode.U, () => { StartTraining(BCITrainingType.User);});
            _keyBindings.TryAdd(KeyCode.Semicolon, () => { StartTraining(BCITrainingType.Single);});
            _keyBindings.TryAdd(KeyCode.Backspace, () => {UpdateClassifier();});

            //Register Object Selection
            _keyBindings.TryAdd(KeyCode.Alpha0, () => { SelectSPOAtEndOfRun(0); });
            _keyBindings.TryAdd(KeyCode.Alpha1, () => { SelectSPOAtEndOfRun(1); });
            _keyBindings.TryAdd(KeyCode.Alpha2, () => { SelectSPOAtEndOfRun(2); });
            _keyBindings.TryAdd(KeyCode.Alpha3, () => { SelectSPOAtEndOfRun(3); });
            _keyBindings.TryAdd(KeyCode.Alpha4, () => { SelectSPOAtEndOfRun(4); });
            _keyBindings.TryAdd(KeyCode.Alpha5, () => { SelectSPOAtEndOfRun(5); });
            _keyBindings.TryAdd(KeyCode.Alpha6, () => { SelectSPOAtEndOfRun(6); });
            _keyBindings.TryAdd(KeyCode.Alpha7, () => { SelectSPOAtEndOfRun(7); });
            _keyBindings.TryAdd(KeyCode.Alpha8, () => { SelectSPOAtEndOfRun(8); });
            _keyBindings.TryAdd(KeyCode.Alpha9, () => { SelectSPOAtEndOfRun(9); });
        }

        public static void EnableDisableHotkeys(bool _registerKeyBindings)
        {
             if (Instance == null)
            {
                Debug.Log("No BCI Controller instance set.");
                return;
            }

            if(_registerKeyBindings == true)
            {
                Instance.RegisterKeyBindings();
            }
            else
            {
                Instance._keyBindings.Clear();
            }
        }
        
        public static void ChangeBehavior(BCIBehaviorType behaviorType)
        {
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.CleanUp();
            }

            if (Instance._registeredBehaviors.TryGetValue(behaviorType, out var requestedBehavior))
            {
                Instance.ActiveBehavior = requestedBehavior;
                Instance.ActiveBehavior.Initialize(Instance._lslMarkerStream, Instance._lslResponseStream);
                Debug.Log($"New BCI Controller active of type {behaviorType}");
            }
            else
            {
                Debug.LogError($"Unable to find a registered behavior for type {behaviorType}]");
            }
        }

        public static bool RegisterBehavior(BCIControllerBehavior behavior, bool setAsActive = false)
        {
            if (behavior == null)
            {
                Debug.LogError("Controller Behavior is null");
                return false;
            }

            if (Instance == null)
            {
                Debug.LogError("No BCI Controller instance set.");
                return false;
            }

            if (Instance._registeredBehaviors.TryAdd(behavior.BehaviorType, behavior))
            {
                if (setAsActive)
                {
                    ChangeBehavior(behavior.BehaviorType);
                }

                return true;
            }

            Debug.LogWarning($"Was unable to register a new controller behavior for type {behavior.BehaviorType}");
            return false;
        }

        public static void UnregisterBehavior(BCIControllerBehavior behavior)
        {
            if (behavior == null)
            {
                return;
            }
            
            if (Instance == null)
            {
                Debug.Log("No BCI Controller instance set.");
                return;
            }

            if (!Instance._registeredBehaviors.TryGetValue(behavior.BehaviorType, out var foundBehavior) ||
                foundBehavior != behavior)
            {
                return;
            }

            Instance._registeredBehaviors.Remove(behavior.BehaviorType);
            Debug.Log($"Unregistered behavior for type {behavior.BehaviorType}");
            
            if (Instance.ActiveBehavior == behavior)
            {
                Instance.ActiveBehavior = null;
                Debug.Log("The active behavior was also removed.");
            }
        }

        public static bool HasBehaviorForType(BCIBehaviorType type)
        {
            if (Instance == null)
            {
                Debug.Log("No BCI Controller instance set.");
                return false;
            }
            
            return Instance._registeredBehaviors.ContainsKey(type);
        }
        
        public static bool HasBehaviorOfType<T>() where T : BCIControllerBehavior
        {
            if (Instance == null)
            {
                Debug.Log("No BCI Controller instance set.");
                return false;
            }
            
            foreach (var value in Instance._registeredBehaviors.Values)
            {
                if (value is T)
                {
                    return true;
                }
            }
            
            return false;
        }


        #region Behavior Passthroughs

        /// <summary>
        /// <para>Invokes <see cref="StartStimulusRun"/> if <see cref="ActiveBehavior.StimulusRunning"/> is false.</para>
        /// <para>Invokes <see cref="StopStimulusRun"/> if <see cref="ActiveBehavior.StimulusRunning"/> is true.</para>
        /// </summary>
        public static void StartStopStimulus()
        {
            if (Instance.ActiveBehavior == null)
            {
                return;
            }

            Instance.ActiveBehavior.StartStopStimulusRun();
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
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.StartStimulusRun(sendConstantMarkers);
            }
        }

        /// <summary>
        /// Stops the current stimulus run.
        /// </summary>
        public static void StopStimulusRun()
        {
            if (Instance.ActiveBehavior == null)
            {
                return;
            }

            Instance.ActiveBehavior.StopStimulusRun();
        }

        /// <summary>
        /// Select an object from <see cref="ActiveBehavior.SelectableSPOs"/>.
        /// </summary>
        /// <param name="objectIndex">The index value of the object to select.</param>
        /// <param name="stopStimulusRun">If true will end the current stimulus run.</param>
        public static void SelectSPO(int objectIndex, bool stopStimulusRun = false)
        {
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.SelectSPO(objectIndex, stopStimulusRun);
            }
        }
        
        /// <summary>
        /// Select an object from <see cref="ActiveBehavior.SelectableSPOs"/> if no objects were
        /// selected during a stimulus run.
        /// </summary>
        /// <param name="objectIndex"></param>
        public static void SelectSPOAtEndOfRun(int objectIndex)
        {
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.SelectSPOAtEndOfRun(objectIndex);
            }
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
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.StartTraining(trainingType);
            }
        }

        /// <summary>
        /// Stops the current training run.
        /// </summary>
        public static void StopTraining()
        {
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.StopTraining();
            }
        }

        public static void WhileDoSingleTraining()
        {
            if (Instance.ActiveBehavior != null)
            {
                Instance.StartCoroutine(Instance.ActiveBehavior.WhileDoSingleTraining());
            }
        }

        public static void UpdateClassifier()
        {
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.UpdateClassifier();
            }
        }

        public static void PassBessyPythonMessage(string message)
        {
            if (Instance.ActiveBehavior != null)
            {
                Instance.ActiveBehavior.PassBessyPythonMessage(message);
            }
        }

        #endregion
    }
}