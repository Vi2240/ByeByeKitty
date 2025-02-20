using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;

    [Header("Passive healing")]
    [SerializeField] bool passiveHealing = false;
    [SerializeField, Tooltip("Healing starts after X seconds out of combat.")] float timeBeforeHeal = 3;
    [SerializeField] float healPerTick = 0.5f;
    [SerializeField] float ticksPerSecond = 8;
    float timeBetweenHealingTicks = 0;
    bool inCombat = false;

    UI_HealthBar healthBar;

    float currentHealth = 0;
    float timer = 0;
    float timer2 = 0;
        
    void Start()
    {
        healthBar = GameObject.FindGameObjectWithTag("UI_HealthBar").GetComponent<UI_HealthBar>();
        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth, maxHealth);
        timeBetweenHealingTicks = 1/ticksPerSecond;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) { TakeDamage(10); } // Temporary

        if (currentHealth <= 0)
        {
            Die();
        }

        if (!inCombat && currentHealth < maxHealth)
        {
            PassiveHeal();
        }

        timer += Time.deltaTime;
        if (timer >= timeBeforeHeal)
        {
            timer = 0;
            inCombat = false;
        }
    }

    void PassiveHeal()
    {
        timer2 += Time.deltaTime;
        if (timer2 >= timeBetweenHealingTicks)
        {
            timer2 = 0;
            currentHealth += healPerTick;
            if (currentHealth > maxHealth) { currentHealth = maxHealth; }
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        inCombat = true;
        currentHealth -= damageAmount;
        if (currentHealth < 0) { currentHealth = 0; }
        healthBar.SetHealth(currentHealth, maxHealth);
        print("Player took " + damageAmount + " damage");
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth) { currentHealth = maxHealth; }
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    void Die()
    {
        // Death stuff
    }
}