using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] bool isBoss = false;
    [Header("Health")]
    [SerializeField] float maxHealth = 100;
    [SerializeField] float currentHealth;
    [SerializeField] Enemy_HealthBar healthBar;

    [Header("Healing")]
    [SerializeField] bool passiveHealing = true;
    [SerializeField, Tooltip("Healing starts after X seconds out of combat.")] float timeToHeal = 3;
    [SerializeField] int healPerTick = 2;
    [SerializeField] float timeBetweenHealingTicks = 0.5f;

    [Header("Blood")]
    [SerializeField] GameObject bloodExplosion;
    [SerializeField] GameObject[] bloodPuddles;

    [Header("Loot")]
    [SerializeField, Tooltip("Overrides the loot table set by LootManager.")] LootTable lootTableOverride;
    [SerializeField] int minDrops = 1;
    [SerializeField] int maxDrops = 1;


    float timer = 0;
    float timer2 = 0;
    bool outOfCombat = false;
    bool immune = false;
    LootManager lootManager;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    void FixedUpdate()
    {
        if (passiveHealing)
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
        if (isBoss)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                enemy.GetComponent<EnemyHealth>().TakeDamage(100000);
            }
            GameObject.FindGameObjectWithTag("EndScreenManager").GetComponent<EndScreenFader>().ShowWinScreen();
        }
        AudioPlayer.Current.PlaySfxAtPoint("BloodExplosion", transform.position, 0.5f);
        if (bloodExplosion) { Instantiate(bloodExplosion, transform.position, quaternion.identity); }; // Blood splatter
        if (bloodPuddles.Length > 0) 
        {
            Instantiate(bloodPuddles[UnityEngine.Random.Range(0, bloodPuddles.Length)], transform.position, Quaternion.identity);
        }

        if (LootManager.Instance != null)
        {
            LootManager.Instance.RequestLootDrop(
                transform.position,
                minDrops,
                maxDrops,
                lootTableOverride
            );
        }
        else
        {
            Debug.LogError("LootManager Instance not found in scene! Cannot drop loot.", this);
        }

        Destroy(gameObject);
    }

    public IEnumerator TemporaryImmunity(float seconds)
    {
        immune = true;
        yield return new WaitForSeconds(seconds);
        immune = false;
    }
}