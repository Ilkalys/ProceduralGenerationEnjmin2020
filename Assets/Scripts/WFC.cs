﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFC : MonoBehaviour
{
    public int sizeTile;
    public List<GameObject> possiblePrototype;
    public Vector3Int sizeField;
    public List<GameObject>[,,] field;

    private void Start()
    {
        field = new List<GameObject>[sizeField.x, sizeField.y, sizeField.z];
        for(int i = 0; i < sizeField.z; i++)
            for (int j = 0; j < sizeField.y; j++)
                for (int k = 0; k < sizeField.x; k++)
                    field[k, j, i] = new List<GameObject>(possiblePrototype);

        
        do
        {
            // On Choisit aleatoirement une case parmis celle ayant l'entropie la plus basse == celles qui ont le moins de possibilités
            List<Vector3Int> LE = FindLowestEntropy();
            Vector3Int randPos = LE[Random.Range(0, LE.Count)];

            //On recupere les possibilités de cette case
            List<GameObject> listAtRandPos = field[randPos.x, randPos.y, randPos.z];

            // On prend une possibilité aleatoirement dans cette case et remplace la liste des possibilités par cette unique possibilité
            List<GameObject> newListAtRandPos = new List<GameObject>();
            int randomPrototype = Random.Range(0, listAtRandPos.Count);
            newListAtRandPos.Add(listAtRandPos[randomPrototype]);

            //On Instantie cette possibilité
            field[randPos.x, randPos.y, randPos.z] = newListAtRandPos;
            Instantiate(field[randPos.x, randPos.y, randPos.z][0], new Vector3(randPos.x * sizeTile, randPos.y * sizeTile, randPos.z * sizeTile), field[randPos.x, randPos.y, randPos.z][0].transform.rotation);

            // On actualise les possibilités des autres cases en fonction du nouvel ajout
            Propagation(randPos);
        }
        //Ceci tant que toutes nos cases n'ont pas été initialisés
        while (!AllAtOne());           

    }

    //On recuprere l'entropie la plus basse en regardant le nombre de possibilités de chaque case
    List<Vector3Int> FindLowestEntropy()
    {
        List<Vector3Int> result = new List<Vector3Int>();
        for (int i = 0; i < sizeField.z; i++)
            for (int j = 0; j < sizeField.y; j++)
                for (int k = 0; k < sizeField.x; k++)
                {
                    if (field[k, j, i].Count > 1 && (result.Count == 0 || field[result[0].x, result[0].y, result[0].z].Count > field[k, j, i].Count))
                    {
                        result = new List<Vector3Int>();
                        result.Add(new Vector3Int(k, j, i));
                    }
                    else if (field[k, j, i].Count > 1 &&  field[result[0].x, result[0].y, result[0].z].Count == field[k, j, i].Count)
                    {
                        result.Add(new Vector3Int(k, j, i));
                    }
                }
        return result;
    }

    //On verifie que toutes les cases sont initialisés
    bool AllAtOne()
    {
        for (int i = 0; i < sizeField.z; i++)
            for (int j = 0; j < sizeField.y; j++)
                for (int k = 0; k < sizeField.x; k++)
                    if (field[k, j, i].Count > 1) return false;
        return true;
    }

    void Propagation(Vector3Int pos)
    {
        //Stack representant les positions qui vont subir un potentiel changement et qui n'ont pas encore été traités
        Stack<Vector3Int> stack = new Stack<Vector3Int>();

        //On commence avec la position initale de la fonction
        stack.Push(pos);

        //Tant que l'on a pas traité toutes les positions qui subissent des modifications on continue
        while(stack.Count > 0)
        {
            //On retire la position dont on s'occupe
            Vector3Int currentPos = stack.Pop();

            for (int i = 0; i < 6; i++)
            {
                //On recupere toutes les possibilités de la position traité
                HashSet<int> possibleValue = new HashSet<int>();
                foreach (GameObject currentPosPrototyp in field[currentPos.x, currentPos.y, currentPos.z])
                {
                    possibleValue.Add(currentPosPrototyp.GetComponent<PrototypeBlock>().GetValues()[i]);
                }

                //On fais correspondre l'incrementation du I avec la position de l'item que l'on va traité
                int x = (i == 0) ? 1 : (i == 5) ? -1 : 0;
                int y = (i == 1) ? 1 : (i == 4) ? -1 : 0;
                int z = (i == 2) ? 1 : (i == 3) ? -1 : 0;

                //On verifie que l'on n'atteint pas les bords de la map, auquel cas on peut passer
                if (currentPos.x + x < 0 || currentPos.x + x >= sizeField.x)
                    continue;
                if (currentPos.y + y < 0 || currentPos.y + y >= sizeField.y)
                    continue;
                if (currentPos.z + z < 0 || currentPos.z + z >= sizeField.z)
                    continue;

                //CHECK LIMIT
                //Debug.Log(currentPos.x  + " " + currentPos.y  + " " + currentPos.z );
                //Debug.Log("add " +x + " " + y + " " + z);
                //On verifie que la case en +xyz n'a pas deja été initialisée
                if (field[currentPos.x + x, currentPos.y + y, currentPos.z + z].Count > 1)
                {
                    bool hasChanged = false;
                    List<GameObject> toRemove = new List<GameObject>();
                    //On fait correspondre ce que les voisins acceptent à la position traité avec ses possibilités, si un changement s'effectue on le marque et on note lesquels sont à retirer des possibilités
                    foreach (GameObject otherPrototype in field[currentPos.x + x, currentPos.y + y, currentPos.z + z])
                    {
                        if (!possibleValue.Contains(otherPrototype.GetComponent<PrototypeBlock>().GetValues()[5 - i]))          // value.lenght -1 = 5
                        {
                            toRemove.Add(otherPrototype);
                            hasChanged = true;
                        }
                    }
                    //On retire les possibilités fausses et indique que la case que l'on a traité va potentiellement aussi subir un changement
                    if (hasChanged)
                    {
                        foreach (GameObject otherPrototype in toRemove)
                        {
                            field[currentPos.x + x, currentPos.y + y, currentPos.z + z].Remove(otherPrototype);
                        }
                        stack.Push(new Vector3Int(currentPos.x + x, currentPos.y +y, currentPos.z+z));
                    }
                    //Si il ne reste qu'une possiiblité on l'instancie
                    if (field[currentPos.x + x, currentPos.y + y, currentPos.z + z].Count == 1)
                    {
                        Instantiate(field[currentPos.x + x, currentPos.y + y, currentPos.z + z][0], new Vector3((currentPos.x + x) * sizeTile, (currentPos.y + y) * sizeTile, (currentPos.z + z) * sizeTile), this.transform.rotation);
                    }
                    else if (field[currentPos.x + x, currentPos.y + y, currentPos.z + z].Count < 1) Debug.LogError("AUCUNE POSSIBILITÉ");
                }
            }
        }
    }
}
