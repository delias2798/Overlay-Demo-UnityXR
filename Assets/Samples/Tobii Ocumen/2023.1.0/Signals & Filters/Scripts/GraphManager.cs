// Copyright © 2020 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR;
using UnityEngine;

namespace Tobii.Ocumen.Filters.Samples
{
    /// <summary>
    /// Populates the graphs with relevant advanced eye tracking data.
    /// </summary>
    public class GraphManager : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [Header("Graphs")] [SerializeField] private LineChartView pupilDiameterGraph;
        [SerializeField] private LineChartView verticalPositionGuideGraph;
        [SerializeField] private LineChartView horizontalPositionGuideGraph;
        [SerializeField] private LineChartView verticalGazeAngleGraph;
        [SerializeField] private LineChartView horizontalGazeAngleGraph;
        [SerializeField] private LineChartView saccadeGraphLeftEye;
        [SerializeField] private LineChartView saccadeGraphRightEye;
        [SerializeField] private LineChartView velocityGraphLeftEye;
        [SerializeField] private LineChartView velocityGraphRightEye;
        [SerializeField] private LineChartView fixationGraphLeftEye;
        [SerializeField] private LineChartView fixationGraphRightEye;
        [Header("Filters")]
#pragma warning restore 649

        private TobiiXR_AdvancedEyeTrackingData _latestData;
        private readonly CalculateFilters _filterProvider = new CalculateFilters();

        private float _currentTime;
        private bool _isVerticalPositionGuideGraphNull;
        private bool _isHorizontalPositionGuideGraphNull;
        private bool _isPupilDiameterGraphNull;
        private bool _isVerticalGazeAngleGraphNull;
        private bool _isHorizontalGazeAngleGraphNull;
        private bool _isAnyFixationGraphNull;
        private bool _isAnySaccadeGraphNull;
        private bool _isAnyVelocityGraphNull;

        private void Start()
        {
            // Set which graphs are connected only once so that null checks don't happen every update loop (for optimization purposes).

            // Gaze Angles.
            _isHorizontalGazeAngleGraphNull = horizontalGazeAngleGraph == null;
            _isVerticalGazeAngleGraphNull = verticalGazeAngleGraph == null;

            // Pupil Diameter.
            _isPupilDiameterGraphNull = pupilDiameterGraph == null;

            // Position Guide.
            _isHorizontalPositionGuideGraphNull = horizontalPositionGuideGraph == null;
            _isVerticalPositionGuideGraphNull = verticalPositionGuideGraph == null;

            // Eye Movement Classifiers.
            _isAnyVelocityGraphNull = velocityGraphLeftEye == null || velocityGraphRightEye == null;
            _isAnySaccadeGraphNull = saccadeGraphRightEye == null || saccadeGraphLeftEye == null;
            _isAnyFixationGraphNull = fixationGraphLeftEye == null || fixationGraphRightEye == null;
        }

        private void Update()
        {
            _latestData = TobiiXR.Advanced.LatestData;
            _currentTime = Time.time;

            // Update data graphs.
            UpdatePupilDiameterGraph();
            UpdateVerticalPositionGuideGraph();
            UpdateHorizontalPositionGuideGraph();
            UpdateVerticalGazeAngleGraph();
            UpdateHorizontalGazeAngleGraph();
            
            // Update filter graphs.
            _filterProvider.Tick();
            UpdateVelocityFilterGraph(_filterProvider.VelocityLeftEye, _filterProvider.VelocityRightEye);
            UpdateFixationFilterGraph(_filterProvider.FixationDataLeftEye, _filterProvider.FixationDataRightEye);
            UpdateSaccadeFilterGraph(_filterProvider.SaccadeDataLeftEye, _filterProvider.SaccadeDataRightEye);
        }


        /// <summary>
        /// Populate the Pupil Diameter graph with the latest data.
        /// </summary>
        private void UpdatePupilDiameterGraph()
        {
            if (_isPupilDiameterGraphNull) return;

            pupilDiameterGraph.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime,
                _latestData.Left.PupilDiameterValid ? (float?)_latestData.Left.PupilDiameter : null);
            pupilDiameterGraph.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime,
                _latestData.Right.PupilDiameterValid ? (float?)_latestData.Right.PupilDiameter : null);
        }

        /// <summary>
        /// Populate the Vertical Position Guide graph with the latest data.
        /// </summary>
        private void UpdateVerticalPositionGuideGraph()
        {
            if (_isVerticalPositionGuideGraphNull) return;

            verticalPositionGuideGraph.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime,
                _latestData.Left.PositionGuideValid ? (float?)_latestData.Left.PositionGuide.y : null);
            verticalPositionGuideGraph.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime,
                _latestData.Right.PositionGuideValid ? (float?)_latestData.Right.PositionGuide.y : null);
        }

        /// <summary>
        /// Populate the Horizontal Position Guide graph with the latest data.
        /// </summary>
        private void UpdateHorizontalPositionGuideGraph()
        {
            if (_isHorizontalPositionGuideGraphNull) return;

            horizontalPositionGuideGraph.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime,
                _latestData.Left.PositionGuideValid ? (float?)_latestData.Left.PositionGuide.x : null);
            horizontalPositionGuideGraph.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime,
                _latestData.Right.PositionGuideValid ? (float?)_latestData.Right.PositionGuide.x : null);
        }

        /// <summary>
        /// Populate the Vertical Gaze Angle graph with the latest data.
        /// </summary>
        private void UpdateVerticalGazeAngleGraph()
        {
            if (_isVerticalGazeAngleGraphNull) return;

            // Convert the left eye gaze ray from a direction to a vertical angle.
            var leftVerticalDirection =
                new Vector3(0, _latestData.Left.GazeRay.Direction.y, _latestData.Left.GazeRay.Direction.z);
            var leftVerticalAngle = Vector3.SignedAngle(Vector3.forward, leftVerticalDirection, Vector3.left);
            verticalGazeAngleGraph.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime,
                _latestData.Left.GazeRay.IsValid ? (float?)leftVerticalAngle : null);

            // Convert the right eye gaze ray from a direction to a vertical angle.
            var rightVerticalDirection = new Vector3(0, _latestData.Right.GazeRay.Direction.y,
                _latestData.Right.GazeRay.Direction.z);
            var rightVerticalAngle = Vector3.SignedAngle(Vector3.forward, rightVerticalDirection, Vector3.left);
            verticalGazeAngleGraph.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime,
                _latestData.Right.GazeRay.IsValid ? (float?)rightVerticalAngle : null);
        }

        /// <summary>
        /// Populate the Horizontal Gaze Angle graph with the latest data.
        /// </summary>
        private void UpdateHorizontalGazeAngleGraph()
        {
            if (_isHorizontalGazeAngleGraphNull) return;

            // Convert the left eye gaze ray from a direction to a horizontal angle.
            var leftHorizontalDirection = new Vector3(_latestData.Left.GazeRay.Direction.x, 0,
                _latestData.Left.GazeRay.Direction.z);
            var leftHorizontalAngle = Vector3.SignedAngle(Vector3.forward, leftHorizontalDirection, Vector3.up);
            horizontalGazeAngleGraph.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime,
                _latestData.Left.GazeRay.IsValid ? (float?)leftHorizontalAngle : null);

            // Convert the right eye gaze ray from a direction to a horizontal angle.
            var rightHorizontalDirection = new Vector3(_latestData.Right.GazeRay.Direction.x, 0,
                _latestData.Right.GazeRay.Direction.z);
            var rightHorizontalAngle = Vector3.SignedAngle(Vector3.forward, rightHorizontalDirection, Vector3.up);
            horizontalGazeAngleGraph.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime,
                _latestData.Right.GazeRay.IsValid ? (float?)rightHorizontalAngle : null);
        }

        /// <summary>
        /// Populate the Saccade Filter graph with the latest data.
        /// </summary>
        private void UpdateSaccadeFilterGraph(float? saccadeLeftEye, float? saccadeRightEye)
        {
            if (_isAnySaccadeGraphNull) return;

            // Add data for the graph renderer.
            saccadeGraphLeftEye.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime, saccadeLeftEye);
            saccadeGraphRightEye.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime, saccadeRightEye);
        }

        /// <summary>
        /// Populate the Velocity Filter graph with the latest data.
        /// </summary>
        private void UpdateVelocityFilterGraph(float? velocityLeftEye, float? velocityRightEye)
        {
            if (_isAnyVelocityGraphNull) return;

            // Add data for the graph renderer.
            velocityGraphLeftEye.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime, velocityLeftEye);
            velocityGraphRightEye.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime, velocityRightEye);
        }

        /// <summary>
        /// Populate the Fixation Filter graph with the latest data.
        /// </summary>
        private void UpdateFixationFilterGraph(float? fixationLeftEye, float? fixationRightEye)
        {
            if (_isAnyFixationGraphNull) return;

            // Add data for the graph renderer.
            fixationGraphLeftEye.AddSampleToLine(LineChartView.DataLine.Line1, _currentTime, fixationLeftEye);
            fixationGraphRightEye.AddSampleToLine(LineChartView.DataLine.Line2, _currentTime, fixationRightEye);
        }

    }
}