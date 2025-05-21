using UnityEngine;
using System.Collections; // Required for IEnumerator

public class EndScreenFader : MonoBehaviour
{
    [Header("Screen Settings")]
    [SerializeField] private float fadeDuration = 2.0f; // How long the fade should take in seconds
    [SerializeField] private float delayBeforeFade = 1.0f; // Seconds to wait before starting the fade

    [Header("Death Screen (Optional)")]
    [SerializeField] private CanvasGroup deathScreenCanvasGroup;
    [SerializeField] private GameObject deathScreenRoot;

    [Header("Win Screen (Optional)")]
    [SerializeField] private CanvasGroup winScreenCanvasGroup;
    [SerializeField] private GameObject winScreenRoot;


    void Awake()
    {
        // Initialize Death Screen (if assigned)
        if (deathScreenCanvasGroup != null && deathScreenRoot != null)
        {
            deathScreenCanvasGroup.alpha = 0f;
            deathScreenCanvasGroup.interactable = false;
            deathScreenCanvasGroup.blocksRaycasts = false;
            deathScreenRoot.SetActive(false);
        }
        else if (deathScreenCanvasGroup != null || deathScreenRoot != null)
        {
            // Log a warning if only one part is assigned
            Debug.LogWarning("Death Screen setup is incomplete. Both CanvasGroup and Root GameObject should be assigned, or neither.", this);
        }

        // Initialize Win Screen (if assigned)
        if (winScreenCanvasGroup != null && winScreenRoot != null)
        {
            winScreenCanvasGroup.alpha = 0f;
            winScreenCanvasGroup.interactable = false;
            winScreenCanvasGroup.blocksRaycasts = false;
            winScreenRoot.SetActive(false);
        }
        else if (winScreenCanvasGroup != null || winScreenRoot != null)
        {
            Debug.LogWarning("Win Screen setup is incomplete. Both CanvasGroup and Root GameObject should be assigned, or neither.", this);
        }
    }

    // --- Public Methods to Trigger Screens ---

    public void ShowDeathScreen()
    {
        if (deathScreenCanvasGroup == null || deathScreenRoot == null)
        {
            Debug.LogWarning("Attempted to show Death Screen, but it's not fully configured.", this);
            return;
        }
        // Hide win screen if it's somehow active
        HideWinScreen(false); // false means no fade, just immediate hide

        deathScreenRoot.SetActive(true);
        StartCoroutine(FadeInCanvasGroup(deathScreenCanvasGroup));
    }

    public void ShowWinScreen()
    {
        if (winScreenCanvasGroup == null || winScreenRoot == null)
        {
            Debug.LogWarning("Attempted to show Win Screen, but it's not fully configured.", this);
            return;
        }
        // Hide death screen if it's somehow active
        HideDeathScreen(false); // false means no fade, just immediate hide

        winScreenRoot.SetActive(true);
        StartCoroutine(FadeInCanvasGroup(winScreenCanvasGroup));
    }

    // --- Coroutine for Fading ---

    private IEnumerator FadeInCanvasGroup(CanvasGroup targetCanvasGroup)
    {
        // Wait for the specified delay
        if (delayBeforeFade > 0)
        {
            yield return new WaitForSeconds(delayBeforeFade);
        }

        float elapsedTime = 0f;
        targetCanvasGroup.alpha = 0f; // Start fully transparent
        targetCanvasGroup.interactable = false; // Ensure not interactive during fade
        targetCanvasGroup.blocksRaycasts = false; // Ensure not blocking raycasts during fade

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            targetCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null; // Wait for the next frame
        }

        // Ensure it's fully opaque and interactive at the end
        targetCanvasGroup.alpha = 1f;
        targetCanvasGroup.interactable = true;
        targetCanvasGroup.blocksRaycasts = true;
    }


    public void HideDeathScreen(bool useFade = true) // Added optional parameter for immediate hide
    {
        if (deathScreenCanvasGroup != null && deathScreenRoot != null && deathScreenRoot.activeSelf)
        {
            if (useFade)
            {
                StartCoroutine(FadeOutCanvasGroup(deathScreenCanvasGroup, deathScreenRoot));
            }
            else
            {
                deathScreenRoot.SetActive(false);
                deathScreenCanvasGroup.alpha = 0f;
                deathScreenCanvasGroup.interactable = false;
                deathScreenCanvasGroup.blocksRaycasts = false;
            }
        }
    }

    public void HideWinScreen(bool useFade = true) // Added optional parameter for immediate hide
    {
        if (winScreenCanvasGroup != null && winScreenRoot != null && winScreenRoot.activeSelf)
        {
            if (useFade)
            {
                StartCoroutine(FadeOutCanvasGroup(winScreenCanvasGroup, winScreenRoot));
            }
            else
            {
                winScreenRoot.SetActive(false);
                winScreenCanvasGroup.alpha = 0f;
                winScreenCanvasGroup.interactable = false;
                winScreenCanvasGroup.blocksRaycasts = false;
            }
        }
    }

    private IEnumerator FadeOutCanvasGroup(CanvasGroup targetCanvasGroup, GameObject targetRoot)
    {
        float elapsedTime = 0f;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;

        // Assuming current alpha is 1f
        float startAlpha = targetCanvasGroup.alpha;


        while (elapsedTime < fadeDuration) // Using same fadeDuration for fade out
        {
            elapsedTime += Time.deltaTime;
            targetCanvasGroup.alpha = Mathf.Clamp01(startAlpha - (elapsedTime / fadeDuration));
            yield return null;
        }

        targetCanvasGroup.alpha = 0f;
        targetRoot.SetActive(false);
    }
}