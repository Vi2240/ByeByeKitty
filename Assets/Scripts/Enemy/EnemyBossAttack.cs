using UnityEngine;

public class EnemyBossAttack : MonoBehaviour
{
    // General Settings
    [Header("General Settings")]
    public bool disableAttacking = false;

    // Projectile Attack Settings
    [Header("Projectile Attack")]
    [SerializeField] bool shootProjectiles = true;
    [SerializeField, Tooltip("Number of projectiles to fire in a spread. First projectile aims at player.")]
    int projectiles = 8;
    [SerializeField] float projectileAttackCooldown = 2f;
    [SerializeField] float projectileAttackDamage = 50f;
    [SerializeField] float bulletForce = 15f;
    [SerializeField] float bulletScaleMultiplier = 1.5f;
    [SerializeField] Transform bulletSpawnPos;
    [SerializeField] GameObject bulletPrefab;

    // Melee Attack Settings
    [Header("Melee Attack")]
    [SerializeField] bool meleeAttack = true;
    [SerializeField] float meleeAttackRange = 1.5f;
    [SerializeField] float meleeAttackCooldown = 0.1f;
    [SerializeField] float meleeAttackDamage = 5f;

    EnemyMovement movementScript;
    GameObject nearestPlayer;
    float projectileTimer;
    float meleeTimer;

    void Start()
    {
        movementScript = gameObject.GetComponent<EnemyMovement>();
        if (movementScript == null)
        {
            Debug.LogError("EnemyMovement script not found on EnemyBossAttack GameObject. Disabling attacking.", this);
            disableAttacking = true;
        }

        if (shootProjectiles && bulletSpawnPos == null)
        {
            Debug.LogError("Bullet Spawn Position (bulletSpawnPos) is not assigned, but shootProjectiles is enabled. Projectile attacks may fail or cause errors.", this);
        }
        if (shootProjectiles && bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab (bulletPrefab) is not assigned, but shootProjectiles is enabled. Projectile attacks will fail.", this);
        }
    }

    void Update()
    {
        if (disableAttacking) { return; }

        nearestPlayer = movementScript.FindNearestObject("Player");
        if (nearestPlayer == null) { return; }

        float distanceToPlayer = movementScript.DistanceTo(nearestPlayer);

        if (shootProjectiles)
        {
            projectileTimer += Time.deltaTime;
            float currentProjectileCooldown = projectileAttackCooldown;
            if (InventoryAndBuffs.enemyAttackSpeedMultiplier > 0.001f)
            {
                currentProjectileCooldown /= InventoryAndBuffs.enemyAttackSpeedMultiplier;
            }
            if (projectileTimer >= currentProjectileCooldown)
            {
                PerformProjectileSpreadAttack();
                projectileTimer = 0f;
            }
        }

        if (meleeAttack)
        {
            meleeTimer += Time.deltaTime;
            float currentMeleeCooldown = meleeAttackCooldown;
            if (InventoryAndBuffs.enemyAttackSpeedMultiplier > 0.001f)
            {
                currentMeleeCooldown /= InventoryAndBuffs.enemyAttackSpeedMultiplier;
            }
            if (meleeTimer >= currentMeleeCooldown)
            {
                if (distanceToPlayer <= meleeAttackRange)
                {
                    PerformMeleeAttack();
                    meleeTimer = 0f;
                }
            }
        }
    }

    void PerformProjectileSpreadAttack()
    {
        if (bulletPrefab == null || bulletSpawnPos == null || projectiles <= 0) return;
        if (nearestPlayer == null) return;

        Vector2 directionToPlayer = (Vector2)nearestPlayer.transform.position - (Vector2)bulletSpawnPos.position;
        float baseAngleDeg = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        float angleStepDeg = 360f / projectiles;

        for (int i = 0; i < projectiles; i++)
        {
            float currentAngleDeg = baseAngleDeg + (i * angleStepDeg);
            Vector2 projectileDirection = new Vector2(
                Mathf.Cos(currentAngleDeg * Mathf.Deg2Rad),
                Mathf.Sin(currentAngleDeg * Mathf.Deg2Rad)
            );

            GameObject bulletInstance = Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.identity);
            // Scale up bullet here by multiplier
            bulletInstance.transform.localScale = new Vector3(
                bulletInstance.transform.localScale.x * bulletScaleMultiplier,
                bulletInstance.transform.localScale.y * bulletScaleMultiplier,
                bulletInstance.transform.localScale.z
            );
            EnemyBullet bulletScript = bulletInstance.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(projectileAttackDamage * InventoryAndBuffs.enemyDamageMultiplier);
            }

            bulletInstance.transform.rotation = Quaternion.Euler(0f, 0f, currentAngleDeg - 90f);
            Rigidbody2D rb = bulletInstance.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(projectileDirection * bulletForce, ForceMode2D.Impulse);
            }
        }
    }

    void PerformMeleeAttack()
    {
        if (nearestPlayer == null) return;

        PlayerHealth playerHealth = nearestPlayer.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(meleeAttackDamage * InventoryAndBuffs.enemyDamageMultiplier);
        }
    }
}