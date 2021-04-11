using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element : MonoBehaviour
{
    public int id;
    public bool isForbidden;
    public float coeff;

    public Dictionary<int, Element> xP = new Dictionary<int, Element>();
    public Dictionary<int, Element> xM = new Dictionary<int, Element>();
    public Dictionary<int, Element> yP = new Dictionary<int, Element>();
    public Dictionary<int, Element> yM = new Dictionary<int, Element>();
    public Dictionary<int, Element> zP = new Dictionary<int, Element>();
    public Dictionary<int, Element> zM = new Dictionary<int, Element>();

    public void Initialize()
    {
        isForbidden = false;
        coeff = 1;
    }
}
