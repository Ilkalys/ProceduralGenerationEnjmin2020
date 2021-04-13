using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayVoid : MonoBehaviour
{
    public int sizeTile;

    public Color colorGizmos;

    private void OnDrawGizmos()
    {
        Gizmos.color = colorGizmos;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(sizeTile, sizeTile, sizeTile));
    }
}
