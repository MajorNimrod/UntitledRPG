using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmingSceneBinder : MonoBehaviour
{
    public Tilemap baseMap;     // scene's Farming tilemap
    public Tilemap overlayMap;  // scene's FarmingOverlay tilemap
    public TileBase tilledOverlayTile;  // visual for tilled

    private void Start()
    {
        if (FarmingSystem.Instance)
            FarmingSystem.Instance.BindScene(baseMap, overlayMap, tilledOverlayTile);
        else
            Debug.LogWarning("[FarmingBinding] FarmingSystem not present yet.");
    }
}
