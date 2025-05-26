using System.Security.Permissions;
using UnityEngine;

public class BtnSelectColor : MonoBehaviour
{

    public Renderer modelRenderer;
    public Material materialNormal,materialSelect;
     
    public void normal()
    {
        modelRenderer.material = materialNormal;
    }
    public void select()
    {  
        modelRenderer.material = materialSelect;
    }
}
