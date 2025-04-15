using UnityEngine;
using System.IO;
using UMol;
public class ReposudoeManager : MonoBehaviour
{

    public ReadSaveFilesWithBrowser readScr;
    private string currentPath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Debug.Log("Platform->>> " + Application.platform);
        btnLoadMolecule(); // prueba forzada
        /*
        btnLoadAnim();

        btnLoadLigando();
              
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

}
