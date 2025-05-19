using UnityEngine;
using System.Collections; // Required for IEnumerator

public class DeathScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 2.0f; // How long the fade should take in seconds
    [SerializeField] private float delayBeforeFade = 1.0f; // Seconds to wait before starting the fade
    [SerializeField] private GameObject deathScreenRoot; // Assign your "DeathScreen" parent object here

    void Awake()
    {
        // Ensure the death screen is initially hidden and non-interactive
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is not assigned in the Inspector!", this); // Corrected variable name in log
            enabled = false; // Disable script if not set up
            return;
        }
        if (deathScreenRoot == null)
        {
            Debug.LogError("DeathScreenRoot is not assigned in the Inspector!", this);
            enabled = false;
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        deathScreenRoot.SetActive(false); // Keep the whole object inactive until needed
    }

    // Public method to be called when the player dies
    public void ShowDeathScreen()
    {
        if (canvasGroup == null || deathScreenRoot == null) return;

        // We still activate the root immediately, but the CanvasGroup's alpha
        // will keep it invisible until the fade starts.
        // If you prefer the root to also be activated after the delay,
        // move this SetActive(true) call into the FadeIn coroutine after the delay.
        deathScreenRoot.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        // Wait for the specified delay
        if (delayBeforeFade > 0)
        {
            yield return new WaitForSeconds(delayBeforeFade);
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = 0f; // Start fully transparent
        canvasGroup.interactable = false; // Ensure not interactive during fade
        canvasGroup.blocksRaycasts = false; // Ensure not blocking raycasts during fade

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null; // Wait for the next frame
        }

        // Ensure it's fully opaque and interactive at the end
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // Optional: Method to hide it again if needed (e.g., for testing)
    public void HideDeathScreen()
    {
        if (canvasGroup == null || deathScreenRoot == null) return;

        deathScreenRoot.SetActive(false); // Can just deactivate the root
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}