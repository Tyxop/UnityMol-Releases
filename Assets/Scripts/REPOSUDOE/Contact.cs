using UMol;
using UMol.API;
using UnityEngine;
using UMol;
using System.Collections.Generic;

public class Contact : MonoBehaviour
{
    public UnityMolStructure structure;
    public UnityMolStructure MoleculeStructure;
    public ReposudoeManager reposudoeManager;

    public int numFrame;
    public float maxBfactor = 0f;
    public int indice = 0;
    public int maxindice = 0;

    private bool factorIsTaken = false;

    public lookatobj lookatobj;
    public lookatobj lookatobj2;
    public UnityMolSelection sel,moleculeSel;
    public List<int> listResidues= new List<int>();

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.name.Contains("ligando")) {
             
            if (reposudoeManager.isPlaying == false) {

                GameObject[] prevObjs = GameObject.FindGameObjectsWithTag("etiquetas");
                for (int i = 0; i < prevObjs.Length; i++) { 
                    GameObject.Destroy(prevObjs[i]);
                }   

                APIPython.setCurrentSelection("all(all)");
                APIPython.showSelection(sel.name, "l");
                //APIPython.setLineSize(sel.name, 0.4f); Experimento para hacer mas gruesos los 'palos'
                structure.setModel(numFrame);
                 

                lookatobj.updateSlider(maxindice - numFrame, maxBfactor);
                lookatobj2.updateSlider(maxindice - numFrame, maxBfactor);
                Debug.LogError("Indice" + indice + " Select name " + sel.name);
                /// Seleccion de los residuos por ID ... sacalos del json y los pasa a los contactos

                //string nuevoDuplicado = APIPython.duplicateSelection("all(receptor)");
                string ResidueList = "Residuos";
                foreach (var model in MoleculeStructure.models) {

                    if (model.name == "all(receptor)") {
                        MoleculeStructure.setModel(model.structure.currentModelId);
                   }
                }

                if (listResidues!=null) { 
                    if (listResidues.Count > 0)
                    {
                        string listaIDs = "";

                        for (int i = 0; i < listResidues.Count; i++)
                        {
                            listaIDs = listaIDs+ " " + listResidues[i];
                        }
                        APIPython.setCurrentSelection("all(receptor)");
                        

                        UnityMolSelection mm = APIPython.select("resid" + listaIDs, ResidueList,true,false,false,true,false,false);
                        
                        APIPython.showSelection(ResidueList, "l");
                        APIPython.colorSelection(ResidueList, "l", Color.magenta);
                        
                        APIPython.colorAtomType(ResidueList,"l","H",Color.white);
                        for (int i = 0; i < listResidues.Count; i++)
                            APIPython.annotateResidueText(ResidueList, listResidues[i],gameObject.transform);

                    }
                }
            }

            if (numFrame == 0) {
                // play animacion
                reposudoeManager.playAnim(true);
            }
             
            //Debug.LogError("Factor >>>" + maxBfactor);
        }


    }
    private void OnTriggerExit(Collider other)
    {
        //lookatobj.updateSlider(0, 0);
        // APIPython.deleteSelection(nuevoDuplicado);
    }


}
