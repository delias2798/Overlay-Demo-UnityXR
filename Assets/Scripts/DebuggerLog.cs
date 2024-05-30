using UnityEngine;
using TMPro;

public class DebuggerLog : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    private string output = "";
    private string stack = "";

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        Debug.Log("Log enabled!");
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        ClearLog();
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString + "\n" + output;
        stack = stackTrace;
    }

    private void OnGUI()
    {
        debugText.text = output;
    }

    public void ClearLog()
    {
        output = "";
    }
}




