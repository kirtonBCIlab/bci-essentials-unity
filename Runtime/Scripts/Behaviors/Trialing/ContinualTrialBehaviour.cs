using System.Collections;
using UnityEngine;

namespace BCIEssentials.Behaviours.Trialing
{
    public abstract class ContinualTrialBehaviour : TrialBehaviour
    {
        public int TrainingEpochCount = 3;
        public float EpochLength = 1.0f;
        public float InterEpochInterval = 0;


        protected override IEnumerator Run()
        {
            int epochCount = 0;
            while (epochCount < TrainingEpochCount || !HasTrainingTarget)
            {
                if (MarkerWriter != null)
                {
                    if (HasTrainingTarget) SendTrainingMarker(TrainingTarget.Value);
                    else SendClassificationMarker();
                }
                yield return new WaitForSeconds(EpochLength + InterEpochInterval);
            }
        }

        protected abstract void SendTrainingMarker(int trainingIndex);
        protected abstract void SendClassificationMarker();
    }
}