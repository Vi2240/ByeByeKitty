using UnityEngine;
using TMPro;

public class FloatingHealthNumber : MonoBehaviour
{
    Rigidbody2D _rigidBody;
    TMP_Text _damageValue;

    [SerializeField] float initialYVelocity = 7f;
    [SerializeField] float initialXVelocityRange = 3f;
    [SerializeField] float lifetime = 0.8f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _damageValue = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        _rigidBody.linearVelocity = new Vector2(Random.Range(-initialYVelocity, initialXVelocityRange), initialYVelocity);
        Destroy(gameObject, lifetime);
    }

    public void SetText(string text)
    {
        if (text == null)
        { 
            _damageValue.SetText("NULL");
            return; 
        }
        if (_damageValue == null) 
        {
            print("Is Null");
            return;
        }
        _damageValue.SetText(text);
    }
}