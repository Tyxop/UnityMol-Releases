using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UMol;

 
public class FileExplorer : MonoBehaviour
    {
    public GameObject fileButtonPrefab;
    public Transform contentPanel;
    public Transform contentPanelDrives;
    public Text currentPathText;
    public ReadSaveFilesWithBrowser readScr;
    public string pathFileTest;
    private string[] ListDrives;

    private string currentPath;

    public Transform Hook;

    void Start()
    {

        if (PlayerPrefs.HasKey("lastOpenedFolderVR"))
        {
            currentPath = PlayerPrefs.GetString("lastOpenedFolderVR");
        }
        else
        {
            currentPath = Application.dataPath;
        }

        GetLogicalDrives();
        UpdateFileList();

        Debug.Log("Platform->>> "  + Application.platform);
        btnLoadMolecule(); // prueba forzada
        /*
        btnLoadAnim();

        btnLoadLigando();
              
        if (Hook)
        {
            GameObject loadedMolGO = GameObject.Find("LoadedMolecules");
            loadedMolGO.transform.position = Hook.position - UMol.API.APIPython.CenterMoleculeOffset * loadedMolGO.transform.localScale.x;
            
        } 
    
        */
             
    }

    public void btnLoadMolecule()
            {
        string subFolder = "ALK_alectinib";
        string fileName = "center.pdb";
        string filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);
        Debug.Log("File path :: " + filePath);
        readScr.loadFileFromPath(filePath, false, "c");
        //readScr.loadFileFromPath(Application.persistentDataPath + "/material/ALK_alectinib/center.pdb", false, "c");

        //readScr.loadFileFromPath(currentPath + "/prod_fep.pdb", false, "c");
        //readScr.loadFileFromPath(currentPath + "/receptor_final_bfactor_residue.pdb", false, "s");

    }
    public void btnLoadLigando()
            {
                /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
                readScr.loadFileFromPath(currentPath + "/all.pdb", false, "l");
            }

            public void btnLoadAnim()
            {
                // esto afecta a una molecula.. si funciona
                readScr.loadFileFromPath(currentPath + "/prod_fep.xtc", false);
                //  playAnim(true);
            }

            public void playAnim(bool playit)
            {

                UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

                string notPSelName = "";
                foreach (var item in selM.selections)
                {
                    if (item.Key.Contains("not_protein")) notPSelName = item.Key;
                }
                UMol.API.APIPython.showSelection(notPSelName, "l");
                Debug.Log("SHOW____> " + notPSelName);

                UnityMolStructureManager sm = UnityMolMain.getStructureManager();

                if (sm.loadedStructures.Count == 0)
                {
                    Debug.LogWarning("No molecule loaded");
                    return;
                }

                UnityMolStructure s = sm.GetStructure(readScr.lastStructureName);

                s.trajPlayer.play = playit;// Hack

            }

             


            public void UpdateFileList()
            {
                // Clear current list
                foreach (Transform child in contentPanel)
                {
                    Destroy(child.gameObject);
                }

                // Update the path text
                currentPathText.text = currentPath;

                // Get directories
                string[] directories = Directory.GetDirectories(currentPath);
                foreach (string directory in directories)
                {
                    CreateButton(directory, true);
                }


                // Get files
                string[] files = Directory.GetFiles(currentPath, "*.pdb");

                foreach (string file in files)
                {
                    CreateButton(file, false);
                }
            }

            void CreateButton(string path, bool isDirectory)
            {
                GameObject newButton = Instantiate(fileButtonPrefab);
                newButton.transform.SetParent(contentPanel, false);

                FileButton fileButton = newButton.GetComponent<FileButton>();
                fileButton.Setup(path, isDirectory, this);
            }

            void CreateButtonDrive(string path, bool isDirectory)
            {
                GameObject newButton = Instantiate(fileButtonPrefab);
                newButton.transform.SetParent(contentPanelDrives, false);

                FileButton fileButton = newButton.GetComponent<FileButton>();
                fileButton.SetupDrive(path, isDirectory, this);
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



            private void Update()
            {
                if (Keyboard.current.lKey.wasReleasedThisFrame)
                {
                    readScr.loadFileFromPath(pathFileTest, false);

                }
            }

        }
    