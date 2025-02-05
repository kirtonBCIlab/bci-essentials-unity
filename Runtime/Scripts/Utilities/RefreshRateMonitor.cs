using UnityEngine;

namespace BCIEssentials.Utilities
{
    public class RefreshRateMonitor : MonoBehaviour
    {
        [InspectorReadOnly]
        [SerializeField]
        private float _currentRefreshRate;
        public float CurrentRefreshRate => _currentRefreshRate;
        
        [InspectorReadOnly]
        [SerializeField]
        private float _averageRefreshRate;
        public float AverageRefreshRate => _averageRefreshRate;
        
        private float _sumRefreshRate;
        private int _refreshCounter;

        private void Update()
        {
            _currentRefreshRate = 1 / Time.deltaTime;
            _refreshCounter += 1;
            _sumRefreshRate += CurrentRefreshRate;

            if (_refreshCounter < Application.targetFrameRate)
            {
                return;
            }
            
            _averageRefreshRate = _sumRefreshRate / _refreshCounter;
            if (AverageRefreshRate < 0.95 * Application.targetFrameRate)
            {
                Debug.Log($"Refresh rate is below 95% of target, avg refresh rate: {AverageRefreshRate}");
            }

            _sumRefreshRate = 0;
            _refreshCounter = 0;
        }
    }
}