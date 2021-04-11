using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    const int INPUT_MAP_SIZE = 3;
    const int GENERATED_MAP_SIZE = 10;
    const int CUBE_SIZE = 1;

    public Transform originGeneratedMap;
    public Element defaultElement;

    //List of possible elements with their allowed neighboors
    public Dictionary<int, Element> elements = new Dictionary<int, Element>();
    public Element[,,] inputMap = new Element[INPUT_MAP_SIZE, INPUT_MAP_SIZE, INPUT_MAP_SIZE];

    public Slot[,,] generatedMap = new Slot[GENERATED_MAP_SIZE, GENERATED_MAP_SIZE, GENERATED_MAP_SIZE];

    private bool isWFCFinished = false;

    

    // Start is called before the first frame update
    void Start()
    {
        ReadInput();

        InitializeGeneratedMap();

        //Do the algorithm until the end
        do
        {
            Spread(Observe());

        } while(isWFCFinished);

        print(isWFCFinished);
    }

    private void ReadInput()
    {
        //Put element in the input map
        for (int x = 0; x < INPUT_MAP_SIZE; x++)
        {
            for (int y = 0; y < INPUT_MAP_SIZE; y++)
            {
                for (int z = 0; z < INPUT_MAP_SIZE; z++)
                {
                    //Initialize input map's slots
                    inputMap[x, y, z] = defaultElement;

                    RaycastHit hit;
                    if(Physics.Raycast(new Vector3(x - (CUBE_SIZE/2f +0.1f),y,z), new Vector3(1, 0, 0), out hit, 0.5f)){

                        if(!elements.ContainsKey(hit.collider.gameObject.GetComponent<Element>().id))
                            elements.Add(hit.collider.gameObject.GetComponent<Element>().id, hit.collider.gameObject.GetComponent<Element>());

                        inputMap[x, y, z] = hit.collider.gameObject.GetComponent<Element>();
                    }
                    else
                    {
                        //Add the default element if no element
                        if (!elements.ContainsKey(defaultElement.id))
                            elements.Add(defaultElement.id, defaultElement);

                        inputMap[x, y, z] = defaultElement;
                    }                
                }
            }
        }

        //Add possible neighboor
        for (int x = 0; x < INPUT_MAP_SIZE; x++)
        {
            for (int y = 0; y < INPUT_MAP_SIZE; y++)
            {
                for (int z = 0; z < INPUT_MAP_SIZE; z++)
                {
                  
                    if (x + 1 < INPUT_MAP_SIZE && !elements[inputMap[x,y,z].id].xP.ContainsKey(inputMap[x + 1, y, z].id))
                    {
                        //print(elements[inputMap[x, y, z].id].id + " / " + inputMap[x + 1, y, z].id +"///"+x+" "+y+" "+z);
                        elements[inputMap[x, y, z].id].xP.Add(inputMap[x + 1, y, z].id, inputMap[x + 1, y, z]);
                    }
                            if(x-1 >=0 && !elements[inputMap[x, y, z].id].xM.ContainsKey(inputMap[x - 1, y, z].id))
                        elements[inputMap[x, y, z].id].xM.Add(inputMap[x - 1, y, z].id, inputMap[x - 1, y, z]);


                    if (y+1 < INPUT_MAP_SIZE && !elements[inputMap[x, y, z].id].yP.ContainsKey(inputMap[x, y + 1, z].id))
                        elements[inputMap[x, y, z].id].yP.Add(inputMap[x, y + 1, z].id, inputMap[x, y + 1, z]);

                    if (y - 1 >= 0 && !elements[inputMap[x, y, z].id].yM.ContainsKey(inputMap[x, y - 1, z].id))
                        elements[inputMap[x, y, z].id].yM.Add(inputMap[x, y - 1, z].id, inputMap[x, y - 1, z]);


                    if (z + 1 < INPUT_MAP_SIZE && !elements[inputMap[x, y, z].id].zP.ContainsKey(inputMap[x, y, z + 1].id))
                        elements[inputMap[x, y, z].id].zP.Add(inputMap[x, y, z + 1].id, inputMap[x, y, z + 1]);

                    if (z - 1 >= 0 && !elements[inputMap[x, y, z].id].zM.ContainsKey(inputMap[x, y, z - 1].id))
                        elements[inputMap[x, y, z].id].zM.Add(inputMap[x, y, z - 1].id, inputMap[x, y, z - 1]);
                }
            }
        }

       // print(elements[0].yP[1].id);
    }

    private void InitializeGeneratedMap()
    {
        for (int x = 0; x < GENERATED_MAP_SIZE; x++)
        {
            for (int y = 0; y < GENERATED_MAP_SIZE; y++)
            {
                for (int z = 0; z < GENERATED_MAP_SIZE; z++)
                {
                    //Initialize
                    generatedMap[x, y, z] = new Slot();
                    generatedMap[x, y, z].selectedElement = null;
                    generatedMap[x, y, z].pos = new Vector3(x,y,z);

                    //For each slot, add all the allowed elements
                    foreach (KeyValuePair<int, Element> entry in elements)
                    {
                        generatedMap[x, y, z].possibilities.Add(entry.Value);
                    }
                    
                }
            }
        }
    }

    private Vector3 Observe()
    {
        //Find a wave element with the minimal nonzero entropy.

        List<Slot> lowestEntropy = new List<Slot>();

        for (int x = 0; x < INPUT_MAP_SIZE; x++)
        {
            for (int y = 0; y < INPUT_MAP_SIZE; y++)
            {
                for (int z = 0; z < INPUT_MAP_SIZE; z++)
                {
                    //if the list of lowest entropy is empty, add the next one
                    if (lowestEntropy.Count == 0)
                        lowestEntropy.Add(generatedMap[x,y,z]);

                    //check if this slot has a lowest entropy than the first elem in our list
                    if (generatedMap[x,y,z].possibilities.Count < lowestEntropy[0].possibilities.Count && generatedMap[x, y, z].selectedElement != null)
                    {
                        lowestEntropy.Clear();
                        lowestEntropy.Add(generatedMap[x, y, z]);
                    }

                    //If it's the same entropy, just add it to the list
                    if(generatedMap[x, y, z].possibilities.Count == lowestEntropy[0].possibilities.Count)
                    {
                        lowestEntropy.Add(generatedMap[x, y, z]);
                    }
                    
                }
            }
        }

        if(lowestEntropy.Count != 0)
        {
            //Pick a random element to fix in the lowest entropy list
            int _randomSlot = Random.Range(0, lowestEntropy.Count);
            lowestEntropy[_randomSlot].selectedElement = lowestEntropy[_randomSlot].possibilities[Random.Range(0, lowestEntropy[_randomSlot].possibilities.Count)];

            //Propagation
            return lowestEntropy[_randomSlot].pos;
        }
        else
        {
            isWFCFinished = true;
            return (new Vector3(-1, -1, -1));
        }

        

    }

    private void Spread(Vector3 pos)
    {
        if(pos == new Vector3(-1, -1, -1))
        {
            return;
        }

        Stack<Vector3Int> stack = new Stack<Vector3Int>();

        stack.Push(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z));

        while (stack.Count > 0)
        {
            Vector3Int currentPos = stack.Pop();

            
            if (generatedMap[currentPos.x, currentPos.y, currentPos.z].possibilities.Count == 0 && generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement == null)
            {
                Debug.Log("No solutions");
            }

            //Set the selected element if no other possibilities 
            if (generatedMap[currentPos.x, currentPos.y, currentPos.z].possibilities.Count == 1 && generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement == null)
            {
                generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement = generatedMap[currentPos.x, currentPos.y, currentPos.z].possibilities[0];
            }

            //Do the work for all neighbours
            List<Element> _cleaningList = new List<Element>();


            //Xp
            if (currentPos.x + 1 < GENERATED_MAP_SIZE)
            {
                if (generatedMap[currentPos.x + 1, currentPos.y, currentPos.z].selectedElement == null)
                {
                    foreach (var element in generatedMap[currentPos.x + 1, currentPos.y, currentPos.z].possibilities)
                    {
                        if (!generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement.xP.ContainsKey(element.id))
                        {
                            _cleaningList.Add(element);
                            //generatedMap[currentPos.x + 1, currentPos.y, currentPos.z].possibilities.Remove(element);
                            stack.Push(new Vector3Int(currentPos.x + 1, currentPos.y, currentPos.z));
                        }
                    }

                    //Delete things to delete
                    foreach (var element in _cleaningList)
                    {
                        generatedMap[currentPos.x + 1, currentPos.y, currentPos.z].possibilities.Remove(element);
                    }
                    _cleaningList.Clear();
                }
            }

            //Xm
            if (currentPos.x - 1 >= 0)
            {
                if (generatedMap[currentPos.x - 1, currentPos.y, currentPos.z].selectedElement == null)
                {
                    foreach (var element in generatedMap[currentPos.x - 1, currentPos.y, currentPos.z].possibilities)
                    {
                        if (!generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement.xM.ContainsKey(element.id))
                        {
                            _cleaningList.Add(element);
                            stack.Push(new Vector3Int(currentPos.x - 1, currentPos.y, currentPos.z));
                        }
                    }

                    //Delete things to delete
                    foreach (var element in _cleaningList)
                    {
                        generatedMap[currentPos.x - 1, currentPos.y, currentPos.z].possibilities.Remove(element);
                    }
                    _cleaningList.Clear();
                }
            }

            //Yp
            if (currentPos.y + 1 < GENERATED_MAP_SIZE)
            {
                if (generatedMap[currentPos.x, currentPos.y+1, currentPos.z].selectedElement == null)
                {
                    foreach (var element in generatedMap[currentPos.x, currentPos.y+1, currentPos.z].possibilities)
                    {
                        if (!generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement.yP.ContainsKey(element.id))
                        {
                            _cleaningList.Add(element);
                            stack.Push(new Vector3Int(currentPos.x, currentPos.y+1, currentPos.z));
                        }
                    }

                    //Delete things to delete
                    foreach (var element in _cleaningList)
                    {
                        generatedMap[currentPos.x, currentPos.y + 1, currentPos.z].possibilities.Remove(element);
                    }
                    _cleaningList.Clear();
                }
            }

            //Ym
            if (currentPos.y - 1 >= 0)
            {
                if (generatedMap[currentPos.x, currentPos.y - 1, currentPos.z].selectedElement == null)
                {
                    foreach (var element in generatedMap[currentPos.x, currentPos.y - 1, currentPos.z].possibilities)
                    {
                        if (!generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement.yM.ContainsKey(element.id))
                        {
                            _cleaningList.Add(element);
                            stack.Push(new Vector3Int(currentPos.x, currentPos.y - 1, currentPos.z));
                        }
                    }

                    //Delete things to delete
                    foreach (var element in _cleaningList)
                    {
                        generatedMap[currentPos.x, currentPos.y - 1, currentPos.z].possibilities.Remove(element);
                    }
                    _cleaningList.Clear();
                }
            }

            //Zp
            if (currentPos.z + 1 < GENERATED_MAP_SIZE)
            {
                if (generatedMap[currentPos.x, currentPos.y, currentPos.z+1].selectedElement == null)
                {
                    foreach (var element in generatedMap[currentPos.x, currentPos.y, currentPos.z+1].possibilities)
                    {
                        if (!generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement.zP.ContainsKey(element.id))
                        {
                            _cleaningList.Add(element);
                            stack.Push(new Vector3Int(currentPos.x, currentPos.y, currentPos.z+1));
                        }
                    }

                    //Delete things to delete
                    foreach (var element in _cleaningList)
                    {
                        generatedMap[currentPos.x, currentPos.y, currentPos.z + 1].possibilities.Remove(element);
                    }
                    _cleaningList.Clear();
                }
            }

            //Zm
            if (currentPos.z -1 >= 0)
            {
                if (generatedMap[currentPos.x, currentPos.y, currentPos.z - 1].selectedElement == null)
                {
                    foreach (var element in generatedMap[currentPos.x, currentPos.y, currentPos.z - 1].possibilities)
                    {
                        if (!generatedMap[currentPos.x, currentPos.y, currentPos.z].selectedElement.zM.ContainsKey(element.id))
                        {
                            _cleaningList.Add(element);
                            stack.Push(new Vector3Int(currentPos.x, currentPos.y, currentPos.z - 1));
                        }
                    }

                    //Delete things to delete
                    foreach (var element in _cleaningList)
                    {
                        generatedMap[currentPos.x, currentPos.y, currentPos.z - 1].possibilities.Remove(element);
                    }
                    _cleaningList.Clear();
                }
            }             
         
        }
    }

    //private List<Vector3Int> GetNeighbours(Vector3 pos)
    //{
    //    List<Vector3Int> neighboors = new List<Vector3Int>();

    //    //Xp
    //    if (pos.x + 1 < GENERATED_MAP_SIZE)
    //    {
    //        neighboors.Add(new Vector3Int((int)pos.x +1, (int)pos.y, (int)pos.z));
    //    }

    //    //Xm
    //    if (pos.x - 1 >= 0)
    //    {
    //        neighboors.Add(new Vector3Int((int)pos.x - 1, (int)pos.y, (int)pos.z));
    //    }

    //    //Yp
    //    if (pos.y + 1 < GENERATED_MAP_SIZE)
    //    {
    //        neighboors.Add(new Vector3Int((int)pos.x, (int)pos.y + 1, (int)pos.z));
    //    }

    //    //Ym
    //    if (pos.y - 1 >= 0)
    //    {
    //        neighboors.Add(new Vector3Int((int)pos.x, (int)pos.y - 1, (int)pos.z));
    //    }

    //    //Zp
    //    if (pos.z + 1 < GENERATED_MAP_SIZE)
    //    {
    //        neighboors.Add(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z + 1));
    //    }

    //    //Zm
    //    if (pos.z >= 0)
    //    {
    //        neighboors.Add(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z - 1));
    //    }


    //    return neighboors;
        
    //}

    private void OnDrawGizmos()
    {
        //Input Map
        for (int x = 0; x < INPUT_MAP_SIZE; x++)
        {
            for (int y = 0; y < INPUT_MAP_SIZE; y++)
            {
                for (int z = 0; z < INPUT_MAP_SIZE; z++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(new Vector3(x, y, z), new Vector3(CUBE_SIZE, CUBE_SIZE, CUBE_SIZE));
                }
            }
        }

        //Generated Map
        for (int x = 0; x < GENERATED_MAP_SIZE; x++)
        {
            for (int y = 0; y < GENERATED_MAP_SIZE; y++)
            {
                for (int z = 0; z < GENERATED_MAP_SIZE; z++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(new Vector3(originGeneratedMap.position.x + x, originGeneratedMap.position.y + y, originGeneratedMap.position.z + z), new Vector3(CUBE_SIZE, CUBE_SIZE, CUBE_SIZE));
                }
            }
        }
    }
}
