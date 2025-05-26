using UnityEngine;

public class restoreposition : MonoBehaviour
{

    private Vector3 inipos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inipos = gameObject.transform.position;    
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y < 0f)
        {
            gameObject.transform.position = inipos;
        }
    }
}
