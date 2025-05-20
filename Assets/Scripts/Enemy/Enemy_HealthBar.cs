using UnityEngine;
using UnityEngine.UI;

public class Enemy_HealthBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] GameObject fillArea;
    [SerializeField] Color low;
    [SerializeField] Color high;
    [SerializeField] Vector3 offset;

    public void SetHealth(float health, float maxHealth)
    {
        slider.gameObject.SetActive(health > 0);
        if (fillArea) { fillArea.SetActive(health > 0); }
        slider.gameObject.SetActive(health < maxHealth);
        slider.maxValue = maxHealth;
        slider.value = health;
        slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(low, high, slider.normalizedValue);
    }

    void Update()
    {
        slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + offset);
    }
}
