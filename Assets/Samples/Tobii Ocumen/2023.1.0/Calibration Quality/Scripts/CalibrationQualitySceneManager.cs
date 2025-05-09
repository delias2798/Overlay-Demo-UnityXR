using System;
using System.Collections.Generic;
using System.Linq;
using Tobii.Ocumen.Common;
using Tobii.XR;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace Tobii.Ocumen.Configuration.Samples
{
    public enum Eye
    {
        Left,
        Right
    }
    
    public class CalibrationQualitySceneManager : MonoBehaviour
    {
        [Serializable]
        public class EyeVisualization
        {
            public Eye eye;
            public int precisionStandardDeviation;
            public GameObject visualization;
        }

#pragma warning disable 649
        [SerializeField] private List<EyeVisualization> eyeVisualizations;
        [SerializeField] private GameObject visualizationsParent;
        [SerializeField] private GameObject menu;
        [SerializeField] private GameObject groundPlane;
        [SerializeField] private PrecisionButton[] precisionButtons;
        [SerializeField] private SpawnCalibrationQualityPointCloud leftEyePoints;
        [SerializeField] private SpawnCalibrationQualityPointCloud rightEyePoints;
#pragma warning restore 649

        private int _activePrecisionStandardDeviation = 1;
        private TrackedPoseDriver _trackedPoseDriver;
        private ViewState _currentViewState = ViewState.FirstView;
        private EyeVisualization _currentVisualization;

        // The current visualized view.
        private enum ViewState
        {
            FirstView,
            SecondViewLocked,
            SecondViewFree
        }
        
        private void Awake()
        {
            _trackedPoseDriver = visualizationsParent.GetComponent<TrackedPoseDriver>();

            ShowFirstView();
        }

        private void Start()
        {
            // Toggle on precision button for 1st standard deviation.
            TogglePrecisionButtons(precisionButtons[0]);
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (!ControllerManager.Instance.AnyTriggerPressed()) return;

            if (_currentViewState == ViewState.FirstView)
            {
                if (TobiiXR.FocusedObjects.Count == 0) return;

                var focusedObject = TobiiXR.FocusedObjects[0].GameObject;
                var visualizationButton = focusedObject.GetComponent<VisualizationButton>();
                var precisionButton = focusedObject.GetComponent<PrecisionButton>();

                // If Left/Right visualization buttons are pressed.
                if (visualizationButton != null)
                {
                    // Select visualization by eye and precision.
                    foreach (var eyeVisualization in eyeVisualizations.Where(vis =>
                        vis.eye == visualizationButton.Eye
                        && _activePrecisionStandardDeviation == vis.precisionStandardDeviation))
                    {
                        _currentVisualization = eyeVisualization;
                        GoToNextView();
                    }
                }
                // If precision buttons are pressed.
                else if (precisionButton != null)
                {
                    _activePrecisionStandardDeviation = precisionButton.PrecisionStandardDeviation;
                    TogglePrecisionButtons(precisionButton);

                    // Change the left/right visualizations currently being displayed in the world UI.
                    leftEyePoints.ChangePrecisionStandardDeviation(_activePrecisionStandardDeviation);
                    rightEyePoints.ChangePrecisionStandardDeviation(_activePrecisionStandardDeviation);
                }
            }
            else
            {
                GoToNextView();
            }
        }

        /// <summary>
        /// Changes the current view state to the next.
        /// </summary>
        private void GoToNextView()
        {
            _currentViewState = _currentViewState.Next();

            switch (_currentViewState)
            {
                case ViewState.FirstView:
                    ShowFirstView();
                    break;
                case ViewState.SecondViewLocked:
                    ShowSecondView();
                    _trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                    break;
                case ViewState.SecondViewFree:
                    ShowSecondView();
                    _trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.PositionOnly;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowSecondView()
        {
            groundPlane.SetActive(false);
            menu.SetActive(false);
            visualizationsParent.SetActive(true);
            _currentVisualization?.visualization.SetActive(true);
        }

        private void ShowFirstView()
        {
            groundPlane.SetActive(true);
            menu.SetActive(true);
            visualizationsParent.SetActive(false);
            eyeVisualizations.ForEach(vis => vis.visualization.SetActive(false));
        }

        private void TogglePrecisionButtons(PrecisionButton selectedPrecisionButton)
        {
            foreach (var precisionButton in precisionButtons)
            {
                if (precisionButton == selectedPrecisionButton)
                {
                    precisionButton.Select();
                }
                else
                {
                    precisionButton.Deselect();
                }
            }
        }
    }
}