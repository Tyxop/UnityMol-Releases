using UnityEngine;
using System.IO;

using UMol;
using UMol.API;
using System.Collections.Generic;
using System.Collections;
using System;


public class ReposudoeManager : MonoBehaviour
{

    public ReadSaveFilesWithBrowser readScr;
    private string currentPath;
    public string subFolder;

    public OVRHand leftHand;
    public OVRHand rightHand;

    public Transform MoleculeParent;
    public lookatobj lookatobj;
    public lookatobj lookatobj2; 

    public GameObject Hook, LigandoHook, LoadedMolecules;
    public GameObject PosicionesAll;
    private Vector3 LigandoHookPosition;
    public GameObject Botones;

    // Experimento de seleccion
    /*private bool isIndexFingerPinchingLeft, isIndexFingerPinchingRight;
    public OVRSkeleton skeleton;
    private Transform handIndexTipTransform;
    public Transform CamPosition;
    private UnityMolAtom atomSelected;
    private bool isPinching = false;
    private bool parenting = false;*/

    public bool isExternal = false; // para los que se cargar desde afuera de streamingassets

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoleculeParent = GameObject.Find("LoadedMolecules").transform;
        LigandoHookPosition = LigandoHook.transform.position;

       load_Demo();
        
        APIPython.repo = this;

    }
    public void resetall()
    {

        LigandoHook.transform.position = LigandoHookPosition;// recuperamos la posicion del hook

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm != null)
        {
            if (sm.loadedStructures.Count > 0)
            {
                // Create a list of keys to delete after iteration
                List<UnityMolStructure> keysToDelete = new List<UnityMolStructure>();

                foreach (var item in sm.loadedStructures)
                {
                    keysToDelete.Add(item);
                    
                }

                // Delete the items after completing the iteration
                foreach (UnityMolStructure key in keysToDelete)
                {
                    sm.Delete(key);
                }
            }
        }

        LigandoHook.SetActive(true);
        Hook.transform.localScale = Vector3.one;
        LigandoHook.transform.localScale = Vector3.one;
        LoadedMolecules.transform.localScale = Vector3.one;
        LoadedMolecules.transform.localPosition = Vector3.zero;

    }
    public void load_Demo() {
        //subFolder = "ALK_alectinib";
        //subFolder = "ALK_ceritinib";
        //subFolder = "CDK4_abemaciclib";
        //subFolder = "CDK4_palbociclib";
        subFolder = "CDK6_alectinib";
        //subFolder = "CDK6_palbociclib";
        

        loadall(subFolder);
    }
    public void SetBtnSelected() { 
        
        // Reseteamos
        for (int i = 0; i < Botones.transform.childCount; i++)
        {
            string namebtn = Botones.transform.GetChild(i).name.ToUpper();
            string namemol = subFolder.ToUpper();
             //Debug.LogError(namebtn + "->" + namemol);
            if  (namebtn.Contains(namemol)) {
                
                Botones.transform.GetChild(i).BroadcastMessage("select",SendMessageOptions.DontRequireReceiver);
            } else {
                Botones.transform.GetChild(i).BroadcastMessage("normal", SendMessageOptions.DontRequireReceiver);
            }
        }
    }


    public void loadall(string subfolderparam)
    {
         
        isExternal = false;
        resetall();
        subFolder = subfolderparam;
        SetBtnSelected();
          

        // carga de molecula
        // Tambien se crea un aversin en surface
        btnLoadMolecule();

      
        // Es la animacion que crea colisionadores para enseñar el ligando colocado
        btnLoadPosLigando();
        // Ligando para arrastrar
        btnLoadLigando();

        // carga aniamcion de molecula
        btnLoadMoleculeAnimation();
        btnLoadAnim();

      
    }
    public void loadallExternal(string subfolderparm)
    {
        subFolder = ""; // los externos reinician los botones
       
        SetBtnSelected();

        isExternal = true;
        resetall();

        subFolder = subfolderparm;
        // carga de molecula
        // Tambien se crea un aversin en surface
        btnLoadMolecule();
        
        // Es la animacion que crea colisionadores para enseñar el ligando colocado
        btnLoadPosLigando();
        // Ligando para arrastrar
        btnLoadLigando();

        // carga aniamcion de molecula
        btnLoadMoleculeAnimation();
        btnLoadAnim();

    }
    public void Reload() {

        isPlaying = false;
        isExternal = false;
        resetall();
        
        // carga de molecula
        // Tambien se crea un aversin en surface
        btnLoadMolecule();
        
        // Es la animacion que crea colisionadores para enseñar el ligando colocado
        btnLoadPosLigando();
        // Ligando para arrastrar
        btnLoadLigando();

        // lo paso al final para ver si carga mejor la ultima estructura... por que la animacion se lee asincrona
        // carga aniamcion de molecula
        btnLoadMoleculeAnimation();
        btnLoadAnim();
    }

    public void btnLoadMolecule()
    {
        string filePath = "";
        string fileName = "receptor.pdb"; // antes era Center
        if (isExternal) {
            filePath = Path.Combine(Application.dataPath,"..", "external" ,subFolder, fileName);
        } else {
            filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);
        }
         

        Debug.Log("File path :: " + filePath);

        readScr.loadFileFromPath_repo_sudoe(filePath, false, "c", "molecule"); // esta es la principal
         
    }
    public void btnLoadMoleculeAnimation()
    {
        string filePath = "";
        string fileName = "center.pdb"; // antes era Center
        if (isExternal)
        {
            filePath = Path.Combine(Application.dataPath, "..", "external", subFolder, fileName);
        }
        else
        {
            filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);
        }


        Debug.Log("File path :: " + filePath);

        readScr.loadFileFromPath_repo_sudoe(filePath, false, "c", "animation"); // esta es la principal

    }

    public void btnLoadPosLigando()
    {
        string fileName = "all.pdb";
        string filePath = "";
        if (isExternal)
        {
            filePath = Path.Combine(Application.dataPath, "..", "external", subFolder, fileName);
        }
        else
        {
            filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);
        }
        VinaResultExtractor extractor = new VinaResultExtractor();
        float[] lista = extractor.ExtractVinaResults(subFolder,isExternal);

        JSONReader Residues = gameObject.GetComponent<JSONReader>();
        Residues.LoadLigandoResidues(subFolder, isExternal);

        /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
        readScr.loadFileFromPath_repo_sudoe(filePath, false, "l", "pos");

        GameObject ddd = GameObject.Find("all(all)");

        for (int i = 0; i < ddd.transform.childCount; i++)
        {
            //Debug.Log(ddd.transform.GetChild(i).name);
            int numindex = 0;
            if (int.TryParse(ddd.transform.GetChild(i).name, out numindex)) {
                Contact c = ddd.transform.GetChild(i).GetComponent<Contact>();
                c.maxBfactor = lista[numindex];
                c.indice = numindex;
                c.maxindice = lista.Length - 1;
                c.lookatobj = lookatobj;
                c.lookatobj2 = lookatobj2;
                c.listResidues = Residues.ResiduesList(numindex+1);

                // Debug.LogError(ddd.name + " " + c.maxBfactor);
            }
        }
        lookatobj.maxvalue = lista.Length - 1;
        lookatobj.minvalue = 0;

        lookatobj2.maxvalue = lista.Length - 1;
        lookatobj2.minvalue = 0;
    }
    // Ligando par mover por la pantalla
    public void btnLoadLigando()
    {
        string fileName = "ligando.pdb";
        string filePath = "";
        if (isExternal)
        {
            filePath = Path.Combine(Application.dataPath, "..", "external", subFolder, fileName);
        }
        else
        {
            filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);
        }

        /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
        readScr.loadFileFromPath_repo_sudoe(filePath, false, "l", "ligando");

    }

    // Animacion de la molecula con el ligando incluido
    /*public void btnLoadAnim()
    {
        Debug.LogError("Aplication->  " + Application.platform);
        string fileName = "center.xtc";
        string filePath = "";
        if (isExternal)
        {
            filePath = Path.Combine(Application.dataPath, "..", "external", subFolder, fileName);
        }
        else
        {            
            filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);            
        }

        Debug.LogError("FILEPATH ANIM " + filePath);

        
            // esto afecta a una molecula.. si funciona
      
    }*/


    public void btnLoadAnim()
    {
        Debug.LogError("=== btnLoadAnim ===");
        Debug.LogError("Application.platform: " + Application.platform);

        string fileName = "center.xtc";
        string filePath = "";

        if (isExternal)
        {
            filePath = Path.Combine(Application.dataPath, "..", "external", subFolder, fileName);
            Debug.LogError("External file path: " + filePath);

            // Para archivos externos, usar el método normal
            readScr.loadFileFromPath_repo_sudoe(filePath, false);
        }
        else
        {
            filePath = Path.Combine(Application.streamingAssetsPath, subFolder, fileName);
            Debug.LogError("StreamingAssets file path: " + filePath);

            // Verificar si necesitamos el workaround para Android
            if (AndroidFileHandler.RequiresAndroidWorkaround(filePath))
            {
                Debug.LogError("Using Android workaround for file loading");
                string relativePath = Path.Combine(subFolder, fileName);
                StartCoroutine(LoadTrajectoryAndroid(relativePath));
            }
            else
            {
                Debug.LogError("Using standard file loading");
                readScr.loadFileFromPath_repo_sudoe(filePath, false);
            }
        }
    }

    // Nuevo método para cargar trayectorias en Android
    private IEnumerator LoadTrajectoryAndroid(string relativePath)
    {
        Debug.LogError("=== LoadTrajectoryAndroid ===");
        Debug.LogError("Relative path: " + relativePath);

        bool loadingComplete = false;
        bool loadingError = false;
        string errorMessage = "";
        string cachedFilePath = "";

        // Copiar archivo a cache
        yield return StartCoroutine(AndroidFileHandler.CopyStreamingAssetToCache(
            relativePath,
            (string path) => {
                cachedFilePath = path;
                loadingComplete = true;
                Debug.LogError("File cached successfully: " + path);
            },
            (string error) => {
                errorMessage = error;
                loadingError = true;
                Debug.LogError("Error caching file: " + error);
            }
        ));

        // Esperar a que termine la copia
        while (!loadingComplete && !loadingError)
        {
            yield return null;
        }

        if (loadingError)
        {
            Debug.LogError("Failed to cache trajectory file: " + errorMessage);
            yield break;
        }

        if (string.IsNullOrEmpty(cachedFilePath) || !File.Exists(cachedFilePath))
        {
            Debug.LogError("Cached file path is invalid or file doesn't exist");
            yield break;
        }

        Debug.LogError("Loading trajectory from cached path: " + cachedFilePath);

        // Verificar el tamaño del archivo
        FileInfo fileInfo = new FileInfo(cachedFilePath);
        Debug.LogError("Cached file size: " + fileInfo.Length + " bytes");

        if (fileInfo.Length == 0)
        {
            Debug.LogError("Cached file is empty");
            yield break;
        }

        // Cargar la trayectoria usando el archivo cacheado
        try
        {
            readScr.loadFileFromPath_repo_sudoe(cachedFilePath, false);
            Debug.LogError("Trajectory loading initiated successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading trajectory: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
        }
    }



    public bool isPlaying;
    public void playAnim(bool playit)
    {

        // Ponemos la animcion de la molecual activando el ligando
        
        isPlaying = playit;

        // apagamos el All
        GameObject dd = GameObject.Find("all(receptor)");
        MeshRenderer[] todo = dd.GetComponentsInChildren<MeshRenderer>();
        foreach (var item in todo)
        {
            item.enabled = false;
        }

        //Lo apago.
        LigandoHook.SetActive(false);
        PosicionesAll.SetActive(false); // ocultamos las posiciones

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repM = UnityMolMain.getRepresentationManager();

        //APIPython.hideSelection("Residuos"); // Dejamos que se vean los residuos
        APIPython.StructureAnimation.SetActive(true);
        APIPython.showSelection("all(center)","c");


        string notPSelName = "";
        foreach (var item in selM.selections)
        {
            Debug.LogError(item.Key.ToString()+" " + item.Value.ToString());
            if (item.Key.Contains("not_protein")) {
                APIPython.showSelection(notPSelName, "l");
                notPSelName = item.Key; 
            }
        }

        
        Debug.Log("SHOW____> " + notPSelName);

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0)
        {
            Debug.LogError("No molecule loaded");
            return;
        }
        
        Debug.LogError("estructura para la animacion -> " + readScr.lastStructureName);

        UnityMolStructure s = sm.GetStructure(readScr.lastStructureName);
        if (s.modelsPlayer == null) s.createModelPlayer(); // Crea el model player si no lo tiene
        s.trajPlayer.looping = true;
        s.trajPlayer.smoothing = true;
        s.trajPlayer.play = playit;// Hack
    

    }
    /*public UnityMolAtom getAtomPointed(Transform pinchposition)
    {
        Ray ray = new Ray(pinchposition.position, (CamPosition.position - pinchposition.position)); // mainCam.ScreenPointToRay(Input.mousePosition); // Create the ray from screen to infinite

        CustomRaycastBurst raycaster = UnityMolMain.getCustomRaycast();
        UnityMolAtom a = raycaster.customRaycastAtomBurst(ray.origin, ray.direction);

        return a;
    }*/

    
}

 