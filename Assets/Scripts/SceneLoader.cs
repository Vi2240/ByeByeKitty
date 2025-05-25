using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Optional Settings")]
    [SerializeField] float loadDelay = 0f; // Delay before loading the scene
    [SerializeField] bool logLoad = true;  // Log loading action

    /// <summary>
    /// Load a scene by name.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (IsSceneInBuild(sceneName))
        {
            if (logLoad) Debug.Log($"Loading scene: {sceneName}");
            StartCoroutine(LoadSceneWithDelay(sceneName));
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' is not in build settings.");
        }
    }

    /// <summary>
    /// Load a scene by index.
    /// </summary>
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            if (logLoad) Debug.Log($"Loading scene index: {sceneIndex}");
            StartCoroutine(LoadSceneWithDelay(sceneIndex));
        }
        else
        {
            Debug.LogError($"Scene index {sceneIndex} is out of range.");
        }
    }

    private System.Collections.IEnumerator LoadSceneWithDelay(string sceneName)
    {
        if (loadDelay > 0) yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneName);
    }

    private System.Collections.IEnumerator LoadSceneWithDelay(int sceneIndex)
    {
        if (loadDelay > 0) yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneIndex);
    }

    private bool IsSceneInBuild(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}
