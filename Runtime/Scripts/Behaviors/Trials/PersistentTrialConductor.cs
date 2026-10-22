using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    public abstract class PersistentTrialConductor : TrialConductor
    {
        public int TrainingEpochCount = 3;
        public float EpochLength = 1.0f;
        public float InterEpochInterval = 0;


        protected override IEnumerator Run()
        {
            WaitForSeconds segmentDuration = new(EpochLength + InterEpochInterval);

            if (HasTrainingTarget)
            {
                for (int i = 0; i < TrainingEpochCount; i++)
                {
                    yield return RunTrialSegment(segmentDuration);
                }
            }
            else while (true) yield return RunTrialSegment(segmentDuration);
        }


        protected IEnumerator RunTrialSegment(float duration)
        {
            yield return RunTrialSegment(new WaitForSeconds(duration));
        }
        protected IEnumerator RunTrialSegment(WaitForSeconds duration)
        {
            if (MarkerWriter != null)
            {
                if (HasTrainingTarget) SendTrainingMarker(TrainingTarget.Value);
                else SendClassificationMarker();
            }
            yield return duration;
        }

        protected abstract void SendTrainingMarker(int trainingIndex);
        protected abstract void SendClassificationMarker();
    }
}