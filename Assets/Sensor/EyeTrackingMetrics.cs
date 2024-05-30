using Tobii.XR;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class EyeTrackingMetrics : MonoBehaviour
{
    private List<MetricData> metricsDataList = new List<MetricData>();

    void Update()
    {
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

        if (eyeTrackingData.GazeRay.IsValid)
        {
            string focusedObjectName = "None";
            Vector3? hitPoint = null;

            if (TobiiXR.FocusedObjects.Count > 0)
            {
                focusedObjectName = TobiiXR.FocusedObjects[0].GameObject.name;
            }

            Ray ray = new Ray(eyeTrackingData.GazeRay.Origin, eyeTrackingData.GazeRay.Direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                hitPoint = hit.point;
            }

            MetricData metricData = new MetricData
            {
                Timestamp = Time.time,
                GazeOrigin = eyeTrackingData.GazeRay.Origin,
                GazeDirection = eyeTrackingData.GazeRay.Direction,
                // LeftEyeOpenness = eyeTrackingData.,
                // RightEyeOpenness = eyeTrackingData.EyeOpenness.Right,
                IsLeftEyeBlinking = eyeTrackingData.IsLeftEyeBlinking,
                IsRightEyeBlinking = eyeTrackingData.IsRightEyeBlinking,
                FocusedObjectName = focusedObjectName,
                HitPoint = hitPoint.HasValue ? hitPoint.Value : Vector3.zero
            };

            metricsDataList.Add(metricData);
        }
    }

    void OnApplicationQuit()
    {
        // Save or process the recorded data as needed
        SaveMetricsData();
    }

    public void SaveMetricsData()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = "EyeTrackingMetrics_" + timestamp + ".json";
        string filePath = Path.Combine(Application.persistentDataPath, filename);

        // Convert the list of metrics data to JSON format
        string jsonData = JsonUtility.ToJson(new MetricsDataList { Metrics = metricsDataList }, true);

        // Write the JSON data to a file
        File.WriteAllText(filePath, jsonData);

        Debug.Log("Metrics data saved to: " + filePath);
    }
}

[Serializable]
public class MetricData
{
    public float Timestamp;
    public Vector3 GazeOrigin;
    public Vector3 GazeDirection;
    public bool IsLeftEyeBlinking;
    public bool IsRightEyeBlinking;
    public string FocusedObjectName;
    public Vector3 HitPoint;
}

[Serializable]
public class MetricsDataList
{
    public List<MetricData> Metrics;
}
