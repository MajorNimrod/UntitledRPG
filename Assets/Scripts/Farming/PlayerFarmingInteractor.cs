using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor; // for Handles
#endif

public class PlayerFarmingInteractor : MonoBehaviour
{
    public Transform interactOrigin;
    public Vector2 interactOffset = new(0.6f, 0f);
    public bool showGizmos = true;

    void Awake()
    {
        if (!interactOrigin) interactOrigin = transform;
    }

    void Update()
    {
        var sys = FarmingSystem.Instance;
        if (!sys || !sys.baseMap || !sys.overlayMap) return; // not bound yet

        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector2 worldPoint = (Vector2)interactOrigin.position + interactOffset;
            bool ok = sys.TryTill(worldPoint, (Vector2)transform.position);
            Debug.Log(ok ? "[Interactor] Tilled." : "[Interactor] Till failed.");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (SceneManager.GetActiveScene().name != "Exterior") return;

        // draw origin and target line
        Vector3 origin = interactOrigin ? interactOrigin.position : transform.position;
        Vector3 target = origin + (Vector3)interactOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, target);
        Gizmos.DrawSphere(origin, 0.04f);
        Gizmos.DrawSphere(target, 0.04f);

        // if FarmingSystem + baseMap are bound, draw the cell you'll hit
        var sys = FarmingSystem.Instance;
        if (sys && sys.baseMap)
        {
            var cell = sys.baseMap.WorldToCell(target);
            var center = sys.baseMap.GetCellCenterWorld(cell);
            var size = (Vector3)sys.baseMap.cellSize; size.z = 0f;

            // highlight the exact tile cell
            Gizmos.color = new Color(1f, 0.92f, 0.16f, 1f); // gold
            Gizmos.DrawWireCube(center, size);

            // show reach vs. interactMaxDistance (green if in range, red if out)
            float dist = Vector2.Distance((Vector2)center, (Vector2)target);
            bool inRange = sys.interactMaxDistance <= 0f || dist <= sys.interactMaxDistance;

#if UNITY_EDITOR
            Handles.color = inRange ? Color.green : Color.red;
            // Draw a ring around origin = interact range
            if (sys.interactMaxDistance > 0f)
                Handles.DrawWireDisc(origin, Vector3.forward, sys.interactMaxDistance);
#endif
        }
    }
}
