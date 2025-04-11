using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;
    [SerializeField] float currentHealth;
    [SerializeField, Tooltip("Healing starts after X seconds out of combat.")] float timeToHeal = 3;
    [SerializeField] int healPerTick = 2;
    [SerializeField] float timeBetweenHealingTicks = 0.5f;
    [SerializeField] Enemy_HealthBar healthBar;
    [SerializeField] GameObject bloodExplosion;
    [SerializeField] GameObject[] bloodPuddles;

    float timer = 0;
    float timer2 = 0;
    bool outOfCombat = false;
    bool immune = false;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth, maxHealth);
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

    /// <summary>
    /// Returns true if enemy took damage, false if not.
    /// </summary>
    /// <param name="damageAmount"></param>
    /// <returns></returns>
    public bool TakeDamage(float damageAmount)
    {
        if (immune)
        {
            return false;
        }

        currentHealth -= damageAmount;
        healthBar.SetHealth(currentHealth, maxHealth);
        //print("Took " + damageAmount + " damage");
        timer = 0;
        outOfCombat = false;
        return true;
    }

    void Heal()
    {
        timer2 += Time.deltaTime;
        if (timer2 >= timeBetweenHealingTicks)
        {
            timer2 = 0;
            currentHealth += healPerTick;
            healthBar.SetHealth(currentHealth, maxHealth);
            //print("Healed " + healPerTick + " hp");

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }

    void Die()
    {
        // Add other death stuff here later
        if (bloodExplosion) { Instantiate(bloodExplosion, transform.position, quaternion.identity); }; // Blood splatter
        if (bloodPuddles.Length > 0) 
        {
            Instantiate(bloodPuddles[UnityEngine.Random.Range(0, bloodPuddles.Length)], transform.position, Quaternion.identity);
        }

        gameObject.GetComponent<LootDropper>().AttemptDropItems();
        Destroy(gameObject);
    }

    public IEnumerator TemporaryImmunity(float seconds)
    {
        immune = true;
        yield return new WaitForSeconds(seconds);
        immune = false;
    }
}