using Tobii.XR;
using UnityEngine;

namespace Tobii.Ocumen.Filters.Samples
{
    public class PauseManager : MonoBehaviour
    {
        private bool _paused;
        private LineChartView[] _lineCharts;
        private EyeVisualizer _eyeVisualization;

        private void Awake()
        {
            _lineCharts = FindObjectsOfType(typeof(LineChartView)) as LineChartView[];
            _eyeVisualization = FindObjectOfType(typeof(EyeVisualizer)) as EyeVisualizer;
        }

        private void Update()
        {
            // Configured Button for input.
            if (ControllerManager.Instance.AnyTriggerPressed())
            {
                _paused = !_paused;

                foreach (var lineChart in _lineCharts)
                {
                    lineChart.enabled = !_paused;
                }

                _eyeVisualization.enabled = !_paused;
            }
        }
    }
}