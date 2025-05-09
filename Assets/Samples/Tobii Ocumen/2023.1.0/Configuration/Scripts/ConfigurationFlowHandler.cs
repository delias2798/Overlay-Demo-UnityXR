using System;
using Tobii.XR;
using UnityEngine;

namespace Tobii.Ocumen.Configuration.Samples
{
    public class ConfigurationFlowHandler : MonoBehaviour
    {
        private GameObject _positionGuide;
        private CalibrationView _calibrationView;
        private GameObject _postCalibrationVisualizer;
        private ApplicationFlow _flow;

        private void Awake()
        {
            _positionGuide = transform.Find("Position Guide").gameObject;
            _calibrationView = GetComponent<CalibrationView>();
            _postCalibrationVisualizer = transform.Find("Post-Calibration Visualizer").gameObject;
        }

        private void Update()
        {
            // Configured Button for input.
            if (ControllerManager.Instance.AnyTriggerPressed())
            {
                switch (_flow)
                {
                    case ApplicationFlow.NotStarted:
                    {
                        var provider = TobiiXR.Internal.Provider as TobiiProvider;
                        if ((provider?.PositionGuideSupported).GetValueOrDefault(true)) StartPositionGuide();
                        else StartCalibration();
                        break;
                    }
                    case ApplicationFlow.PositionGuide:
                        StartCalibration();
                        break;
                    case ApplicationFlow.Calibration:
                        break;
                    case ApplicationFlow.PostCalibrationVisualizer:
                        HidePostCalibrationVisualizer();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void StartPositionGuide()
        {
            _positionGuide.SetActive(true);
            _flow = ApplicationFlow.PositionGuide;
        }

        private void StartCalibration()
        {
            _positionGuide.SetActive(false);
            _calibrationView.TriggerCalibrationAndThen(StartPostCalibrationVisualizer);
            _flow = ApplicationFlow.Calibration;
        }

        private void StartPostCalibrationVisualizer()
        {
            _flow = ApplicationFlow.PostCalibrationVisualizer;
            _postCalibrationVisualizer.SetActive(true);
        }

        private void HidePostCalibrationVisualizer()
        {
            _postCalibrationVisualizer.SetActive(false);
            _flow = ApplicationFlow.NotStarted;
        }

        private enum ApplicationFlow
        {
            NotStarted,
            PositionGuide,
            Calibration,
            PostCalibrationVisualizer,
        }
    }
}