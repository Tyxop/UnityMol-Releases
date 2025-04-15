using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UMol;

public class FileButton : MonoBehaviour
{
    public Text buttonText;
    private string path;
    private bool isDirectory;
    private FileExplorer fileExplorer;
    public GameObject imageFolder, imagefile;

    public void Setup(string newPath, bool newIsDirectory, FileExplorer explorer)
    {
        path = newPath;
        isDirectory = newIsDirectory;
        fileExplorer = explorer;
        
        imagefile.SetActive(!isDirectory);
        imageFolder.SetActive(isDirectory);

        buttonText.text = Path.GetFileName(path);
    }

    public void SetupDrive(string newPath, bool newIsDirectory, FileExplorer explorer)
    {
        path = newPath;
        isDirectory = newIsDirectory;
        fileExplorer = explorer;

        imagefile.SetActive(!isDirectory);
        imageFolder.SetActive(isDirectory);

        buttonText.text = path;
    }


    public void OnClick()
    {
        if (isDirectory)
        {
            fileExplorer.SetCurrentPath(path);
        }
        else
        {
            Debug.Log("File selected: " + path);
            fileExplorer.readScr.loadFileFromPath(path, false);
        }
    }
}
