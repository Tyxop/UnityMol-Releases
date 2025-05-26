using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class lookatobj : MonoBehaviour
{
    public GameObject point; 

    public Slider  Slider;
    public TextMeshProUGUI texto;

    public float maxvalue,minvalue;
    public GameObject rotableObj;

    public void updateSlider(float value,float bfactor) {

        Slider.minValue = minvalue;
        Slider.maxValue = maxvalue;

        Slider.value = value;
        
        texto.text = bfactor.ToString() + " kcal/mol";
    }

    public void updateText(string textMsg)
    {
        if (!point) {
            GameObject[] listCameras = GameObject.FindGameObjectsWithTag("MainCamera");

            for (int i = 0; i < listCameras.Length; i++) { 
                if (listCameras[i].name == "CenterEyeAnchor" ) point= listCameras[i];
            }

        }       
        texto.text = textMsg;
    }

    // Update is called once per frame
    void Update()
    {
        if (point)
        {
            if (rotableObj != null) {
                rotableObj.transform.LookAt(point.transform.position);
            }
            else
            {
                gameObject.transform.LookAt(point.transform.position);
            }
             
        }
            
    }
}
