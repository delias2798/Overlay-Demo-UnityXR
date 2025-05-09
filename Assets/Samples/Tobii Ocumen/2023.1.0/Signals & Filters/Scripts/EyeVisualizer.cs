// Copyright © 2020 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR;
using UnityEngine;

namespace Tobii.Ocumen.Filters.Samples
{
    public class EyeVisualizer : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [Header("Top View")] [SerializeField] private Transform topViewLeftEye;
        [SerializeField] private Transform topViewRightEye;

        [Header("Side View")] [SerializeField] private Transform sideViewLeftEye;
        [SerializeField] private Transform sideViewRightEye;

        [Header("3D View")] [SerializeField] private Transform frontViewLeftEye;
        [SerializeField] private Transform frontViewRightEye;
        [SerializeField] private Transform leftPupil;
        [SerializeField] private Transform rightPupil;
        [SerializeField] private Transform hmd;
        [SerializeField] private Transform hmdPivotPoint;
        [SerializeField] private Transform leftPositionGuidePoint;
        [SerializeField] private Transform rightPositionGuidePoint;

        [Header("Pupil View")] [SerializeField]
        private Transform leftScaledPupil;

        [SerializeField] private Transform rightScaledPupil;

        [Header("Customization")] [SerializeField, Tooltip("Movement offset for the position guide signal.")]
        private float hmdMovementFactor = 0.015f;

        [SerializeField] private float pupilScaleFactor = 70f;
#pragma warning restore 649

        private TobiiXR_AdvancedEyeTrackingData _latestData;
        private Vector3 _leftPositionGuidePointStartPosition;
        private Vector3 _rightPositionGuidePointStartPosition;
        private bool _leftEyeHidden;
        private bool _rightEyeHidden;

        private void Start()
        {
            _leftPositionGuidePointStartPosition = leftPositionGuidePoint.localPosition;
            _rightPositionGuidePointStartPosition = rightPositionGuidePoint.localPosition;
        }

        private void Update()
        {
            _latestData = TobiiXR.Advanced.LatestData;

            RotateEye(frontViewLeftEye, topViewLeftEye, sideViewLeftEye, _latestData.Left);
            RotateEye(frontViewRightEye, topViewRightEye, sideViewRightEye, _latestData.Right);
            SetPerEyePupilDilation(leftPupil, leftScaledPupil, _latestData.Left);
            SetPerEyePupilDilation(rightPupil, rightScaledPupil, _latestData.Right);
            MoveHmd();
            Blink();
        }

        /// <summary>
        /// Rotate the transforms of an eye given a direction.
        /// </summary>
        /// <param name="frontView">Front view transform of an eye.</param>
        /// <param name="topView">Top view transform of an eye.</param>
        /// <param name="sideView">Side view transform of an eye.</param>
        /// <param name="eye">Eye data to use for rotation.</param>
        private static void RotateEye(Transform frontView, Transform topView, Transform sideView,
            TobiiXR_AdvancedPerEyeData eye)
        {
            if (!eye.GazeRay.IsValid) return;

            var newRotation = Quaternion.LookRotation(eye.GazeRay.Direction, Vector3.up);

            frontView.localRotation = newRotation;
            topView.localEulerAngles = new Vector3(0, newRotation.eulerAngles.y, 0);
            sideView.localEulerAngles = new Vector3(newRotation.eulerAngles.x, 0, 0);
        }

        /// <summary>
        /// Set per eye pupil dilation visualization. 
        /// </summary>
        /// <param name="pupil">3D pupil transform.</param>
        /// <param name="scaledPupil">2D pupil transform.</param>
        /// <param name="eyeData">Eye data from where to get pupil diameter info.</param>
        private void SetPerEyePupilDilation(Transform pupil, Transform scaledPupil, TobiiXR_AdvancedPerEyeData eyeData)
        {
            if (!eyeData.PupilDiameterValid) return;

            var pupilScaleInMeters = eyeData.PupilDiameter / 1000f;

            // Scale the 3D pupil, cylinder is rotated so y stays constant.
            pupil.localScale = new Vector3(pupilScaleInMeters, pupil.localScale.y, pupilScaleInMeters);

            // Scale the 2D pupil.
            scaledPupil.localScale = new Vector3(pupilScaleInMeters * pupilScaleFactor,
                pupilScaleInMeters * pupilScaleFactor, scaledPupil.localScale.z);
        }

        /// <summary>
        /// Set position guide position for an eye.
        /// </summary>
        /// <param name="positionGuidePoint">Transform to set position of.</param>
        /// <param name="startPosition">Initial position.</param>
        /// <param name="eye">Eye from which to get position guide data from.</param>
        private void SetPositionGuidePoint(Transform positionGuidePoint, Vector3 startPosition,
            TobiiXR_AdvancedPerEyeData eye)
        {
            if (!eye.PositionGuideValid) return;

            // Translate offset point from (0 to 1) to (-0.5 to 0.5).
            var newPositionX = -eye.PositionGuide.x + 0.5f;
            var newPositionY = eye.PositionGuide.y - 0.5f;

            positionGuidePoint.localPosition =
                startPosition + new Vector3(newPositionX, newPositionY, 0) * hmdMovementFactor;
        }

        /// <summary>
        /// Position and rotate the HMD visualization depending on where the user's eyes are in the HMD.
        /// </summary>
        private void MoveHmd()
        {
            // Update the left position guide transform point, determined by the offset of the position guide signal.
            SetPositionGuidePoint(leftPositionGuidePoint, _leftPositionGuidePointStartPosition, _latestData.Left);
            SetPositionGuidePoint(rightPositionGuidePoint, _rightPositionGuidePointStartPosition, _latestData.Right);

            // Set rotation and position of HMD visualization if there are valid signals from both eyes.
            if (_latestData.Left.PositionGuideValid && _latestData.Right.PositionGuideValid)
            {
                // Reset HMD pivot point (used when position guide signal from only one eye is valid).
                hmdPivotPoint.localPosition = Vector3.zero;

                // Set the position of the HMD visualization to the average of the left and right position guide point.
                var hmdCenter = (rightPositionGuidePoint.localPosition + leftPositionGuidePoint.localPosition) / 2f;
                hmd.localPosition = hmdCenter;

                // Set the rotation of the HMD visualization.
                var vectorTowardsRightPoint = rightPositionGuidePoint.position - leftPositionGuidePoint.position;
                hmd.right = vectorTowardsRightPoint;

                // Prevent the rotation from going off axis in z.
                hmd.localEulerAngles = new Vector3(0, 0, hmd.transform.localEulerAngles.z);
            }
            // Set position of the HMD visualization when there is only a valid signal from one eye.
            else if (_latestData.Left.PositionGuideValid)
            {
                hmd.localPosition = leftPositionGuidePoint.localPosition;
                hmdPivotPoint.localPosition = new Vector3(-_leftPositionGuidePointStartPosition.x, 0, 0);
            }
            else if (_latestData.Right.PositionGuideValid)
            {
                hmd.localPosition = rightPositionGuidePoint.localPosition;
                hmdPivotPoint.localPosition = new Vector3(_leftPositionGuidePointStartPosition.x, 0, 0);
            }
        }

        /// <summary>
        /// Hide eyeball visualizations when the user blinks with that eye.
        /// </summary>
        private void Blink()
        {
            // Left eye.
            frontViewLeftEye.gameObject.SetActive(!_latestData.Left.IsBlinking);
            topViewLeftEye.gameObject.SetActive(!_latestData.Left.IsBlinking);
            sideViewLeftEye.gameObject.SetActive(!_latestData.Left.IsBlinking);
            leftScaledPupil.gameObject.SetActive(!_latestData.Left.IsBlinking);

            // Right eye.
            frontViewRightEye.gameObject.SetActive(!_latestData.Right.IsBlinking);
            topViewRightEye.gameObject.SetActive(!_latestData.Right.IsBlinking);
            sideViewRightEye.gameObject.SetActive(!_latestData.Right.IsBlinking);
            rightScaledPupil.gameObject.SetActive(!_latestData.Right.IsBlinking);
        }
    }
}