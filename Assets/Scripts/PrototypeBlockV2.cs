using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeBlockV2:MonoBehaviour
{
    public GameObject mesh;

    public int id;

    public List<int> xP, xM, yP, yM, zP, zM;


    /* 
       Ce qu'il faut faire
       Ajouter un trigger par face, avec un ontriggerenter qui ajoute à la bonne liste celon le coté touché par leditTrigger
       les listes doivent etre statiques pour etre communes à toutes les instances
       il faut une liste sans repetitions
       il faut indiquer sur le code des ontriggerenter quel face ce trigger represente (possiblement un choix dans un menu deroulant)
    */


    public List<int>[] GetValues()
    {
        return new List<int>[] { xP, yP, zP, zM, yM, xM };
    }
}
