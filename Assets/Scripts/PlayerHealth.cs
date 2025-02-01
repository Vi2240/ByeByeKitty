using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;
    [SerializeField] HealthBar healthBar;

    float currentHealth = 0;

    void Start()
    {
        currentHealth = maxHealth;
        currentHealth -= 1;
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        healthBar.SetHealth(currentHealth, maxHealth);
        print("Player took " + damageAmount + " damage");
    }

    void Die()
    {
        // Death stuff
    }
}
