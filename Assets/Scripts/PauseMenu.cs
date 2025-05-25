using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false; // Static to be easily accessible from other scripts

    [Header("UI Elements")]
    [SerializeField] private GameObject pauseMenuUI; // Assign your PauseMenuPanel here in the Inspector

    [Header("Settings")]
    [Tooltip("Name of your Main Menu scene. Leave empty if you only want to quit.")]
    [SerializeField] private string mainMenuSceneName = "MenuScene"; // Change this to your actual main menu scene name
    [SerializeField] bool pauseTimeAtStart = false;

    void Start()
    {
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        if (pauseTimeAtStart) {
            PauseTime();
            AudioListener.pause = false;
        }
        else {
            Resume();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (GameIsPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        if (pauseMenuUI) pauseMenuUI.SetActive(true);
        else { print("pauseMenuUI is null"); }
        PauseTime();
    }

    public void Resume()
    {
        if (pauseMenuUI != null) { pauseMenuUI.SetActive(false); }
        ResumeTime();
    }

    public void LoadOptionsMenu()
    {
        Debug.Log("Options Button Clicked - Implement Options Menu!");
    }

    public void LoadMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName)) {
            ResumeTime();

            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log("Loading Main Menu: " + mainMenuSceneName);
        }
        else {
            Debug.LogWarning("MainMenuSceneName is not set in PauseMenu script. Cannot load main menu.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        ResumeTime();
        Application.Quit();

        // If running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void PauseTime()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        GameIsPaused = true;
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = 1;
        GameIsPaused = false;
    }
}