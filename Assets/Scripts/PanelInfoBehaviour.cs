using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.IO;

public class PanelInfoBehaviour : MonoBehaviour
{
    public GameObject Canvas;
    public GameObject PanelPrefab;
    public GameObject Trivia;
    private List<GameObject> panels = new List<GameObject>();
    private int currentPanelIndex = 0;
    private string jsonPath;
    public string jsonDataFileName = "data";
    public Animator manateeAnimator = null;
    public UnityEvent StartEvent;
    public UnityEvent EndEvent;

    private float timeElapsed = 0f;
    private bool timerActive = false;

    void Start()
    {
        BetterStreamingAssets.Initialize();
        jsonPath = jsonDataFileName + ".json";
        LoadData();
        if (panels.Count > 0)
        {
            panels[0].SetActive(true);
            timerActive = true; // Inicia el temporizador
        }
    }

    void Update()
    {
        if (timerActive)
        {
            timeElapsed += Time.deltaTime;
        }
    }

    void LoadData()
    {
        if (BetterStreamingAssets.FileExists(jsonPath))
        {
            string jsonString = BetterStreamingAssets.ReadAllText(jsonPath);
            Data data = JsonUtility.FromJson<Data>(jsonString);

            InitializeDataPanels(data);
            UpdateNavigationButtons();
        }
        else
        {
            Debug.LogError("File not found: " + jsonPath);
            return;
        }
    }

    void InitializeDataPanels(Data data)
    {
        foreach (var item in data.dataList)
        {
            GameObject panel = Instantiate(PanelPrefab, Canvas.transform, false);
            TextMeshProUGUI titleText = panel.transform.Find("Tittle").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI infoText = panel.transform.Find("Paragraph").GetComponent<TextMeshProUGUI>();
            Button previousButton = panel.transform.Find("PreviousButton").GetComponent<Button>();
            Button nextButton = panel.transform.Find("NextButton").GetComponent<Button>();

            titleText.text = item.tittle;
            infoText.text = item.info;

            panels.Add(panel); // Add the panel to the list before setting up the buttons

            int panelIndex = panels.Count - 1; // Correct index for the current panel
            previousButton.onClick.AddListener(() => ChangePanel(panelIndex - 1));
            nextButton.onClick.AddListener(() => ChangePanel(panelIndex + 1));

            panel.SetActive(false);
        }
        if (panels.Count > 0) panels[0].SetActive(true);
    }

    void UpdateNavigationButtons()
    {
        if (panels.Count > 0)
        {
            panels[currentPanelIndex].transform.Find("PreviousButton").gameObject.SetActive(currentPanelIndex > 0);
            panels[currentPanelIndex].transform.Find("NextButton").gameObject.SetActive(currentPanelIndex < panels.Count - 1);
        }
        if (!(currentPanelIndex < panels.Count - 1))
        {
            panels[currentPanelIndex].transform.Find("NextButton").gameObject.SetActive(true);
            panels[currentPanelIndex].transform.Find("NextButton").GetComponent<Button>().onClick.AddListener(() => ChangeLastPanel());
        }
    }

    void ChangePanel(int newIndex)
    {
        manateeAnimator.SetTrigger("PointTrigger");
        if (newIndex < 0 || newIndex >= panels.Count)
        {
            Debug.Log("Out of bounds");
            return;
        }

        panels[currentPanelIndex].SetActive(false);
        currentPanelIndex = newIndex;
        panels[currentPanelIndex].SetActive(true);
        UpdateNavigationButtons();
    }

    void ChangeLastPanel()
    {
        manateeAnimator.SetTrigger("PointTrigger");
        panels[currentPanelIndex].SetActive(false);
        Trivia.SetActive(true);
        timerActive = false;
    }

    public float GetTimeElapsed()
    {
        return timeElapsed;
    }

    [Serializable]
    public class Data
    {
        public string startText;
        public string startButtonLabel;
        public string endText;
        public string endButtonLabel;
        public string previousButtonLabel;
        public string nextButtonLabel;
        public List<InfoItem> dataList;
    }

    [Serializable]
    public class InfoItem
    {
        public string tittle;
        public string info;
    }
}
