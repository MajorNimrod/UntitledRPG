using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-950)]
public class SpawnManager : MonoBehaviour
{
    public static string NextSpawnPointID;   // set by Doorway before scene load
    public static SpawnManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) { Debug.LogWarning("SpawnManager: No Player found in scene."); return; }

        // 1) Prefer an explicit SpawnPoint by ID (set by Doorway)
        if (!string.IsNullOrEmpty(NextSpawnPointID))
        {
            var points = GameObject.FindObjectsOfType<SpawnPoint>(includeInactive: true);
            foreach (var p in points)
            {
                if (p.spawnID == NextSpawnPointID)
                {
                    player.transform.position = p.transform.position;
                    NextSpawnPointID = null; // consume it
                    return;
                }
            }

            Debug.LogWarning($"SpawnManager: SpawnPoint with id '{NextSpawnPointID}' not found in scene '{scene.name}'.");
            NextSpawnPointID = null; // consume anyway so we don't carry bad state
        }

        // 2) Fallback: if there’s a PlayerSpawnPoint (no id system), use the first one
        var fallback = GameObject.FindObjectOfType<PlayerSpawnPoint>(includeInactive: true);
        if (fallback)
        {
            player.transform.position = fallback.transform.position;
            return;
        }

        // 3) Otherwise do nothing (stay where you were)
        Debug.Log("SpawnManager: No spawn specified; keeping player position.");
    }
}
