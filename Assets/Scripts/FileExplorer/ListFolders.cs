using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
 
 
public class ListFolders : MonoBehaviour
    {
    public GameObject fileButtonPrefab;

    public Transform Panel;
    public Transform contentPanelDrives;
  
    public string pathFileTest;
    private string[] ListDrives;

    private string currentPath;
    public ReposudoeManager reposudoeManager;
    

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android) { 
        
            Panel.gameObject.SetActive(false);
            return;

        }

       // reposudoeManager = gameObject.GetComponent<ReposudoeManager>();
        currentPath = Path.Combine(Application.dataPath,"..","external");
        Debug.Log("Current path-> " + currentPath);
        GetLogicalDrives();
        UpdateFileList();
           
    }
           
    public void UpdateFileList()
    {
        // Clear current list
        foreach (Transform child in contentPanelDrives)
        {
            Destroy(child.gameObject);
        }

        
        // Get directories
        string[] directories = Directory.GetDirectories(currentPath);
        foreach (string directory in directories)
        {
            
            string lastFolderName = new DirectoryInfo(directory).Name;
            CreateButtonDrive(lastFolderName, true);
            
            //CreateButtonDrive(directory, true);
        }


        // Get files para ver si tenemos lo que necesitamos
        string[] files = Directory.GetFiles(currentPath, "*.pdb");

        foreach (string file in files)
        {
            /// revisa que tenga todos los ficheros
        }
    }
     

    void CreateButtonDrive(string path, bool isDirectory)
    {
        GameObject newButton = Instantiate(fileButtonPrefab);
        newButton.transform.SetParent(contentPanelDrives, false);

        Toggle toggle = newButton.GetComponent<Toggle>();
        toggle.isOn = false;

        FolderButton fileButton = newButton.GetComponent<FolderButton>();
        fileButton.SetupDrive(path, true, reposudoeManager);
       
    }



    public void SetCurrentPath(string newPath)
    {
        currentPath = newPath;
        PlayerPrefs.SetString("lastOpenedFolderVR", currentPath);
        UpdateFileList();
    }

    public void GoUp()
    {
        DirectoryInfo parentDir = Directory.GetParent(currentPath);
        if (parentDir != null)
        {
            SetCurrentPath(parentDir.FullName);
        }
    }

    void GetLogicalDrives()
    {
        try
        {
            ListDrives = System.IO.Directory.GetLogicalDrives();
            if (contentPanelDrives)
            {
                // Clean drives list
                foreach (Transform child in contentPanelDrives)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (string str in ListDrives)
            {
                Debug.Log(str);
                CreateButtonDrive(str, true);

            }

        }
        catch (System.IO.IOException)
        {
            Debug.LogError("An I/O error occurs.");
        }
        catch (System.Security.SecurityException)
        {
            Debug.LogError("The caller does not have the " +
                "required permission.");
        }
    }


}
    