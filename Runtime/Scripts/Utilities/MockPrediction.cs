using UnityEngine;

namespace BCIEssentials.Utilities
{
    using LSLFramework;

    public class MockPrediction : Prediction
    {
        public MockPrediction(int index) { Index = index; }
        public MockPrediction(int selectedIndex, int targetCount) : this(selectedIndex)
        {
            Probabilities = new float[targetCount];
            float selectionProbability = Random.Range(1f / targetCount, 1f);
            float remainingProbability = 1f - selectionProbability;

            for (int i = 0; i < targetCount - 1; i++)
            {
                if (i == selectedIndex) Probabilities[i] = selectionProbability;
                else
                {
                    float maximumProbability = Mathf.Min(selectionProbability, remainingProbability);
                    Probabilities[i] = Random.Range(0, maximumProbability);
                    remainingProbability -= Probabilities[i];
                }
            }
            if (remainingProbability >= selectionProbability)
            {
                remainingProbability /= 2;
                selectionProbability += remainingProbability;
                Probabilities[selectedIndex] += selectionProbability;
            }
            if (targetCount > 1) Probabilities[^1] = remainingProbability;
        }
    }
}