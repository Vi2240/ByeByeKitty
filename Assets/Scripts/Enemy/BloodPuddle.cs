using UnityEngine;
using System.Collections;

public class BloodPuddle : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] float timeBeforeFading = 3f;
    [SerializeField] float fadeDuration = 2f;

    [Header("Size Variation")]
    [SerializeField] float minScaleFactor = 0.75f;
    [SerializeField] float maxScaleFactor = 1.25f;

    [Header("Rotation Variation")]
    [SerializeField] float maxRotationAngle = 4f;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found", this);
            Destroy(gameObject);
            return;
        }

        ApplyRandomTransformations();
        StartCoroutine(FadeOut());
    }

    void ApplyRandomTransformations()
    {
        // Random size variation
        float scaleFactor = Random.Range(minScaleFactor, maxScaleFactor);
        transform.localScale *= scaleFactor;

        // Random rotation
        float randomAngle = Random.Range(-maxRotationAngle, maxRotationAngle);
        transform.rotation = Quaternion.Euler(0f, 0f, randomAngle);
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(timeBeforeFading);

        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}