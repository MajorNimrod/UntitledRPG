using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmingSceneBinder : MonoBehaviour
{
    [Header("Tilemaps (in this scene)")]
    [SerializeField] Tilemap baseMap;        // ground / Farming
    [SerializeField] Tilemap overlayMap;     // tilled shading / FarmingOverlay
    [SerializeField] Tilemap plantedMap;     // crop visuals / Planted (TOP)

    [Header("Tiles")]
    [SerializeField] TileBase tilledOverlayTile; // visual for tilled cells
    [SerializeField] TileBase defaultPlantTile;  // optional: default crop visual

    void Start() => TryBind();

    void TryBind()
    {
        var sys = FarmingSystem.Instance;
        if (!sys) { Debug.LogWarning("[FarmingBinding] FarmingSystem not present yet."); return; }

        // Pass all three maps (base, overlay, planted)
        sys.BindScene(baseMap, overlayMap, plantedMap, tilledOverlayTile);
    }
}
