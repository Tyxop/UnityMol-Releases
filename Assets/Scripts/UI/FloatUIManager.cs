using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatUIManager : MonoBehaviour
{

    public Vector3 UserPosition;
    public float InitDistance = 2f;
    public bool Reposicionar;



    // Start is called before the first frame update
    void Start()
    {
        // posicion de la camara 
        UserPosition = Camera.main.transform.position;

        //Rotar hacia la camara
        LookAtUser();

        
    }

    void LookAtUser() {

        if (Reposicionar) {

            transform.position = UserPosition + Camera.main.transform.forward * InitDistance;

        }


        transform.rotation = Quaternion.LookRotation((transform.position - UserPosition), Vector3.up);

        RectTransform dd = gameObject.GetComponent<RectTransform>();

        Vector3[] PanelCorners = new Vector3[4];
        
        dd.GetWorldCorners(PanelCorners);

        float minY = 0;
        foreach (var corner in PanelCorners)
        {
            minY = Mathf.Min(minY, corner.y);
        }

        if (minY < 0) {
            Vector3 temPos = transform.position;
            temPos.y = temPos.y + Mathf.Abs(minY)+0.15f;
            transform.position = temPos;
        }

    
    }

    public void Update()
    {
        
    }

}
