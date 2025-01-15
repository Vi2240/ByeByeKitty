using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;
    [SerializeField] float currentHealth;
    [SerializeField, Tooltip("Healing starts after X seconds out of combat.")] float timeToHeal = 3;
    [SerializeField] int healPerTick = 2;
    [SerializeField] float timeBetweenHealingTicks = 0.5f;

    float timer = 0;
    float timer2 = 0;
    bool outOfCombat = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= timeToHeal)
        {
            timer = 0;
            outOfCombat = true;
        }

        if (outOfCombat && currentHealth < maxHealth)
        {
            Heal();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        print("Took " + damageAmount + " damage");
        timer = 0;
        outOfCombat = false;
    }

    void Heal()
    {
        timer2 += Time.deltaTime;
        if (timer2 >= timeBetweenHealingTicks)
        {
            timer2 = 0;
            currentHealth += healPerTick;
            print("Healed " + healPerTick + " hp");

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }

    void Die()
    {
        // Add other death stuff here later
        GameObject drop = GetComponent<TempLootDropper>().GetRandomDrop();
        if (drop != null) { Instantiate(drop, transform.position, Quaternion.identity); }
        Destroy(gameObject);
    }
}