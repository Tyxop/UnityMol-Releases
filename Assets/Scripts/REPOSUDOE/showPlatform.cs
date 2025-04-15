using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
public class showPlatform : MonoBehaviour
{

    public Text texto;

    // Nombre del archivo que quieres leer
    public string fileName = "center.pdb";
    // Nombre de la subcarpeta dentro de StreamingAssets (si existe)
    public string subFolder = "ALK_alectinib";

    void Start()
    {
        // Leer el archivo
        StartCoroutine(LoadTextFile());
       
    }

    IEnumerator LoadTextFile()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);
        string result = "";

        // La forma de acceder al archivo depende de la plataforma
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            // Para Android o WebGL necesitamos usar UnityWebRequest
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                result = www.downloadHandler.text;
               // Debug.Log("Archivo leído correctamente: " + result);
            }
            else
            {
                Debug.LogError("Error al leer el archivo: " + www.error);
            }
        }
        else
        {
            // Para PC, Mac, iOS y otras plataformas podemos usar File.ReadAllText
            try
            {
                result = File.ReadAllText(filePath);
                //Debug.Log("Archivo leído correctamente: " + result);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al leer el archivo: " + e.Message);
            }

            yield return null;
        }

        // Aquí puedes hacer algo con el contenido del archivo
        //ProcessFileContent(result);
    }

    void ProcessFileContent(string content)
    {
        // Procesa el contenido del archivo según tus necesidades
        texto.text= "Procesando contenido: " + content;
    }
     
       

 

}
