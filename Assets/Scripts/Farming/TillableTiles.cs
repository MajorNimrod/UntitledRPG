using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName ="Farming/TillableTiles")]
public class TillableTiles : ScriptableObject
{
    public TileBase[] tillable; // drag your dirt/hoel tiles here
    public bool IsTillable(TileBase t)
    {
        if (!t) return false;
        foreach (var x in tillable) if (x == t) return true;
        return false;
    }
}
