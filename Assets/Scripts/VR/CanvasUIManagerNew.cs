using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UMol;
using UMol.API;
using TMPro;

public class CanvasUIManagerNew : MonoBehaviour
{
    // Start is called before the first frame update
   
 

	public void wrapperFetchTMpro(TMPro.TMP_InputField t)
	{
		StartCoroutine(fetch(t.text));
	}


	public IEnumerator fetch(string t)
	{
		 
		bool mmCIF = false;
		bool readHTM = false;
		
		try
		{
			APIPython.fetch(t, mmCIF, readHTM);			 
		}
		catch (System.Exception e)
		{
			string errM = e.ToString();
			if (errM.Contains("404"))
			{
				Debug.LogError("Wrong PDB Id");
			}
			else if (errM.Contains("ConnectFailure"))
			{
				Debug.LogError("No internet connection or blocked access to the PDB");
			}
			else
			{
				Debug.LogError("Could not fetch PDB file"+ e.ToString());
			}			 
		}
		yield return 0;
	}

    public void Update()
    {
		if (Keyboard.current.cKey.wasReleasedThisFrame)
		{
			StartCoroutine(fetch("2crn"));
		}
	}
	   
}
