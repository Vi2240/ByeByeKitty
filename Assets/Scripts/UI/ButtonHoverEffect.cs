using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler // Add IPointerDownHandler
{
    public float hoverScaleMultiplier = 1.1f; // Renamed for clarity
    public float pressScaleMultiplier = 0.95f; // Optional: scale down on press
    public float duration = 0.1f;

    private Vector3 originalScale;
    private RectTransform rectTransform;
    private Coroutine scaleCoroutine;

    void Awake() // Use Awake for initialization
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
        }
        else
        {
            Debug.LogError("ButtonHoverEffect: RectTransform not found on " + gameObject.name, this);
            enabled = false; // Disable script if no RectTransform
        }
    }

    // This method is intended to be called by the Button's OnClick() event in the Inspector
    public void PlayButtonPressSFX()
    {
        // Ensure AudioPlayer exists and is accessible
        if (AudioPlayer.Current != null)
        {
            // PlaySfxAtPoint might not be ideal for UI if it's 3D.
            // Consider using PlayUISfx if "ButtonPress" is a 2D sound.
            // If "ButtonPress" is meant to be a 3D sound at the button's UI position,
            // then transform.position is okay, but it might sound weird if your AudioListener is far.
            AudioPlayer.Current.PlayUISfx("ButtonPress"); // Assuming ButtonPress is a 2D UI sound
            Debug.Log("PlayButtonPressSFX called on " + gameObject.name);
        }
        else
        {
            Debug.LogWarning("AudioPlayer.Current is null. Cannot play ButtonPress SFX.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enabled || rectTransform == null) return; // Ensure script is active and rectTransform exists

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * hoverScaleMultiplier, duration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled || rectTransform == null) return;

        // If not currently scaling down from a press, scale back to original
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale, duration));
    }

    // Optional: Add effect for button press
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enabled || rectTransform == null) return;

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        // Scale down slightly on press, then the OnClick will handle the SFX
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * pressScaleMultiplier, duration / 2)); // Faster press scale

        // Note: The button's actual OnClick event (where PlayButtonPressSFX is hooked up)
        // will fire on PointerUp (when the click is completed).
        // If you want the sound on PointerDown, you'd call it here directly.
    }


    private IEnumerator ScaleTo(Vector3 targetScale, float animDuration)
    {
        float elapsed = 0;
        Vector3 startScale = rectTransform.localScale;

        while (elapsed < animDuration)
        {
            // Check if rectTransform became null (e.g. object destroyed)
            if (rectTransform == null) yield break;

            float t = elapsed / animDuration;
            // Ease-out (decelerate)
            // t = Mathf.Sin(t * Mathf.PI * 0.5f); // Sinusoidal ease-out
            // Or a simple quadratic ease-out:
            t = 1f - (1f - t) * (1f - t);

            rectTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t); // LerpUnclamped for overshooting if desired
            elapsed += Time.unscaledDeltaTime; // Use unscaledDeltaTime for UI animations that should work when Time.timeScale is 0
            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
        }
        scaleCoroutine = null; // Clear the coroutine reference
    }

    // Ensure to reset scale if the object is disabled while scaled
    void OnDisable()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
        if (rectTransform != null && originalScale != Vector3.zero) // originalScale check for safety
        {
            rectTransform.localScale = originalScale;
        }
    }
}