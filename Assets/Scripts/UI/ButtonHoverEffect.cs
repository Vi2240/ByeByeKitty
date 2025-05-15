using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.1f;
    public float duration = 0.1f;

    private Vector3 originalScale;
    private RectTransform rectTransform;
    private Coroutine scaleCoroutine;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void PlayButtonPressSFX() // Never gets here for some reason
    {
        AudioPlayer.Current.PlaySfxAtPoint("ButtonPress", transform.position);
        print("should play");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        float elapsed = 0;
        Vector3 startScale = rectTransform.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out quadratic
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = targetScale;
    }
}