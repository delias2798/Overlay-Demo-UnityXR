using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static System.FormattableString;

namespace Tobii.Ocumen.Configuration.Samples
{
    public class SpawnCalibrationQualityPointCloud : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private GameObject biasAndPrecisionVisualizationPrefab;
        [SerializeField] private int standardDeviations = 1;
        [SerializeField] private Eye eye;
        [SerializeField] private Color biasColor;
        [SerializeField] private Color precisionColor;
        [SerializeField] private Color invalidOrUnusedStimulusColor;
        [SerializeField] private bool inWorldUI;
        [SerializeField] private Collider killBoxCollider;
        [SerializeField] private Transform worldUILabelMask;
#pragma warning restore 649

        private readonly List<GameObject> _points = new List<GameObject>();
        private List<CalibrationPointData> _calibrationMetrics;

        private const float InvalidStimulusRotation = 45 * Mathf.Deg2Rad;
        private const float InvalidStimulusSize = 0.06f;

        private void Awake()
        {
            _calibrationMetrics = ConfigurationManager.GetCalibrationMetrics();
            ShowCalibrationMetrics(_calibrationMetrics);
        }

        private void ShowCalibrationMetrics(List<CalibrationPointData> calibrationMetrics)
        {
            foreach (var calibrationMetric in calibrationMetrics)
            {
                var currentEye = eye == Eye.Left ? calibrationMetric.Left : calibrationMetric.Right;

                // Create point. 
                var point = Instantiate(biasAndPrecisionVisualizationPrefab, transform);

                // Set position. If in 2D, adapt to 2D plane. 
                var pointPosition = calibrationMetric.Point;
                point.transform.localPosition = inWorldUI
                    ? (pointPosition / pointPosition.z) * -transform.localPosition.z
                    : pointPosition;

                // Rotate the points toward the camera for the points not used in the world UI.
                if (!inWorldUI)
                    point.transform.rotation = Quaternion.LookRotation(point.transform.position - Vector3.zero);

                _points.Add(point);

                // Get and calculate values needed for bias and precision.
                var bias = currentEye.BiasRad;
                var precision = currentEye.PrecisionRad * standardDeviations;
                var biasCircleRadius = GetRadius(bias, calibrationMetric.Point.magnitude);
                var precisionCircleRadius = GetRadius(precision, calibrationMetric.Point.magnitude);
                var biasPlusPrecisionCircleRadius = GetRadius(precision + bias, calibrationMetric.Point.magnitude);

                // Get values for valid and used.
                var valid = currentEye.Valid;
                var used = currentEye.Used;

                // Find particle components.
                var particlesParent = point.transform.Find("Particles");
                var biasParticles =
                    particlesParent.Find("Particles Bias").gameObject.GetComponent<ParticleSystem>();
                var precisionParticles = particlesParent.Find("Particles Precision").gameObject
                    .GetComponent<ParticleSystem>();
                var stimulusParticle =
                    particlesParent.Find("Particle Stimulus").gameObject.GetComponent<ParticleSystem>();

                // Set bias values.
                var biasParticlesShape = biasParticles.shape;
                biasParticlesShape.radius = valid ? biasCircleRadius : 0f;

                // Set precision values.
                var precisionParticlesShape = precisionParticles.shape;
                precisionParticlesShape.radius = valid ? biasPlusPrecisionCircleRadius : 0f;
                precisionParticlesShape.radiusThickness =
                    valid ? (precisionCircleRadius * 2) / biasPlusPrecisionCircleRadius : 0f;

                // Dynamically set number of spawned precision particles depending on the circle size.
                var rateMultiplier = 5000f;
                var rateOverTime = biasPlusPrecisionCircleRadius * rateMultiplier;
                var particleEmission = precisionParticles.emission;
                particleEmission.rateOverTime = rateOverTime;

                // Set stimulus point values to create a red X if the point is not used.
                if (!used)
                {
                    var stimulusParticleMain = stimulusParticle.main;
                    stimulusParticleMain.startRotation = InvalidStimulusRotation;
                    stimulusParticleMain.startColor = invalidOrUnusedStimulusColor;
                    stimulusParticleMain.startSize = InvalidStimulusSize;
                }

                // Set label text.
                SetTextLabel(point, bias, precision, valid, used);

                // Mask particles and labels if we are in the world UI
                if (inWorldUI)
                {
                    MaskParticles(biasParticles);
                    MaskParticles(precisionParticles);
                    MaskParticles(stimulusParticle);
                    
                    var canvas = point.GetComponentInChildren<Canvas>();
                    var pointLabel = canvas.transform.Find("Point Label");
                    pointLabel.SetParent(worldUILabelMask, true);
                }
            }
        }

        private void SetTextLabel(GameObject point, float bias, float precision, bool isValid, bool isUsed)
        {
            var pointLabel = point.transform.Find("Canvas/Point Label");
            var precisionText = pointLabel.Find("Precision Text").GetComponent<TMP_Text>();
            var validImage = pointLabel.transform.Find("Valid Image");
            var invalidImage = pointLabel.Find("Invalid Image");
            var usedImage = pointLabel.Find("Used Image");
            var unusedImage = pointLabel.Find("Unused Image");
            var biasInDegrees = bias * Mathf.Rad2Deg;
            var precisionInDegrees = precision * Mathf.Rad2Deg;

            var biasColorString = ColorUtility.ToHtmlStringRGBA(biasColor);
            var precisionColorString = ColorUtility.ToHtmlStringRGBA(precisionColor);
            var stimuliColorString = ColorUtility.ToHtmlStringRGBA(invalidOrUnusedStimulusColor);

            precisionText.text = isValid
                ? $"<color=#{biasColorString}>{Invariant($"{biasInDegrees:F2}")}°</color> ± " +
                  $"<color=#{precisionColorString}>{Invariant($"{precisionInDegrees:F2}")}°</color>"
                : $"<color=#{stimuliColorString}>-</color> ± <color=#{stimuliColorString}>-</color>";

            validImage.gameObject.SetActive(isValid);
            invalidImage.gameObject.SetActive(!isValid);

            usedImage.gameObject.SetActive(isUsed);
            unusedImage.gameObject.SetActive(!isUsed);
        }

        private void MaskParticles(ParticleSystem particleSystem)
        {
            var particlesTrigger = particleSystem.trigger;
            particlesTrigger.enabled = true;
            particlesTrigger.SetCollider(0, killBoxCollider);
        }

        public void ChangePrecisionStandardDeviation(int precisionStandardDeviation)
        {
            // Remove previous points.
            foreach (var p in _points)
            {
                Destroy(p);
            }

            _points.Clear();

            // Clean up all current labels.
            foreach (Transform label in worldUILabelMask.transform)
            {
                Destroy(label.gameObject);
            }

            // Set the new precision standard deviation
            standardDeviations = precisionStandardDeviation;

            // Add new points with new precision standard deviation.
            ShowCalibrationMetrics(_calibrationMetrics);
        }

        /// <summary>
        /// Returns the radius of a circle given the distance to its center and the angle to its edge in radians.
        /// </summary>
        /// <param name="angle">The angle to the outer edge of the circle in radians.</param>
        /// <param name="distance">The distance to the circle center.</param>
        /// <returns>Circle radius in meters.</returns>
        private static float GetRadius(float angle, float distance)
        {
            return Mathf.Tan(angle) * distance;
        }
    }
}