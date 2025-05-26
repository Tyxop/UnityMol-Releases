using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Adaptado de https://gist.github.com/mminer/975374

public class PopUpSystem : MonoBehaviour
{

    public GameObject notificationButtonPrefab;
    public Transform notificationPanelTransform;

    [Tooltip("Ajusta este valor para retrasar la animación de desvanecimiento")]
    public float timeBeforeAnimationStarts = 3.0f;

    public Queue<GameObject> notificationQueue = new Queue<GameObject>();
    private GameObject currentGo;

    public Color errorColor = Color.red;
    public Color warningColor = Color.yellow;
    public Color logColor = Color.black;

    Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
    {
        { LogType.Assert, Color.white },
        { LogType.Error, Color.red },
        { LogType.Exception, Color.red },
        { LogType.Log, Color.white },
        { LogType.Warning, Color.yellow },
    };

    struct Log
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }
    List<Log> logs = new List<Log>();

    void OnEnable()
    {
        // Usar la API moderna para registrar callback de logs
        Application.logMessageReceived += HandleLog;
        notificationQueue.Clear();
    }

    void OnDisable()
    {
        // Remover el callback cuando se desactiva el componente
        Application.logMessageReceived -= HandleLog;
    }

    /// <summary>
    /// Registra un log desde el callback de log.
    /// </summary>
    /// <param name="message">Mensaje.</param>
    /// <param name="stackTrace">Traza de donde provino el mensaje.</param>
    /// <param name="type">Tipo de mensaje (error, excepción, advertencia, aserción).</param>
    void HandleLog(string message, string stackTrace, LogType type)
    {
        // Guardar todos los logs para referencia
        logs.Add(new Log()
        {
            message = message,
            stackTrace = stackTrace,
            type = type,
        });

        // Verificar si debemos mostrar este tipo de log
        // Puedes cambiar esta condición para mostrar otros tipos de logs
        // Por ejemplo: if (type == LogType.Error || type == LogType.Warning)
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
        {
            // Verificar que tenemos las referencias necesarias
            if (notificationPanelTransform == null || notificationButtonPrefab == null)
            {
                Debug.LogError("PopUpSystem: notificationPanelTransform o notificationButtonPrefab no están asignados");
                return;
            }

            // Crear notificación
            GameObject notif = Instantiate(notificationButtonPrefab, notificationPanelTransform);

            // Configurar texto
            Text textComponent = notif.transform.Find("Text")?.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }
            else
            {
                Debug.LogError("PopUpSystem: No se encuentra el componente Text en el prefab");
            }

            // Configurar imagen/ícono
            Image logo = notif.transform.Find("Image")?.GetComponent<Image>();
            if (logo != null)
            {
                switch (type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                        logo.color = errorColor;
                        break;
                    case LogType.Warning:
                        logo.color = warningColor;
                        break;
                    case LogType.Log:
                        logo.color = logColor;
                        break;
                    default:
                        logo.color = Color.black;
                        break;
                }
            }
            else
            {
                Debug.LogError("PopUpSystem: No se encuentra el componente Image en el prefab");
            }

            // Agregar a la cola de notificaciones
            notificationQueue.Enqueue(notif);
        }
    }

    void Update()
    {
        // Procesar la cola de notificaciones y mostrar la siguiente cuando sea necesario
        if (currentGo == null && notificationQueue.Count > 0)
        {
            currentGo = notificationQueue.Dequeue();
            if (currentGo != null)
            {
                SlideAnimationButton animComponent = currentGo.GetComponent<SlideAnimationButton>();
                if (animComponent != null)
                {
                    animComponent.delayedAnimation(timeBeforeAnimationStarts);
                }
                else
                {
                    Debug.LogError("PopUpSystem: No se encuentra el componente SlideAnimationButton en la notificación");
                    Destroy(currentGo); // Limpiar objeto sin animación
                    currentGo = null;
                }
            }
        }
    }

    public void ClearAllNotifications()
    {
        foreach (GameObject n in notificationQueue)
        {
            SlideAnimationButton animComponent = n.GetComponent<SlideAnimationButton>();
            if (animComponent != null)
            {
                animComponent.playAnimation();
            }
            else
            {
                Destroy(n);
            }
        }
        notificationQueue.Clear();
    }
}