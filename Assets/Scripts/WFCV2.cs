using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCV2 : MonoBehaviour
{
    const int INPUT_MAP_SIZE_X = 20;
    const int INPUT_MAP_SIZE_Y = 10;
    const int INPUT_MAP_SIZE_Z = 20;

    public Color colorGizmos;

    public PrototypeBlockV2 defaultElement;
    public PrototypeBlockV2[,,] inputMap = new PrototypeBlockV2[INPUT_MAP_SIZE_X, INPUT_MAP_SIZE_Y, INPUT_MAP_SIZE_Z];
    //List of possible elements with their allowed neighboors
    public Dictionary<int, PrototypeBlockV2> elements = new Dictionary<int, PrototypeBlockV2>();
    public Transform originGeneratedMap;


    public int sizeTile;
    public List<GameObject> possiblePrototype;
    public Vector3Int sizeField;
    public List<GameObject>[,,] field;

    private void Start()
    {

        ReadInput();



        foreach (KeyValuePair<int, PrototypeBlockV2> entry in elements)
        {
            print("le cube d'id " + entry.Key + " peut avoir  :");
            for(int i = 0; i < 6; i++)
            {
                foreach(int value in entry.Value.GetValues()[i])
                {
                    switch(i)
                    {
                        case 0: print(value + " en position XP"); break;
                        case 1: print(value + " en position YP"); break;
                        case 2: print(value + " en position ZP"); break;
                        case 3: print(value + " en position ZM"); break;
                        case 4: print(value + " en position YM"); break;
                        case 5: print(value + " en position XM"); break;
                    }
                }
            }
        }

        field = new List<GameObject>[sizeField.x, sizeField.y, sizeField.z];
        for (int i = 0; i < sizeField.z; i++)
            for (int j = 0; j < sizeField.y; j++)
                for (int k = 0; k < sizeField.x; k++)
                {
                    List<GameObject> possibilities = new List<GameObject>();

                    foreach (PrototypeBlockV2 item in elements.Values)
                    {
                        if(!possibilities.Contains(item.gameObject))
                            possibilities.Add(item.gameObject);
                    }

                    field[k, j, i] = possibilities;
                }


        do
        {
            // On Choisit aleatoirement une case parmis celle ayant l'entropie la plus basse == celles qui ont le moins de possibilités
            List<Vector3Int> LE = FindLowestEntropy();
            Vector3Int randPos = LE[Random.Range(0, LE.Count)];
            print("here");
            //On recupere les possibilités de cette case
            List<GameObject> listAtRandPos = field[randPos.x, randPos.y, randPos.z];
            //print(listAtRandPos.Count);
            // On prend une possibilité aleatoirement dans cette case et remplace la liste des possibilités par cette unique possibilité
            List<GameObject> newListAtRandPos = new List<GameObject>();
            int randomPrototype = Random.Range(0, listAtRandPos.Count);
            newListAtRandPos.Add(listAtRandPos[randomPrototype]);

            //On Instantie cette possibilité
            field[randPos.x, randPos.y, randPos.z] = newListAtRandPos;
            Instantiate(field[randPos.x, randPos.y, randPos.z][0], originGeneratedMap.position + new Vector3(randPos.x * sizeTile, randPos.y * sizeTile, randPos.z * sizeTile), field[randPos.x, randPos.y, randPos.z][0].transform.rotation);

            // On actualise les possibilités des autres cases en fonction du nouvel ajout
            Propagation(randPos);
        }
        //Ceci tant que toutes nos cases n'ont pas été initialisés
        while (!AllAtOneOrLess());

    }

    private void ReadInput()
    {
        //Put element in the input map
        for (int x = 0; x < INPUT_MAP_SIZE_X; x++)
        {
            for (int y = 0; y < INPUT_MAP_SIZE_Y; y++)
            {
                for (int z = 0; z < INPUT_MAP_SIZE_Z; z++)
                {
                    //Initialize input map's slots
                    inputMap[x, y, z] = defaultElement;

                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(x - (sizeTile / 2f + 0.1f), y, z), new Vector3(1, 0, 0), out hit, 0.5f))
                    {
                        if (!elements.ContainsKey(hit.collider.gameObject.GetComponent<PrototypeBlockV2>().id))
                            elements.Add(hit.collider.gameObject.GetComponent<PrototypeBlockV2>().id, hit.collider.gameObject.GetComponent<PrototypeBlockV2>());

                        inputMap[x, y, z] = hit.collider.gameObject.GetComponent<PrototypeBlockV2>();
                    }
                    //else
                    //{
                    //    //Add the default element if no element
                    //    if (!elements.ContainsKey(defaultElement.id))
                    //        elements.Add(defaultElement.id, defaultElement);

                    //    inputMap[x, y, z] = defaultElement;
                    //}
                }
            }
        }

        foreach (KeyValuePair<int, PrototypeBlockV2> entry in elements)
        {
            print(entry.Key);
        }

        //Add possible neighboor
        for (int x = 0; x < INPUT_MAP_SIZE_X; x++)
        {
            for (int y = 0; y < INPUT_MAP_SIZE_Y; y++)
            {
                for (int z = 0; z < INPUT_MAP_SIZE_Z; z++)
                {
                    if (inputMap[x, y, z].id != 0 &&  x + 1 < INPUT_MAP_SIZE_X && !elements[inputMap[x, y, z].id].xP.Contains(inputMap[x + 1, y, z].id))
                    {
                        //print(elements[inputMap[x, y, z].id].id + " / " + inputMap[x + 1, y, z].id +"///"+x+" "+y+" "+z);                     
                        elements[inputMap[x, y, z].id].xP.Add(inputMap[x + 1, y, z].id);
                    }
                    if (inputMap[x, y, z].id != 0 && x - 1 >= 0 && !elements[inputMap[x, y, z].id].xM.Contains(inputMap[x - 1, y, z].id))
                        elements[inputMap[x, y, z].id].xM.Add(inputMap[x - 1, y, z].id);


                    if (inputMap[x, y, z].id != 0 && y + 1 < INPUT_MAP_SIZE_Y && !elements[inputMap[x, y, z].id].yP.Contains(inputMap[x, y + 1, z].id))
                        elements[inputMap[x, y, z].id].yP.Add(inputMap[x, y + 1, z].id);

                    if (inputMap[x, y, z].id != 0 && y - 1 >= 0 && !elements[inputMap[x, y, z].id].yM.Contains(inputMap[x, y - 1, z].id))
                        elements[inputMap[x, y, z].id].yM.Add(inputMap[x, y - 1, z].id);


                    if (inputMap[x, y, z].id != 0 && z + 1 < INPUT_MAP_SIZE_Z && !elements[inputMap[x, y, z].id].zP.Contains(inputMap[x, y, z + 1].id))
                        elements[inputMap[x, y, z].id].zP.Add(inputMap[x, y, z + 1].id);

                    if (inputMap[x, y, z].id != 0 && z - 1 >= 0 && !elements[inputMap[x, y, z].id].zM.Contains(inputMap[x, y, z - 1].id))
                        elements[inputMap[x, y, z].id].zM.Add(inputMap[x, y, z - 1].id);
                }
            }
        }
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
                    else if (field[k, j, i].Count > 1 && field[result[0].x, result[0].y, result[0].z].Count == field[k, j, i].Count)
                    {
                        result.Add(new Vector3Int(k, j, i));
                    }
                }
        return result;
    }

    //On verifie que toutes les cases sont initialisés
    bool AllAtOneOrLess()
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
        while (stack.Count > 0)
        {
            //On retire la position dont on s'occupe
            Vector3Int currentPos = stack.Pop();



            for (int i = 0; i < 6; i++)
            {
                //On recupere toutes les possibilités de la position traité
                HashSet<int> possibleValue = new HashSet<int>();
                foreach (GameObject currentPosPrototyp in field[currentPos.x, currentPos.y, currentPos.z])
                {
                    foreach (int tmp in currentPosPrototyp.GetComponent<PrototypeBlockV2>().GetValues()[i])
                        possibleValue.Add(tmp);
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
                    foreach (GameObject possibiliteXYZ in field[currentPos.x + x, currentPos.y + y, currentPos.z + z])
                    {
                        bool hasValue = false;
                            if (possibleValue.Contains(possibiliteXYZ.GetComponent<PrototypeBlockV2>().id))
                            {
                                hasValue = true;
                            }

                        if (!hasValue)
                        {
                            toRemove.Add(possibiliteXYZ);
                            hasChanged = true;
                        }
                    }
                    //On retire les possibilités fausses et indique que la case que l'on a traité va potentiellement aussi subir un changement
                    if (hasChanged)
                    {
                        foreach (GameObject possibiliteXYZ in toRemove)
                        {
                            field[currentPos.x + x, currentPos.y + y, currentPos.z + z].Remove(possibiliteXYZ);
                        }
                        stack.Push(new Vector3Int(currentPos.x + x, currentPos.y + y, currentPos.z + z));

                    }
                    //Si il ne reste qu'une possiiblité on l'instancie
                    if (field[currentPos.x + x, currentPos.y + y, currentPos.z + z].Count == 1)
                    {
                        Instantiate(field[currentPos.x + x, currentPos.y + y, currentPos.z + z][0], originGeneratedMap.position + new Vector3((currentPos.x + x) * sizeTile, (currentPos.y + y) * sizeTile, (currentPos.z + z) * sizeTile), this.transform.rotation);
                    }
                    else if (field[currentPos.x + x, currentPos.y + y, currentPos.z + z].Count < 1) Debug.LogError("AUCUNE POSSIBILITÉ");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Input Map
        for (int x = 0; x < INPUT_MAP_SIZE_X; x++)
        {
            for (int y = 0; y < INPUT_MAP_SIZE_Y; y++)
            {
                for (int z = 0; z < INPUT_MAP_SIZE_Z; z++)
                {
                    Gizmos.color = colorGizmos;
                    Gizmos.DrawWireCube(new Vector3(x, y, z), new Vector3(sizeTile, sizeTile, sizeTile));
                }
            }
        }

        //Generated Map
        for (int x = 0; x < sizeField.x; x++)
        {
            for (int y = 0; y < sizeField.y; y++)
            {
                for (int z = 0; z < sizeField.z; z++)
                {
                    Gizmos.color = colorGizmos;
                    Gizmos.DrawWireCube(new Vector3(originGeneratedMap.position.x + x, originGeneratedMap.position.y + y, originGeneratedMap.position.z + z), new Vector3(sizeTile, sizeTile, sizeTile));
                }
            }
        }
    }
}
