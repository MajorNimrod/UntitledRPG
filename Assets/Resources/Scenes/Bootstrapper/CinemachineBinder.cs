using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-800)]
public class CinemachineBinder : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] bool useCameraTargetChild = true;  // optional

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; Bind(); }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene s, LoadSceneMode m) => Bind();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Bind()
    {
        Transform target = null;

        if (Player.Instance != null)
        {
            target = useCameraTargetChild
                ? Player.Instance.transform.Find("CameraTarget")   // optional if you want a specific target on the player
                : Player.Instance.transform;
        }

        if (target == null) target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (vcam == null) vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam == null || target == null) return;

        vcam.Follow = target;
        vcam.LookAt = target;   // optional for 2; safe to set
    }
}
