using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

[DefaultExecutionOrder(-950)]
public class CameraRig : MonoBehaviour
{
    public static CameraRig Instance { get; private set; }

    [Header("References")]
    public CinemachineVirtualCamera vcam;   // assign it in the prefab/inspector

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // 1) Rebind Follow to the Player's CameraTarget every load
        var player = Player.Instance;
        if (!player || !vcam) return;

        var follow = player.transform.Find("CameraTarget");
        vcam.Follow = follow ? follow : player.transform;

        // 2) SNAP the Framing Transposer for one frame (prevents drift-from-origin)
        StartCoroutine(SnapFramingTransposerOnce());
    }

    System.Collections.IEnumerator SnapFramingTransposerOnce()
    {
        var ft = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (ft)
        {
            // cache current damping
            float xd = ft.m_XDamping, yd = ft.m_YDamping, zd = ft.m_ZDamping;

            // zero damping & invalidate to teleport to target this frame
            ft.m_XDamping = ft.m_YDamping = ft.m_ZDamping = 0f;
            vcam.PreviousStateIsValid = false;

            yield return null; // one frame

            // restore your normal damping
            ft.m_XDamping = xd; ft.m_YDamping = yd; ft.m_ZDamping = zd;
        }
    }
}