using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class DebugDisplay : MonoBehaviour
{
    public static string logText = "";
    private static StringBuilder buffer = new StringBuilder();
    private static List<string> logLines = new List<string>();
    private static int maxLines = 15; // Número máximo de líneas a mostrar

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void OnGUI()
    {
        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.5f);

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = Screen.height / 40;

        GUI.Box(new Rect(10, 10, Screen.width - 20, Screen.height / 3), logText, style);
    }

    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
        buffer.Length = 0;
        buffer.Append("[");

        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                buffer.Append("ERROR");
                break;
            case LogType.Warning:
                buffer.Append("WARNING");
                break;
            default:
                buffer.Append("INFO");
                break;
        }

        buffer.Append("] ");
        buffer.Append(logString);

        // Añadir a la lista manteniendo el máximo de líneas
        logLines.Add(buffer.ToString());
        if (logLines.Count > maxLines)
            logLines.RemoveAt(0);

        // Actualizar el texto a mostrar
        buffer.Length = 0;
        foreach (string line in logLines)
        {
            buffer.AppendLine(line);
        }

        logText = buffer.ToString();
    }
}