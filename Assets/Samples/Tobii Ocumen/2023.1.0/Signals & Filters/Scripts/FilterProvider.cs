using System;
using System.Collections.Generic;
using System.Linq;
using Tobii.XR;
using Tobii.Ocumen.Common;

namespace Tobii.Ocumen.Filters.Samples
{
    public class CalculateFilters
    {
        public float? VelocityLeftEye { get; private set; }
        public float? VelocityRightEye { get; private set; }
        public float? FixationDataRightEye { get; private set; }
        public float? FixationDataLeftEye { get; private set; }
        public float? SaccadeDataLeftEye { get; private set; }
        public float? SaccadeDataRightEye { get; private set; }

        // Time window for how much data is used to calculate filter data
        private const long TimeWindowMicroseconds = 1_000_000;
        
        private readonly SortedList<long, BinocularGaze> _gazeData = new SortedList<long, BinocularGaze>();

        private SaccadeOutput[] _saccadeOutput = new SaccadeOutput[50];
        private FixationOutput[] _fixationOutput = new FixationOutput[50];
        private VelocityOutput[] _velocityOutput = new VelocityOutput[50];
        
        public void Tick()
        {
            // Get gaze data from eye tracker and store it in a list.
            for (var i = 0; i < TobiiXR.Advanced.QueuedData.Count; i++)
            {
                var d = TobiiXR.Advanced.QueuedData.Dequeue();
                var binocularGaze = ConvertToBinocularGaze(d);
                _gazeData.Add(d.DeviceTimestamp, binocularGaze);    
            }

            // Ensure there is data to calculate filters.
            if(_gazeData.Count == 0) return;

            // Trim data outside of time window.
            TrimData();

            // Convert lists to arrays to be used when fetching filter data.
            var timesArray = _gazeData.Keys.ToArray();
            var binocularArray = _gazeData.Values.ToArray();

            // Velocity Data.
            var velocityOutput = GetVelocityOutput(timesArray, binocularArray, ref _velocityOutput);
            AddVelocityData(velocityOutput);

            // Fixation Data.
            var fixationOutput = GetFixationOutput(timesArray, velocityOutput, ref _fixationOutput);
            AddFixationData(fixationOutput);

            // Saccade Data.
            var saccadeOutput = GetSaccadeOutput(timesArray, velocityOutput, ref _saccadeOutput);
            AddSaccadeData(saccadeOutput);
        }

        /// Trim data outside the time window.
        private void TrimData()
        {
            var lastTime = _gazeData.Keys.Last();
            for (var i = _gazeData.Keys.Count - 1; i >= 0; i--)
            {
                var time = _gazeData.Keys[i];
                if (time < lastTime - TimeWindowMicroseconds)
                {
                    _gazeData.Remove(time);
                }
            }
        }

        /// Add the velocity data to the public properties of this class.
        private void AddVelocityData(VelocityOutput[] velocityOutput)
        {
            // Get the velocity data for each eye using the value in the middle of the output list.
            var velocity = velocityOutput[velocityOutput.Length / 2]; 
            var leftEyeVelocity = velocity.left.ToNullable();
            var rightEyeVelocity = velocity.right.ToNullable();

            // Set velocity data for the left eye.
            VelocityLeftEye = null;
            if (leftEyeVelocity.HasValue)
            {
                var value = leftEyeVelocity.Value.localAngularSpeedDeg;
                VelocityLeftEye = float.IsNaN(value) ? (float?)null : value;
            }

            // Set velocity data for right eye.
            VelocityRightEye = null;
            if (rightEyeVelocity.HasValue)
            {
                var value = rightEyeVelocity.Value.localAngularSpeedDeg;
                VelocityRightEye = float.IsNaN(value) ? (float?)null : value;
            }
        }

        /// Add the fixation data to the public properties of this class.
        private void AddFixationData(FixationOutput[] fixationOutput)
        {
            // Get the fixation data for each eye using the value in the middle of the output list.
            var fixationData = fixationOutput[fixationOutput.Length / 2];

            // Set fixation values for left and right eye.
            FixationDataLeftEye = ToFloat(fixationData.isFixationLeft.ToNullable());
            FixationDataRightEye = ToFloat(fixationData.isFixationRight.ToNullable());
        }
        
        /// Add the saccade data to the public properties of this class.
        private void AddSaccadeData(SaccadeOutput[] saccadeOutput)
        {
            // Get the saccade data for each eye using the value in the middle of the output list.
            var saccadeData = saccadeOutput[saccadeOutput.Length / 2];

            // Set saccade values for left and right eye.
            SaccadeDataLeftEye = ToFloat(saccadeData.isSaccadeLeft.ToNullable());
            SaccadeDataRightEye = ToFloat(saccadeData.isSaccadeRight.ToNullable());
        }

        /// Query the filter API for velocity data using the Velocities Gaze Instantaneous filter.
        private static VelocityOutput[] GetVelocityOutput(long[] timesArray, BinocularGaze[] binocularGazeArray, ref VelocityOutput[] output)
        {
            Array.Resize(ref output, timesArray.Length);
            
            Interop.VelocityInstantaneous(timesArray, binocularGazeArray, output);
            return output;
        }

        /// Query the filter API for fixation data using the Fixation Dispersion Angles filter.
        private static FixationOutput[] GetFixationOutput(long[] timesArray, VelocityOutput[] velocityOutput, ref FixationOutput[] output)
        {
            Array.Resize(ref output, timesArray.Length);
            
            // Filter configuration.
            var config = new DispersionAnglesFilterConfig
            {
                maxAngleForFixationsDeg = 3.0f,
                minDurationForFixationUs = 100_000,
                maxOutliers = 1
            };

            Interop.FixationDispersionAngles(config, timesArray, velocityOutput, output);

            return output;
        }

        /// Query the filter API for saccade data using the Saccade Smeets and Hooge 2003 filter.
        private static SaccadeOutput[] GetSaccadeOutput(long[] timesArray, VelocityOutput[] velocityOutput, ref SaccadeOutput[] output)
        {
            Array.Resize(ref output, timesArray.Length);
            
            // Filter configuration.
            var config = new SmeetsHoogeFilterConfig
            {
                lowerThresholdDegPerSec = 75f,
                fixationSigmaThreshold = 3,
                fixationVelocityWindowStartUs = 200_000,
                fixationVelocityWindowSizeUs = 100_000,
                fixationVelocityMinimumDegPerSec = 5,
                earlyPeakLimit = 0.25f,
                latePeakLimit = 0.75f,
            };
            
            Interop.SaccadeSmeetsHooge(config, timesArray, velocityOutput, output);
            
            return output;
        }

        /// Convert nullable bool to nullable binary float value. 
        private static float? ToFloat(bool? b)
        {
            if (!b.HasValue) return null;
            
            return b.Value ? 1.0f : 0.0f;
        }
        
        /// Convert TobiiXR Advanced Eye Tracking Data to Binocular Gaze.
        private static BinocularGaze ConvertToBinocularGaze(TobiiXR_AdvancedEyeTrackingData latestData)
        {
            return new BinocularGaze
            {
                left = Rayf32.From(latestData.Left.GazeRay),
                right = Rayf32.From(latestData.Right.GazeRay)
            };
        }
    }
}