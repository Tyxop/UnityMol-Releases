using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;


public class FolderButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    private string path;
    private bool isDirectory;
    public ReposudoeManager Repomanager;
    public GameObject imageFolder, imagefile;

    

    public void SetupDrive(string newPath, bool newIsDirectory, ReposudoeManager manager)
    {
        path = newPath;
        isDirectory = newIsDirectory;
        Repomanager = manager;
        
        //imagefile.SetActive(!isDirectory);
        //imageFolder.SetActive(isDirectory);

        buttonText.text = path;
    }


    public void OnClick()
    {
       if (Repomanager)
        Repomanager.loadallExternal(path);
    }
}
