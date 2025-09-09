using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmingMouseTester : MonoBehaviour
{
    public FarmingSystem farming;
    public Camera cam;

    void Reset()
    {
        if (!cam) cam = Camera.main;
        if (!farming) farming = FindObjectOfType<FarmingSystem>();
    }

    void Update()
    {
        if (!cam || !farming) return;

        // Left click to till the cell under the mouse
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = cam.ScreenToWorldPoint(Input.mousePosition);
            bool ok = farming.TryTill(world, (Vector2)transform.position);
            Debug.Log(ok ? "[Farming] Tilled." : "[Farming] Till FAILED.");
        }
    }

    // Optional: draw the hovered cell
    void OnDrawGizmos()
    {
        if (!cam || !farming || !farming.baseMap) return;
        Vector2 world = cam.ScreenToWorldPoint(Input.mousePosition);
        var cell = farming.baseMap.WorldToCell(world);
        var center = farming.baseMap.GetCellCenterWorld(cell);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, farming.baseMap.cellSize);
    }
}
