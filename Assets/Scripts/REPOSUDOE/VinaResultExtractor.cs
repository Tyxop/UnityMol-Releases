using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
 

public class VinaResultExtractor  
{
    [SerializeField] private TextAsset pdbFile; // Arrastra tu archivo PDB aquí en el inspector

    // Para cargar el archivo desde StreamingAssets
    [SerializeField] private string pdbFileName = "all.pdb";
     

    private List<float> vinaResults = new List<float>();

    public float[] ExtractVinaResults(string subFolder,bool isExternal)
    {
        string pdbContent;
         

        // Opción 1: Usar TextAsset asignado en el inspector
        if (pdbFile != null)
        {
            pdbContent = pdbFile.text;
        }
        // Opción 2: Cargar desde StreamingAssets
        else
        {
            string filePath = "";
            if (isExternal) { 
                filePath = Path.Combine(Application.dataPath,"..","external", subFolder, pdbFileName); }
            else
            { 
                filePath = Path.Combine(Application.streamingAssetsPath, subFolder, pdbFileName); }             



            if (Application.platform == RuntimePlatform.Android)
            {
                Stream textStream;
               // textStream = new StringReaderStream(LoadTextFileAsync(filePath));
                pdbContent = LoadTextFileAsync(filePath);
            }
            else
            {
                
                if (File.Exists(filePath))
                {
                    pdbContent = File.ReadAllText(filePath);
                }
                else
                {
                    Debug.LogError("No se pudo encontrar el archivo PDB: " + filePath);
                    return null;
                }
            }


        }

        // Extraer todos los valores de VINA RESULT usando regex
        Regex regex = new Regex(@"REMARK VINA RESULT:\s+(-?\d+\.\d+)");
        MatchCollection matches = regex.Matches(pdbContent);

        // Limpiar resultados previos
        vinaResults.Clear();

        // Almacenar los valores encontrados
        foreach (Match match in matches)
        {
           
            if (match.Groups.Count >= 2)
            {
                string valueStr = match.Groups[1].Value;
                if (float.TryParse(valueStr, NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
                {
                    vinaResults.Add(value);
                   // Debug.Log("Valor VINA RESULT encontrado: " + value);
                }
            }
        }

        // Mostrar cuántos valores se encontraron
        Debug.Log("Total de valores VINA RESULT encontrados: " + vinaResults.Count);

        // También puedes convertir la lista a un array si lo necesitas
        float[] vinaResultsArray = vinaResults.ToArray();
        
        return vinaResultsArray;
    }

    // Método para acceder a los resultados desde otros scripts
    public float[] GetVinaResults()
    {
        return vinaResults.ToArray();
    }

    public string LoadTextFileAsync(string file)
    {
        // Para Android, usamos la clase WWW (obsoleta pero funcional sin coroutines)
        // o podemos usar UnityWebRequest con await
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(file))
        {
            var operation = www.SendWebRequest();

            // Esperar a que termine la operación sin usar coroutines
            while (!operation.isDone)
            {

            }

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                return www.downloadHandler.text;
                // Debug.Log(result);
            }
            else
            {
                return  null;
            }
        }

        return null;

    }
}