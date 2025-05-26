using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Globalization;

public class PDBParser : MonoBehaviour
{
    [Header("PDB Settings")]
    [Tooltip("Path to the PDB file (relative to project's StreamingAssets folder)")]
    public string pdbFilePath = "all_redux.pdb";
    
    [Header("Visualization Settings")]
    [Tooltip("Prefab for model visualization (optional)")]
    public GameObject modelPrefab;
    [Tooltip("Material for the sphere collider visualization")]
    public Material sphereMaterial;
    [Tooltip("Size of the sphere collider")]
    public float sphereRadius = 1.0f;
    [Tooltip("Parent transform for all created models")]
    public Transform modelsParent;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Internal lists to store model data
    private List<ModelData> models = new List<ModelData>();
    
    // Class to store model data
    [System.Serializable]
    public class ModelData
    {
        public int modelNumber;
        public List<AtomData> atoms = new List<AtomData>();
        public Vector3 centerPoint;
        public float size;
        public GameObject modelObject;
        
        public void CalculateCenter()
        {
            float maxX, maxY, maxZ;
            maxX =maxY= maxZ = float.MinValue;
            float minX, minY, minZ;
            minX = minY = minZ = float.MaxValue;

            if (atoms.Count == 0) return;
            
            Vector3 sum = Vector3.zero;
            foreach (var atom in atoms)
            {
                sum += atom.position;
                maxX = Mathf.Max(maxX, atom.position.x);
                maxY = Mathf.Max(maxY, atom.position.y);
                maxZ = Mathf.Max(maxZ, atom.position.z);

                minX = Mathf.Min(minX, atom.position.x);
                minY = Mathf.Min(minY, atom.position.y);
                minZ = Mathf.Min(minZ, atom.position.z);
                
            }
            centerPoint = sum / atoms.Count;
            size = Vector3.Distance(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ))/2.0f;
            if (size == 0f) size = 1.0f;
        }
    }
    
    // Class to store atom data
    [System.Serializable]
    public class AtomData
    {
        public int serialNumber;
        public string atomName;
        public string residueName;
        public int residueSequence;
        public Vector3 position;
        
        public AtomData(int serial, string name, string resName, int resSeq, Vector3 pos)
        {
            serialNumber = serial;
            atomName = name;
            residueName = resName;
            residueSequence = resSeq;
            position = pos;
        }
    }
    
    void Start()
    {
        if (modelsParent == null)
        {
            // Create a parent object if not assigned
            GameObject parent = new GameObject("PDB Models");
            modelsParent = parent.transform;
        }
        
        // Parse the PDB file
        StartCoroutine(ParsePDBFile());
    }
    
    IEnumerator ParsePDBFile()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, pdbFilePath);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError("PDB file not found: " + filePath);
            yield break;
        }
        
        Debug.Log("Starting to parse PDB file: " + filePath);
        
        // Read the file
        string[] lines = File.ReadAllLines(filePath);
        ModelData currentModel = null;
        
        foreach (string line in lines)
        {
            // Check for MODEL line to start a new model
            if (line.StartsWith("MODEL"))
            {
                // Extract model number
                string numStr = line.Substring(5).Trim();
                int modelNum = 0;
                int.TryParse(numStr, out modelNum);
                
                // Create a new model data
                currentModel = new ModelData();
                currentModel.modelNumber = modelNum;
                models.Add(currentModel);
                
                if (showDebugInfo)
                    Debug.Log("Found Model: " + modelNum);
            }
            // Check for ATOM lines to add atoms
            else if (line.StartsWith("ATOM") && currentModel != null)
            {
                try
                {
                    // Parse ATOM line - PDB format is fixed column
                    int serial = int.Parse(line.Substring(6, 5).Trim());
                    string atomName = line.Substring(12, 4).Trim();
                    string resName = line.Substring(17, 3).Trim();
                    int resSeq = int.Parse(line.Substring(22, 4).Trim());
                    
                    // Parse coordinates
                    float x = float.Parse(line.Substring(30, 8).Trim(), CultureInfo.InvariantCulture);
                    float y = float.Parse(line.Substring(38, 8).Trim(), CultureInfo.InvariantCulture);
                    float z = float.Parse(line.Substring(46, 8).Trim(), CultureInfo.InvariantCulture);
                    Vector3 position = new Vector3(x, y, z);
                    
                    // Create atom and add to current model
                    AtomData atom = new AtomData(serial, atomName, resName, resSeq, position);
                    currentModel.atoms.Add(atom);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Error parsing ATOM line: " + line + "\n" + e.Message);
                }
            }
            // Check for ENDMDL to finish the current model
            else if (line.StartsWith("ENDMDL") && currentModel != null)
            {
                // Calculate center point
                currentModel.CalculateCenter();
                
                if (showDebugInfo)
                    Debug.Log($"Model {currentModel.modelNumber} has {currentModel.atoms.Count} atoms, center at {currentModel.centerPoint}");
                
                // Yield to not freeze the editor/game
                yield return null;
            }
        }
        
        // Create GameObjects for all models
        CreateModelObjects();
        
        Debug.Log($"PDB parsing complete. Found {models.Count} models.");
    }
    
    void CreateModelObjects()
    {
        foreach (var model in models)
        {
            // Create parent GameObject for this model
            GameObject modelObj;
            
            if (modelPrefab != null)
                modelObj = Instantiate(modelPrefab, modelsParent);
            else
                modelObj = new GameObject();
            if (model.size == 0.0f) model.size = 1.0f;

            sphereRadius = model.size;

            modelObj.name = $"Model_{model.modelNumber}";
            modelObj.transform.SetParent(modelsParent);
            //model.modelObject = modelObj;
            
            // Position the model object at the center of the atoms
            modelObj.transform.position = model.centerPoint;
            modelObj.transform.localScale = Vector3.one;

            // Add sphere collider at the center
            SphereCollider collider = modelObj.AddComponent<SphereCollider>();
            collider.radius = sphereRadius;
            collider.gameObject.tag = "AtomCollider";

            
            // Add a visible sphere to visualize the collider
            /*GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(modelObj.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * sphereRadius * 2; // Diameter = radius * 2
            
            // Apply material if provided
            if (sphereMaterial != null)
            {
                Renderer renderer = sphere.GetComponent<Renderer>();
                renderer.material = sphereMaterial;
            }*/

            if (showDebugInfo)
                Debug.Log($"Created GameObject for Model {model.modelNumber} at position {model.centerPoint}");
        }
    }
    
    // Helper method to visualize atom positions (optional)
    public void VisualizeAtoms(int modelIndex, float atomSize = 0.2f)
    {
        if (modelIndex < 0 || modelIndex >= models.Count)
        {
            Debug.LogError("Invalid model index");
            return;
        }
        
        ModelData model = models[modelIndex];
        GameObject atomsParent = new GameObject("Atoms");
        atomsParent.transform.SetParent(model.modelObject.transform);
        
        foreach (var atom in model.atoms)
        {
            GameObject atomObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atomObj.transform.SetParent(atomsParent.transform);
            atomObj.transform.position = atom.position;
            atomObj.transform.localScale = Vector3.one * atomSize;
            atomObj.name = $"{atom.atomName}_{atom.serialNumber}";
        }
    }
}
