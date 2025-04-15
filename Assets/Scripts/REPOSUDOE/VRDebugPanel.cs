using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class VRDebugPanel : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    private static VRDebugPanel instance;
    private static List<string> logMessages = new List<string>();
    private static int maxMessages = 100;
    private static StringBuilder builder = new StringBuilder();

    private void Awake()
    {
        instance = this;
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logMsg = $"[{type}] {logString}";

        // Añadir nuevo mensaje
        logMessages.Add(logMsg);

        // Mantener solo los últimos N mensajes
        if (logMessages.Count > maxMessages)
        {
            logMessages.RemoveAt(0);
        }

        // Actualizar el texto de UI
        if (instance != null && instance.debugText != null)
        {
            builder.Clear();
            foreach (string msg in logMessages)
            {
                builder.AppendLine(msg);
            }
            instance.debugText.text = builder.ToString();
        }
    }

    // Método para uso externo
    public static void AddLog(string message)
    {
        Debug.Log(message);  // Esto activará el handler
    }
}