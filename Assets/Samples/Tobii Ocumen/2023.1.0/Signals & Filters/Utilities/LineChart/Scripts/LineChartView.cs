// Copyright © 2020 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plots a line chart when fed data samples.
/// </summary>
public class LineChartView : MonoBehaviour
{
#pragma warning disable 649 // Field is never assigned
    [SerializeField] private string graphName = "Graph Name";
    [SerializeField] private string xLabel = "X Axis Label";
    [SerializeField] private string yLabel = "Y Axis Label";

    [SerializeField, Tooltip("The width of the rendered data line.")]
    private float lineWidth = 0.003f;

    [SerializeField, Tooltip("Time in seconds for the x-axis.")]
    private float timeWindow = 1.0f;

    [SerializeField, Tooltip("The minimum y-value, this can change the minimum y-value is not locked.")]
    private float yMinStart = 0.0f;

    [SerializeField, Tooltip("Determines if the min y-value can change if the signal's value is lower than the current min y-value.")]
    private bool yMinLocked = false;

    [SerializeField, Tooltip("The maximum y-value, this can change the maximum y-value is not locked")]
    private float yMaxStart = 1.0f;

    [SerializeField, Tooltip("Determines if the max y-value can change if the signal's value is higher than the current max y-value.")]
    private bool yMaxLocked = false;
#pragma warning restore 649

    private static readonly List<Vector3> TempVertices = new List<Vector3>(1000);
    private static readonly List<int> TempTriangles = new List<int>(100);

    private Text _yMinText;
    private Text _yMaxText;
    private Text _xMaxText;
    private MeshFilter _meshFilter1;
    private MeshFilter _meshFilter2;
    private MeshFilter _meshFilter3;
    private readonly LinkedList<Sample> _samples1 = new LinkedList<Sample>();
    private readonly LinkedList<Sample> _samples2 = new LinkedList<Sample>();
    private readonly LinkedList<Sample> _samples3 = new LinkedList<Sample>();
    private float _yMin;
    private float _yMax;

    /// <summary>
    /// Adds a data sample to a specific graph line.
    /// </summary>
    /// <param name="dataLine">Decides which mesh (line) should render this data point</param>
    /// <param name="time">The time the sample was collected (x-value)</param>
    /// <param name="value">The value of the sample (y-value)</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void AddSampleToLine(DataLine dataLine, float time, float? value)
    {
        // Update the y min/max values if the latest data value is above/below it, unless it's set to be locked.
        if (!yMaxLocked && value > _yMax) UpdateYMax(value.GetValueOrDefault());
        if (!yMinLocked && value < _yMin) UpdateYMin(value.GetValueOrDefault());

        switch (dataLine)
        {
            case DataLine.Line1:
                _samples1.AddFirst(new Sample {Time = time, Value = value});
                break;
            case DataLine.Line2:
                _samples2.AddFirst(new Sample {Time = time, Value = value});
                break;
            case DataLine.Line3:
                _samples3.AddFirst(new Sample {Time = time, Value = value});
                break;
            default:
                throw new ArgumentOutOfRangeException("dataLine", dataLine, null);
        }
    }

    /// <summary>
    /// Updates the y max value and label.
    /// </summary>
    /// <param name="value"></param>
    private void UpdateYMax(float value)
    {
        _yMax = value;
        _yMaxText.text = value.ToString("F2");
    }

    /// <summary>
    /// Updates the y min value and label.
    /// </summary>
    /// <param name="value"></param>
    private void UpdateYMin(float value)
    {
        _yMin = value;
        _yMinText.text = value.ToString("F2");
    }

    private void Awake()
    {
        // Get text fields.
        var canvas = GetComponentInChildren<Canvas>();
        var yAxis = canvas.transform.Find("Y");
        var xAxis = canvas.transform.Find("X");
        _yMinText = yAxis.Find("Y_Min").GetComponent<Text>();
        _yMaxText = yAxis.Find("Y_Max").GetComponent<Text>();
        _xMaxText = xAxis.Find("X_Max").GetComponent<Text>();

        // Get mesh filters for data lines.
        var dataTransform = transform.Find("Data");
        _meshFilter1 = dataTransform.Find("Line 1").GetComponent<MeshFilter>();
        _meshFilter2 = dataTransform.Find("Line 2").GetComponent<MeshFilter>();
        _meshFilter3 = dataTransform.Find("Line 3").GetComponent<MeshFilter>();

        // Set graph labels.
        canvas.transform.Find("Graph Title").GetComponent<Text>().text = graphName;
        xAxis.Find("X Label").GetComponent<Text>().text = xLabel;
        yAxis.Find("Y Label").GetComponent<Text>().text = yLabel;

        // Set text for x max.
        _xMaxText.text = timeWindow.ToString("F2");

        // Update y min and max.
        UpdateYMin(yMinStart);
        UpdateYMax(yMaxStart);
    }

    private void Update()
    {
        var endTime = Time.time;
        var startTime = endTime - timeWindow;

        // Render the lines for which there is enough data samples.
        if (_samples1.Count > 1) UpdateMeshWith(_meshFilter1.mesh, _yMin, _yMax, _samples1, startTime, timeWindow, lineWidth);
        if (_samples2.Count > 1) UpdateMeshWith(_meshFilter2.mesh, _yMin, _yMax, _samples2, startTime, timeWindow, lineWidth);
        if (_samples3.Count > 1) UpdateMeshWith(_meshFilter3.mesh, _yMin, _yMax, _samples3, startTime, timeWindow, lineWidth);
    }

    /// <summary>
    /// Fills a mesh with vertices to create a line representing a time window of the provided samples.
    /// </summary>
    /// <param name="mesh">The mesh to fill with vertices</param>
    /// <param name="yMin">Y min value for the Line Chart</param>
    /// <param name="yMax">Y max value for the Line Chart</param>
    /// <param name="samples">The collection of time and data points to be rendered</param>
    /// <param name="startTime">The start time of the Line Chart</param>
    /// <param name="timeWindow">The amount of samples to be rendered in a time window (length of x-axis)</param>
    /// <param name="lineWidth">The width of the rendered line</param>
    private static void UpdateMeshWith(Mesh mesh, float yMin, float yMax, LinkedList<Sample> samples, float startTime, float timeWindow, float lineWidth)
    {
        // Scale the mesh on the Y axis to be able to contain all values between yMin to yMax.
        var yScale = 1 / (yMax - yMin);

        // Create all the vertices for the line graph.
        TempVertices.Clear();
        var prev = samples.First;
        var current = prev.Next;
        while (current != null)
        {
            // Early out if values are too old to fit inside window.
            if (current.Value.Time < startTime)
            {
                while (samples.Last != current) samples.RemoveLast();
                samples.Remove(current);
                break;
            }

            // Skip rendering parts of the mesh where there is no valid data.
            if (!prev.Value.Value.HasValue || !current.Value.Value.HasValue)
            {
                // Prep next iteration.
                prev = current;
                current = current.Next;
                continue;
            }

            // Calculate start and end of line segment.
            var x1 = 1.0f - (prev.Value.Time - startTime) / timeWindow;
            var x2 = 1.0f - (current.Value.Time - startTime) / timeWindow;
            var y1 = Mathf.Clamp(prev.Value.Value.GetValueOrDefault() - yMin, 0f, (yMax - yMin)) * yScale;
            var y2 = Mathf.Clamp(current.Value.Value.GetValueOrDefault() - yMin, 0f, (yMax - yMin)) * yScale;
            var p1 = new Vector2(x1, y1);
            var p2 = new Vector2(x2, y2);

            // Calculate the 2 normals for the line between x1y1 to x2y2.
            var v = (p2 - p1).normalized;
            var n1 = new Vector2(-v.y, v.x).normalized * lineWidth;
            var n2 = new Vector2(v.y, -v.x).normalized * lineWidth;

            // Create vertices for the line segment.
            TempVertices.Add(new Vector3(-0.5f + p1.x + n1.x, -0.5f + p1.y + n1.y, 0.0f));
            TempVertices.Add(new Vector3(-0.5f + p1.x + n2.x, -0.5f + p1.y + n2.y, 0.0f));
            TempVertices.Add(new Vector3(-0.5f + p2.x + n1.x, -0.5f + p2.y + n1.y, 0.0f));
            TempVertices.Add(new Vector3(-0.5f + p2.x + n2.x, -0.5f + p2.y + n2.y, 0.0f));

            // Create vertices for the end caps.
            var cap1 = p1 - v * lineWidth;
            var cap2 = p2 + v * lineWidth;
            TempVertices.Add(new Vector3(-0.5f + cap1.x, -0.5f + cap1.y, 0.0f));
            TempVertices.Add(new Vector3(-0.5f + cap2.x, -0.5f + cap2.y, 0.0f));

            // Prep next iteration.
            prev = current;
            current = current.Next;
        }

        // Map vertices into triangles.
        TempTriangles.Clear();
        for (var i = 0; i < TempVertices.Count / 6; i++)
        {
            // Each line has 6 vertices.
            var p = i * 6;

            // Line segment.
            TempTriangles.Add(p + 0);
            TempTriangles.Add(p + 2);
            TempTriangles.Add(p + 1);

            TempTriangles.Add(p + 2);
            TempTriangles.Add(p + 3);
            TempTriangles.Add(p + 1);

            // End cap 1.
            TempTriangles.Add(p + 0);
            TempTriangles.Add(p + 1);
            TempTriangles.Add(p + 4);

            // End cap 2.
            TempTriangles.Add(p + 2);
            TempTriangles.Add(p + 5);
            TempTriangles.Add(p + 3);
        }

        // Add vertices to mesh.
        mesh.Clear();
        mesh.SetVertices(TempVertices);
        mesh.SetTriangles(TempTriangles, 0);
    }

    private struct Sample
    {
        public float Time;
        public float? Value;
    }

    public enum DataLine
    {
        Line1,
        Line2,
        Line3
    }
}