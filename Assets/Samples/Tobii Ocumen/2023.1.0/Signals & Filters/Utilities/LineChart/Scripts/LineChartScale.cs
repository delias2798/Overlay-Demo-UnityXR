// Copyright © 2020 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script automatically scales the Line Chart to keep text clearly visible and to minimize aliasing when running in VR HMDs.
/// Below values are made for Line Charts situated at roughly 2 meters from the camera. 
/// </summary>
[ExecuteInEditMode]
public class LineChartScale : MonoBehaviour
{
#pragma warning disable 649 // Field is never assigned
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Transform data;
    [SerializeField] private Text[] texts;
    [SerializeField] private float graphWidth = 170f;
    [SerializeField] private float graphHeight = 120f;
    [SerializeField] private int titleFontSize = 8;
    [SerializeField] private int labelFontSize = 7;
#pragma warning restore 649

    private const float DataPadding = 0.8f;
    private const float CanvasScale = 0.005f;

    void Update()
    {
        // Lock parent transform scale to avoid messing up text quality.
        transform.localScale = Vector3.one;

        // Set canvas scale.
        canvas.sizeDelta = new Vector2(graphWidth, graphHeight);
        canvas.localScale = Vector3.one * CanvasScale;

        // Set rendered data scale (lines).
        data.localScale = new Vector3(graphWidth * CanvasScale * DataPadding, graphHeight * CanvasScale * DataPadding, 1f);

        // Set font size for labels and title.
        foreach (var text in texts)
        {
            text.fontSize = text.name.Equals("Graph Title") ? titleFontSize : labelFontSize;
        }
    }
}