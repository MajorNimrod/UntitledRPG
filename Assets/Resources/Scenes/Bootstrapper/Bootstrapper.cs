using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]  // runs before most scene scripts
public class Bootstrapper : MonoBehaviour
{
    public List<string> sceneNames = new();

    private void Awake()
    {
        sceneNames.Clear();
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            sceneNames.Add(name);
        }
    }

    IEnumerator Start()
    {
        // Optional tiny delay for splash or to guarantee init order
        yield return null;
        SceneManager.LoadScene(1);
    }
}