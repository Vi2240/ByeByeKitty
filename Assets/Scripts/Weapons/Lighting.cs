using UnityEngine;
using System.Collections;

public class Lighting : MonoBehaviour
{
    [SerializeField] private float TTL_Seconds = 0.2f;
    [SerializeField] private GameObject lightningStrikePrefab;
    [SerializeField] private float visualWidth = 0.1f;

    private Transform startAnchor = null;
    private Vector3 endPosition;
    private bool isAnchored = false;

    public void Initialize(Vector3 endPos, Transform anchor = null)
    {
        gameObject.SetActive(true);

        this.endPosition = endPos;
        this.startAnchor = anchor;
        this.isAnchored = (this.startAnchor != null);

        if (lightningStrikePrefab != null)
        {
            Instantiate(lightningStrikePrefab, this.endPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("LightningStrikePrefab not assigned in Lighting script.", this);
        }

        if (isAnchored)
        {
            UpdateVisualTransform();
        }

        StartCoroutine(LifetimeCoroutine());
    }

    void Update()
    {
        if (isAnchored && startAnchor != null)
        {
            UpdateVisualTransform();
        }
    }

    void UpdateVisualTransform()
    {
        if (startAnchor == null) return;

        Vector3 currentStartPosition = startAnchor.position;
        Vector3 direction = endPosition - currentStartPosition;
        float distance = direction.magnitude;

        if (distance > 0.01f)
        {
            transform.position = currentStartPosition + direction / 2.0f;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction.normalized);
            transform.localScale = new Vector3(visualWidth, distance, 1f);
        }
        else
        {
            transform.position = currentStartPosition;
            transform.rotation = Quaternion.identity;
            transform.localScale = new Vector3(visualWidth, 0.01f, 1f);
        }
    }

    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(TTL_Seconds);
        Destroy(gameObject);
    }
}