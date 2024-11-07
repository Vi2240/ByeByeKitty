using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///  Remember to add Manager scene to Scenes in Build Settings
/// </summary>

public class SceneInstantiate : MonoBehaviour
{
    [SerializeField] private Object persistentScene;

    private void Awake()
    {
        SceneManager.LoadSceneAsync(persistentScene.name, LoadSceneMode.Additive);
    }

    [ContextMenu("ChangeScene")]
    public void NextScene()
    {
        SceneManager.UnloadSceneAsync("FirstScene");
        SceneManager.LoadSceneAsync("SecondScene", LoadSceneMode.Additive);
    }
}
