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
        if (pauseTimeAtStart)
        {
            Time.timeScale = 0f;
            //AudioListener.pause = true;
            GameIsPaused = true;
        }
        else
        {
            // Ensure the pause menu is hidden at the start
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(false);
            }
            // Ensure game is not paused at start (in case of scene reloads while paused)
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameIsPaused = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Or any other key you prefer
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        Time.timeScale = 0f;
        AudioListener.pause = true;
        GameIsPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameIsPaused = false;
    }

    public void LoadOptionsMenu()
    {
        // For now, just a placeholder. You would typically load an options scene
        // or activate an options panel.
        Debug.Log("Options Button Clicked - Implement Options Menu!");
        // Example: if you have an options panel on the same canvas:
        // optionsMenuUI.SetActive(true);
        // pauseMenuUI.SetActive(false); // Hide pause menu if opening another panel
    }

    public void LoadMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            // IMPORTANT: Always unpause time before loading a new scene
            // if the game was paused, otherwise the new scene might start paused.
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameIsPaused = false; // Reset pause state

            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log("Loading Main Menu: " + mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("MainMenuSceneName is not set in PauseMenu script. Cannot load main menu.");
            // If no main menu, you might just want this button to quit
            // QuitGame();
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        // IMPORTANT: Always unpause time if quitting from a paused state
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Application.Quit();

        // If running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}