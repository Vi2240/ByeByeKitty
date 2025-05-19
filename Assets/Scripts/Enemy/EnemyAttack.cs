using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    // Serialize fields    
    public bool disableAttacking = false;
    [SerializeField, Tooltip("0 = melee, 1 = ranged")] int attackType = 0;
    [SerializeField] float attackRange = 0;
    [SerializeField] float bulletForce = 20;
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] float attackDamage = 5f;
    [SerializeField] Transform bulletSpawnPos;
    [SerializeField] GameObject bulletPrefab;    

    EnemyMovement movementScript;
    GameObject nearestPlayer;
    float timer;

    void Start()
    {
        movementScript = gameObject.GetComponent<EnemyMovement>();
    }

    void Update()
    {
        if (disableAttacking) { return; }

        timer += Time.deltaTime;
        nearestPlayer = movementScript.FindNearestObject("Player");
        if (nearestPlayer == null) { return; }

        if (nearestPlayer && movementScript.DistanceTo(nearestPlayer) > attackRange) { return; } // Return if it's too far away to skip code below

        if (timer >= attackCooldown)
        {
            if (attackType == 0)
            {
                MeleeAttack();
            }
            else if (attackType == 1)
            {
                RangedAttack();
            }

            timer = 0;
        }   
    }

    void RangedAttack()
    {
        print("Enemy used ranged attack");

        // Get the direction to the nearest player
        Vector2 direction = nearestPlayer.transform.position - bulletSpawnPos.position;
        //direction.Normalize(); // Normalize the direction vector to get a unit vector (magnitude of 1)

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));

        Rigidbody2D rigidbody = bullet.GetComponent<Rigidbody2D>();
        rigidbody.AddForce(direction * bulletForce, ForceMode2D.Impulse);
    }

    void MeleeAttack()
    {
        print("Enemy used melee attack");
        var playerHealth = nearestPlayer.GetComponent<PlayerHealth>();
        playerHealth.TakeDamage(attackDamage * InventoryAndBuffs.enemyDamageMultiplier);
    }
}