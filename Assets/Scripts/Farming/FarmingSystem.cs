using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmingSystem : MonoBehaviour
{
    public static FarmingSystem Instance { get; private set; }

    Inventory Inv => InventoryManager.Instance?.PlayerInventory;


    // Bound each scene via a lightweight binder

    [Header("Tilemaps")]
    [System.NonSerialized] public Tilemap baseMap;     // "Farming" tilemap
    [System.NonSerialized] public Tilemap overlayMap;  // separate overlay tilemap
    [System.NonSerialized] public Tilemap plantedMap;  // separate planted overlay tilemap

    [Header("Visual Tiles")]
    [System.NonSerialized] public TileBase tilledOverlayTile;  // a darker/ruffled soil tile for visuals (check assets)
    [SerializeField] TileBase defaultPlantTile;                // default assigned in inspector.
    // please change to a list or some other data structure if you want multiple plant types.

    [Header("Rules")]
    public float interactMaxDistance = 1.6f;    // tweak to taste
    public TillableTiles tillables;

    // Per-scene plot state: sceneKey -> (cell -> state)
    readonly Dictionary<string, Dictionary<Vector3Int, PlotState>> sceneStates = new();
    string sceneKey;    // name or path of the currently active scene

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += (_, newScene) =>
        {
            sceneKey = newScene.path; // or newScene.name
            baseMap = overlayMap = null; // wait for binder to reattach
        };
        var s = SceneManager.GetActiveScene();
        sceneKey = s.path;
    }

    Dictionary<Vector3Int, PlotState> StateMapForCurrentScene()
    {
        if (!sceneStates.TryGetValue(sceneKey, out var map))
            sceneStates[sceneKey] = map = new Dictionary<Vector3Int, PlotState>();
        return map;
    }

    public PlotState GetState(Vector3Int cell)
    {
        var map = StateMapForCurrentScene();
        return map.TryGetValue(cell, out var s) ? s : PlotState.Untilled;
    }

    // Player interaction attempts:
    // Tilling:

    public bool TryTill(Vector2 worldPoint, Vector2 playerPos)
    {
        if (!baseMap || !overlayMap) return false; // not bound yet
        var cell = baseMap.WorldToCell((Vector3)worldPoint);
        var center = baseMap.GetCellCenterWorld(cell);

        if ((center - (Vector3)playerPos).sqrMagnitude > interactMaxDistance * interactMaxDistance) return false;
        if (GetState(cell) != PlotState.Untilled) return false;   // Untilled -> Tilled only
        if (!IsTillableCell(cell)) return false;

        SetState(cell, PlotState.Tilled);
        return true;
    }
    
    bool IsTillableCell(Vector3Int cell)
    {
        var t = baseMap.GetTile(cell);
        // implement your asset/sprite/mask check here
        return t != null; // placeholder
    }

    // Planting:
    public bool TryPlant(Vector2 worldPoint, Vector2 playerPos, ItemDefinition seedDef, int costPerPlant = 1)
    {
        if (!baseMap || !overlayMap) return false;

        var inv = Inv;
        if (inv == null) { Debug.LogWarning("FarmingSystem: Inventory is null (manager not ready?)"); return false; }

        var cell = baseMap.WorldToCell((Vector3)worldPoint);
        var center = baseMap.GetCellCenterWorld(cell);

        if ((center - (Vector3)playerPos).sqrMagnitude > interactMaxDistance * interactMaxDistance) return false;
        if (GetState(cell) != PlotState.Tilled) return false;
        if (!IsPlantableCell(cell)) return false;

        if (!inv.TryRemove(seedDef, costPerPlant)) return false;

        SetState(cell, PlotState.Planted);

        // Drop the plant visual on the Planted map
        if (plantedMap)
        {
            var plantTile = GetPlantTile(seedDef);
            plantedMap.SetTile(cell, plantTile);
        }

        Debug.Log($"Planted {seedDef.name} at {cell}");
        // temp, comment out when not needed.
        //Debug.Log("Inventory Count = " + Inv.Count(seedDef));
        return true;
    }


    bool IsPlantableCell(Vector3Int cell)
    {
        if (!baseMap) return false;
        if (!tillables) return false; // ensure assigned in Inspector or via binder

        var t = baseMap.GetTile(cell);
        return t && tillables.IsTillable(t);
    }

    void SetPlantVisual(Vector3Int cell, TileBase tile)
    {
        if (!plantedMap) return;
        plantedMap.SetTile(cell, tile);
    }

    TileBase GetPlantTile(ItemDefinition def)
    {
        // for now, return default
        return defaultPlantTile;
    }


    int CheckInventory(ItemDefinition def)
    {
        var inv = Inv;
        if (inv == null) return 0;
        var list = inv.Slots;            // make sure this matches your real field/property name
        if (list == null) return 0;

        int available = 0;
        foreach (var s in list)
            if (s.def == def) available += s.quantity;
        return available;
    }

    bool HasItem(ItemDefinition def, int qty = 1) => CheckInventory(def) >= qty;

    public void SetState(Vector3Int cell, PlotState newState)
    {
        var map = StateMapForCurrentScene();
        map[cell] = newState;

        // cases for overlays
        if (overlayMap)
            overlayMap.SetTile(cell, newState == PlotState.Tilled ? tilledOverlayTile : null);

        // Clear plant overlay unless state is Planted.
        // (TryPlant sets the planted tile right after calling SetState)
        if (plantedMap && newState != PlotState.Planted)
            plantedMap.SetTile(cell, null);
    }


    // called by the scene binder:
    // Optional property for binder to set
    public TileBase DefaultPlantTile
    {
        get => defaultPlantTile;
        set => defaultPlantTile = value;
    }

    public void BindScene(Tilemap baseMap, Tilemap overlayMap, Tilemap plantedMap, TileBase tilledTile)
    {
        this.baseMap = baseMap;
        this.overlayMap = overlayMap;
        this.plantedMap = plantedMap;     // NEW
        this.tilledOverlayTile = tilledTile;

        // Repaint overlays for this scene
        foreach (var kv in StateMapForCurrentScene())
            overlayMap?.SetTile(kv.Key, kv.Value == PlotState.Tilled ? tilledTile : null);

        // (Optional) Repaint planted visuals if you want them to persist on load.
        // If you don't yet store which crop per cell, you can drop the default tile:
        foreach (var kv in StateMapForCurrentScene())
            plantedMap?.SetTile(kv.Key, kv.Value == PlotState.Planted ? defaultPlantTile : null);
    }

    // Backward-compatible overload (old binder calls this)
    public void BindScene(Tilemap baseMap, Tilemap overlayMap, TileBase tilledTile)
        => BindScene(baseMap, overlayMap, null, tilledTile);

}