using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmingSystem : MonoBehaviour
{
    public static FarmingSystem Instance { get; private set; }

    // Bound each scene via a lightweight binder

    [Header("Tilemaps")]
    [System.NonSerialized] public Tilemap baseMap;     // "Farming" tilemap
    [System.NonSerialized] public Tilemap overlayMap;  // separate overlay tilemap

    [Header("Visual Tiles")]
    [System.NonSerialized] public TileBase tilledOverlayTile;  // a darker/ruffled soil tile for visuals (check assets)

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

    public void SetState(Vector3Int cell, PlotState newState)
    {
        var map = StateMapForCurrentScene();
        map[cell] = newState;
        // paint overlay if we're bound
        if (!overlayMap) return;
        overlayMap.SetTile(cell, newState == PlotState.Tilled ? tilledOverlayTile : null);
    }

    public bool TryTill(Vector2 worldPoint, Vector2 playerPos)
    {
        if (!baseMap || !overlayMap) return false; // not bound yet
        var cell = baseMap.WorldToCell((Vector3)worldPoint);
        var center = baseMap.GetCellCenterWorld(cell);

        if ((center - (Vector3)playerPos).sqrMagnitude > interactMaxDistance * interactMaxDistance) return false;
        if (GetState(cell) != PlotState.Untilled) return false;
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

    // called by the scene binder:
    public void BindScene(Tilemap baseMap, Tilemap overlayMap, TileBase tilledTile)
    {
        this.baseMap = baseMap;
        this.overlayMap = overlayMap;
        this.tilledOverlayTile = tilledTile;
        // Repaint overlays for already-tilled cells in this scene:
        foreach (var kv in StateMapForCurrentScene())
            overlayMap.SetTile(kv.Key, kv.Value == PlotState.Tilled ? tilledTile : null);
    }
}
