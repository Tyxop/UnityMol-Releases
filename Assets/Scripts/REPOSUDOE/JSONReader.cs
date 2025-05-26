using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class Atom
{
    public int atomId;
}

[Serializable]
public class Residue
{
    public string resname;
    public int resid;
    public string chain;
    public List<int> atoms;
}

[Serializable]
public class Model
{
    public float model;
    public float energy;
    public List<Residue> residues;
}

[Serializable]
public class ModelContainer
{
    public List<Model> models;
}

public class JSONReader : MonoBehaviour
{
    private List<Model> modelData;
    private string folder;
    private bool external= false;
    void Start()
    {
     //  StartCoroutine(LoadJSONData());
    }

    public void LoadLigandoResidues(string pathtofolder, bool isExternal) { 
    
        folder = pathtofolder;
        external = isExternal;
        if (pathtofolder !="")
        {
            StartCoroutine(LoadJSONData());
        }
    }

   

    IEnumerator LoadJSONData()
    {
        string filePath = "";
        if (external) {
            filePath = Path.Combine(Application.dataPath, "..", "external", folder, "aa_interactions_arx.json");
        } else {
            filePath = Path.Combine(Application.streamingAssetsPath, folder, "aa_interactions_arx.json"); 
        }
        

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            // Handle WebGL or Android platform
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                ProcessJSONData(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error loading JSON: " + www.error);
            }
        }
        else
        {
            // For standalone platforms (Windows, Mac, Linux)
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                ProcessJSONData(jsonData);
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
            }
        }
    }

    void ProcessJSONData(string jsonData)
    {
        // The JSON is structured as an array, so we need to deserialize it differently
        modelData = JsonUtility.FromJson<ModelContainer>("{\"models\":" + jsonData + "}").models;

        // Now you can access the data
        Debug.Log("Models loaded: " + modelData.Count);

        // Example: Display information about the first model
        /*if (modelData.Count > 0)
        {
            Model firstModel = modelData[0];
            Debug.Log("Model: " + firstModel.model + ", Energy: " + firstModel.energy);
            Debug.Log("Residues count: " + firstModel.residues.Count);

            // Example: Display information about residues in the first model
            foreach (Residue res in firstModel.residues)
            {
                Debug.Log("Residue: " + res.resname + ", ID: " + res.resid + ", Chain: " + res.chain);
                Debug.Log("Atoms count: " + res.atoms.Count);
            }
        }*/
    }

    public List<int> ResiduesList(int modelIDnum)
    {
        List<int> list = new List<int>();
        if (modelData != null) { 
            for (int i = 0; i < modelData.Count; i++)
            {
                if ((int)modelData[i].model == modelIDnum)
                {
                    Model modelo = modelData[i];
                    foreach (Residue res in modelo.residues)
                    {
                        //Debug.Log("Residue: " + res.resname + ", ID: " + res.resid + ", Chain: " + res.chain);
                        list.Add(res.resid);
                    }
                    return list;
                }
            }
        }
        return null;
    }


    public MemoryStream memStream;

   
} 
