using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Tobii.Ocumen.Configuration.Samples
{
    public class CalibrationView : MonoBehaviour
    {
        [Serializable]
        private struct CalibrationPoint
        {
            [Tooltip("The angle radiating outwards from the Z axis (forward direction) of the camera.")]
            public float angleFromCenter;

            [Tooltip(
                "The angle of rotation around the Z axis (forward direction) of the camera, with 0 degrees to the right and 180 degrees to the left.")]
            public float angularOffsetAroundCenter;

            public CalibrationPoint(float angleFromCenter, float angularOffsetAroundCenter)
            {
                this.angleFromCenter = angleFromCenter;
                this.angularOffsetAroundCenter = angularOffsetAroundCenter;
            }
        }

        [Header("Calibration Customization")]
#pragma warning disable 0649 // Field is never assigned to, and will always have its default value null
        [SerializeField,
         Tooltip(
             "The number of calibration points and their pattern. Points are positioned at 2 metres from the camera, and the angle from center and the angular offset around center (counter-clockwise, starting from right hand side) is customizable.")]
        private CalibrationPoint[] calibrationPattern;

        [SerializeField, Tooltip("The speed in degrees/second at which the point moves to the next location.")]
        private float pointMovementSpeed = 5f;

        [SerializeField,
         Tooltip("The animation curve of the point movement; controls acceleration of the point movement.")]
        private AnimationCurve movementAnimationCurve;

        [SerializeField, Tooltip("Displays circles at 5 degree increments from the center.")]
        private bool displayDebugLines;
#pragma warning restore 0649

        /// <summary>
        /// Variables for customizing the calibration sequence. 
        /// </summary>
        private const string
            InstructionsMessageText = "Follow the blue dot"; // The instruction text at the start of a calibration.

        private const string
            CalibrationSuccessfulMessageText =
                "Calibration successful!"; // The text to present when calibration succeeds.

        private const string
            CalibrationErrorFailedToComputeMessageText =
                "Calibration failed to compute model"; // The text to present when calibration fails to compute the result..

        private const string
            CalibrationErrorUnknownMessageText =
                "Calibration failed. Unknown error"; // The text to present when calibration fails with unkown error.

        private const float DisplayInstructionsDuration = 3.0f; // The duration of the instructions text.

        private const float
            UserGettingReadyDuration =
                1.0f; // The duration between the end of the instructions text and the start of the movement of the first calibration point.

        private const float
            StimuliMinimumDuration =
                0.8f; // The minimum duration for displaying a calibration point. Collection time per stimuli point varies due to various factors. Set to 0 if you want the point to move as soon as the collection is done. This is mainly for UX reasons to allow the user to follow the calibration without stress.

        private const float DisplayResultsDuration = 3.0f; // The duration for displaying the results text.

        private const float
            TimeDelayBeforeCollectingData =
                0.5f; // Time delay after the point stops moving and before the data collection begins.

        private readonly ConfigurationManager _configurationManager = new ConfigurationManager();
        private GameObject _calibrationParent;
        private GameObject _background;
        private GameObject _message;
        private GameObject _calibrationPointPivot;
        private GameObject _debugLines;
        private Transform _calibrationPointTransform;
        private int _currentPointIndex = -1;
        private Coroutine _movingCoroutine;
        private Coroutine _dataCollectionCoroutine;
        private TMP_Text _instructionsText;
        private bool _calibrationRunning;
        private Action _calibrationFinished;

        private void Awake()
        {
            var calibrationTransform = transform.Find("Calibration");
            _calibrationParent = calibrationTransform.gameObject;
            _background = calibrationTransform.Find("Background").gameObject;
            _message = calibrationTransform.Find("Message").gameObject;
            _calibrationPointPivot = calibrationTransform.Find("Calibration Pivot").gameObject;
            _debugLines = calibrationTransform.Find("Debug Degree Lines").gameObject;
            _calibrationPointTransform = _calibrationPointPivot.transform.Find("Point");
            _instructionsText = _message.GetComponentInChildren<TMP_Text>();
            HideCalibrationVisuals();
        }

        private void Update()
        {
            HandleCalibrationState();
        }

        private void OnApplicationQuit()
        {
            // Stop all running calibration point collection and animations.
            StopAllCoroutines();

            // Gracefully stop the calibration based on state.
            switch (_configurationManager.State)
            {
                case CalibrationState.NotStarted:
                case CalibrationState.Succeeded:
                case CalibrationState.Failed:
                    return;
                case CalibrationState.Starting:
                case CalibrationState.Collecting:
                    SleepUntilStateNoLongerIs(_configurationManager.State);
                    _configurationManager.Abort();
                    SleepUntilStateNoLongerIs(CalibrationState.Aborting);
                    return;
                case CalibrationState.AwaitingStimulus:
                    _configurationManager.Abort();
                    SleepUntilStateNoLongerIs(CalibrationState.Aborting);
                    return;
                case CalibrationState.Computing:
                    SleepUntilStateNoLongerIs(CalibrationState.Computing);
                    return;
            }
        }

        /// <summary>
        /// State handles for the calibration process. 
        /// </summary>
        private void HandleCalibrationState()
        {
            switch (_configurationManager.State)
            {
                case CalibrationState.NotStarted: // Initial state
                    break;
                case CalibrationState.Starting: // Transitional state
                    break;
                case CalibrationState.AwaitingStimulus:
                    MoveToNextStimulusPoint();
                    break;

                case CalibrationState.Collecting: // Transitional state 
                case CalibrationState.Computing: // Transitional state
                case CalibrationState.Aborting: // Transitional state
                    break;
                case CalibrationState.Aborted: // End state
                case CalibrationState.Failed: // End state
                case CalibrationState.Succeeded: // End state
                    var outcome = _configurationManager.Outcome;
                    _configurationManager.ResetState();
                    StartCoroutine(ShowOutcomeAndExitCalibration(outcome));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Moves the calibration point to the next position in the calibration sequence.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveCalibrationPoint()
        {
            // Unit circle coordinates.
            var angularOffsetFromCameraForwardVector = calibrationPattern[_currentPointIndex].angleFromCenter;
            var rotationAngleAroundCameraForwardVector =
                calibrationPattern[_currentPointIndex].angularOffsetAroundCenter;
            var xCord = angularOffsetFromCameraForwardVector *
                        Mathf.Cos(rotationAngleAroundCameraForwardVector * Mathf.Deg2Rad);
            var yCord = angularOffsetFromCameraForwardVector *
                        Mathf.Sin(rotationAngleAroundCameraForwardVector * Mathf.Deg2Rad);

            // Calculate target local rotation, transform coordinates to correctly mimic a unit circle (counter-clockwise rotation around center, starting from right hand side).
            var targetEuler = new Vector3(-yCord, xCord, 0);
            var targetLocalRotation = Quaternion.Euler(targetEuler);

            // Set up start and end rotations as well as duration of rotation.
            var elapsedTime = 0f;
            var startRotation = _calibrationPointPivot.transform.localRotation;
            var targetRotation = targetLocalRotation;
            var totalAngle = Quaternion.Angle(startRotation, targetRotation);
            var totalTime = totalAngle / pointMovementSpeed;

            // Rotates the calibration point over a duration.
            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                var step = movementAnimationCurve.Evaluate(elapsedTime / totalTime);
                _calibrationPointPivot.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, step);
                yield return null;
            }

            // Collect data for the calibration point.
            _dataCollectionCoroutine = StartCoroutine(CollectData());
            yield return _dataCollectionCoroutine;

            _movingCoroutine = null;
        }

        /// <summary>
        /// Collects data for the current calibration point.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CollectData()
        {
            // Give the user time to rest their eyes on the point before collecting.
            yield return new WaitForSecondsRealtime(TimeDelayBeforeCollectingData);

            // Collects data.
            _configurationManager.Collect(_calibrationPointTransform);

            yield return new WaitForSecondsRealtime(StimuliMinimumDuration);

            _dataCollectionCoroutine = null;
        }

        /// <summary>
        /// Iterates the calibration sequence and moves to the next point if there is any, otherwise it finishes the calibration.
        /// </summary>
        private void MoveToNextStimulusPoint()
        {
            if (_dataCollectionCoroutine == null && _movingCoroutine == null)
            {
                _currentPointIndex++;

                if (_currentPointIndex < calibrationPattern.Length)
                {
                    _movingCoroutine = StartCoroutine(MoveCalibrationPoint());
                }
                else
                {
                    _configurationManager.Compute();
                }
            }
        }

        /// <summary>
        /// Initializes calibration by displaying the instruction text and then starting the calibration process.
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitializeCalibration()
        {
            ShowCalibrationBackground();

            Debug.Log("Displaying Instructions.");

            ShowMessage(InstructionsMessageText);

            yield return new WaitForSecondsRealtime(DisplayInstructionsDuration);

            Debug.Log("User Getting Ready.");

            HideMessage();

            ShowCalibrationPoint();

            yield return new WaitForSecondsRealtime(UserGettingReadyDuration);

            Debug.Log("Starting Calibration.");
            _configurationManager.Start();
        }

        /// <summary>
        /// Shows the calibration background.
        /// </summary>
        private void ShowCalibrationBackground()
        {
            _background.SetActive(true);
        }

        /// <summary>
        /// Hides the calibration background.
        /// </summary>
        private void HideCalibrationBackground()
        {
            _background.SetActive(false);
        }

        /// <summary>
        /// Shows the calibration point.
        /// </summary>
        private void ShowCalibrationPoint()
        {
            _calibrationPointPivot.SetActive(true);
            _debugLines.SetActive(displayDebugLines);
        }

        /// <summary>
        /// Hides the calibration point.
        /// </summary>
        private void HideCalibrationPoint()
        {
            _calibrationPointPivot.SetActive(false);
            _debugLines.SetActive(false);
        }

        /// <summary>
        /// Shows the message text to the user.
        /// </summary>
        private void ShowMessage(string text)
        {
            _instructionsText.text = text;
            _message.SetActive(true);
        }

        /// <summary>
        /// Hides the message text.
        /// </summary>
        private void HideMessage()
        {
            _message.SetActive(false);
            _instructionsText.text = String.Empty;
        }

        /// <summary>
        /// Hides the calibration visuals. 
        /// </summary>
        private void HideCalibrationVisuals()
        {
            HideMessage();
            HideCalibrationPoint();
            HideCalibrationBackground();
        }

        /// <summary>
        /// Show calibration outcome to user for a configured time and then exit calibration.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowOutcomeAndExitCalibration(CalibrationOutcome outcome)
        {
            HideCalibrationPoint();

            yield return ShowOutcomeMessageFor(outcome, DisplayResultsDuration);

            ResetView();
            _calibrationFinished.Invoke();
        }

        /// <summary>
        /// Shows the calibration result message.
        /// </summary>
        private IEnumerator ShowOutcomeMessageFor(CalibrationOutcome outcome, float duration)
        {
            // Tell user of the calibration outcome.
            switch (outcome)
            {
                case CalibrationOutcome.Succeeded:
                    ShowMessage(CalibrationSuccessfulMessageText);
                    break;
                case CalibrationOutcome.FailedToComputeModel:
                    ShowMessage(CalibrationErrorFailedToComputeMessageText);
                    break;
                default:
                    ShowMessage(CalibrationErrorUnknownMessageText);
                    break;
            }

            yield return new WaitForSecondsRealtime(duration);

            HideMessage();
        }

        /// <summary>
        /// Resets the calibration view.
        /// </summary>
        private void ResetView()
        {
            // Reset calibration properties.
            HideCalibrationVisuals();

            _currentPointIndex = -1;
            _calibrationPointPivot.transform.localRotation = Quaternion.identity;
            _calibrationRunning = false;
            _calibrationParent.SetActive(false);
        }

        /// <summary>
        /// Triggers the calibration sequence, if possible.
        /// </summary>
        public void TriggerCalibrationAndThen(Action calibrationFinished)
        {
            _calibrationFinished = calibrationFinished;

            if (!_calibrationRunning)
            {
                Debug.Log("Initializing Calibration.");
                _calibrationParent.SetActive(true);
                _calibrationRunning = true;
                StartCoroutine(InitializeCalibration());
            }
            else
            {
                _calibrationFinished.Invoke();
            }
        }

        /// <summary>
        /// Sleeps the current thread as long as the current state is not changed
        /// </summary>
        /// <param name="currentState">Sleep will be maintained until state changes from this state.</param>
        private void SleepUntilStateNoLongerIs(CalibrationState currentState)
        {
            while (_configurationManager.State == currentState)
            {
                Thread.Sleep(10);
            }
        }
    }
}