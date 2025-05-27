using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

public static class AndroidFileHandler
{
    /// <summary>
    /// Copia un archivo de StreamingAssets a persistentDataPath en Android
    /// </summary>
    public static IEnumerator CopyStreamingAssetToCache(string relativePath, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        string sourceUrl = Path.Combine(Application.streamingAssetsPath, relativePath);
        string destinationPath = Path.Combine(Application.persistentDataPath, "cache", relativePath);

        Debug.LogError("=== AndroidFileHandler.CopyStreamingAssetToCache ===");
        Debug.LogError("Source: " + sourceUrl);
        Debug.LogError("Destination: " + destinationPath);

        // Crear directorio si no existe
        string directory = Path.GetDirectoryName(destinationPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Debug.LogError("Created directory: " + directory);
        }

        // Verificar si el archivo ya existe y es válido
        if (File.Exists(destinationPath))
        {
            FileInfo fileInfo = new FileInfo(destinationPath);
            if (fileInfo.Length > 0)
            {
                Debug.LogError("File already exists in cache: " + destinationPath);
                onComplete?.Invoke(destinationPath);
                yield break;
            }
        }

        // Copiar archivo usando UnityWebRequest
        using (UnityWebRequest www = UnityWebRequest.Get(sourceUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    File.WriteAllBytes(destinationPath, www.downloadHandler.data);
                    Debug.LogError("File copied successfully to: " + destinationPath);
                    Debug.LogError("File size: " + www.downloadHandler.data.Length + " bytes");
                    onComplete?.Invoke(destinationPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error writing file: " + e.Message);
                    onError?.Invoke("Error writing file: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Error loading file: " + www.error);
                onError?.Invoke("Error loading file: " + www.error);
            }
        }
    }

    /// <summary>
    /// Verifica si estamos en Android y el archivo está en StreamingAssets
    /// </summary>
    public static bool RequiresAndroidWorkaround(string filePath)
    {
        return Application.platform == RuntimePlatform.Android &&
               filePath.Contains(Application.streamingAssetsPath);
    }
}