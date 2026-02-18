using UnityEngine;

namespace BCIEssentials.Behaviours
{
    using Trialing;
    using Training;

    public class BCIController : CommunicationComponentProvider
    {
        public bool IsRunningTrial => _trialBehaviour.IsRunning;
        public bool IsRunningTraining => _trainingBehaviour.IsRunning;

        [SerializeField]
        private TrialBehaviour _trialBehaviour;
        [SerializeField]
        private TrainingBehaviour _trainingBehaviour;


        public void StartTrial() => _trialBehaviour.Begin();
        public void InterruptTrial() => _trialBehaviour.Interrupt();

        public void StartTraining() => _trainingBehaviour.Begin();
        public void InterruptTraining() => _trainingBehaviour.Interrupt();
    }
}