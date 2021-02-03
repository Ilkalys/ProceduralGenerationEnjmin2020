using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeBlock:MonoBehaviour
{
    public GameObject mesh;

    int probability;

    public int valueXp, valueXm, valueZp, valueZm, valueYp, valueYm;

    public int[] GetValues()
    {
        return new int[] { valueXp, valueYp, valueZp, valueZm, valueYm, valueXm };
    }
}
