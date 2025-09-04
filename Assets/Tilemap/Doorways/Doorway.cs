using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Doorway : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] string targetSceneName;    // e.g. "Inside" or "Outside"

    [Header("Spawn Point ID")]
    [SerializeField] string spawnPointID;       // which spawn to use in target scene

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Save spawn point ID in a persistent object before swapping scenes.
        SpawnManager.NextSpawnPointID = spawnPointID;

        // Load the new scene
        SceneManager.LoadScene(targetSceneName);
    }
}
