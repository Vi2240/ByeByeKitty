using UnityEngine;
using TMPro;

public class FloatingHealthNumber : MonoBehaviour
{
    Rigidbody2D _rigidBody;

    [Header("Text Components")]
    [SerializeField] private TMP_Text redTextComponent;
    [SerializeField] private TMP_Text orangeTextComponent;

    [Header("Movement Settings")]
    [SerializeField] float initialYVelocity = 7f;
    [SerializeField] float initialXVelocityRange = 3f;
    [SerializeField] float lifetime = 0.8f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        // Ensure components are assigned
        if (redTextComponent == null)
        {
            Debug.LogError("RedTextComponent is not assigned on " + gameObject.name, this);
        }
        if (orangeTextComponent == null)
        {
            Debug.LogError("OrangeTextComponent is not assigned on " + gameObject.name, this);
        }

        if (redTextComponent != null) redTextComponent.gameObject.SetActive(false);
        if (orangeTextComponent != null) orangeTextComponent.gameObject.SetActive(false);
    }

    private void Start()
    {
        _rigidBody.linearVelocity = new Vector2(Random.Range(-initialYVelocity, initialXVelocityRange), initialYVelocity);
        Destroy(gameObject, lifetime);
    }

    public void SetText(string text, bool useRedText)
    {
        TMP_Text activeTextComponent = null;
        redTextComponent.gameObject.SetActive(useRedText);
        orangeTextComponent.gameObject.SetActive(!useRedText);
        activeTextComponent = (useRedText) ? redTextComponent : orangeTextComponent;
        activeTextComponent.SetText(text);
    }

    public void SetText(string text)
    {
        orangeTextComponent.gameObject.SetActive(false);
        redTextComponent.gameObject.SetActive(true);
        redTextComponent.SetText(text);
    }
}