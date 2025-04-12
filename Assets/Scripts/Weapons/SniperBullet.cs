using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy

public class SniperBullet : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float lifeTime = 0.5f; // Reduce lifetime significantly for visual bullet
    [SerializeField] int maxPenetration = 1;
    [SerializeField] float maxRayDistance = 100f; // Max distance the initial raycast travels

    [Header("Effects & References")]
    [SerializeField] GameObject bloodHitEffect;
    [SerializeField] GameObject sparksHitEffect;
    [SerializeField] GameObject damageNumber;

    [Header("Collision")]
    // Layers the initial Raycast should IGNORE. Also used by the visual bullet's trigger.
    [SerializeField] LayerMask ignoredLayers;
    // Layers the initial Raycast CAN hit (Enemies, Walls, etc.). Helps filter RaycastAll results.
    [SerializeField] LayerMask hittableLayers;

    // Internal State
    float damage;
    private Rigidbody2D rb;
    private Vector2 _direction; // Direction is crucial for the initial raycast
    // Note: hitEnemies and enemiesHitCount are now local to Start method

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("SniperBullet requires a Rigidbody2D component for visual movement!", this);
            enabled = false;
        }
    }

    // The core damage logic now happens instantly here
    void Start()
    {
        // Destroy the visual bullet after a short time
        Destroy(gameObject, lifeTime);

        // Ensure direction has been set by the weapon spawner
        if (_direction == Vector2.zero)
        {
            Debug.LogError("SniperBullet direction was not set before Start! Destroying bullet.", this);
            Destroy(gameObject);
            return;
        }

        // Perform the instant raycast
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, _direction, maxRayDistance, hittableLayers);

        // Sort hits by distance to process them in order
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        int enemiesHitCount = 0;
        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>(); // Track unique enemies hit by this raycast

        foreach (RaycastHit2D hit in hits)
        {
            // Skip ignored layers explicitly (double check, although hittableLayers should handle most)
            if (ignoredLayers.Contains(hit.collider.gameObject.layer))
            {
                continue;
            }

            // Check if it's an enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

                // If it's a valid enemy we haven't hit yet with this specific raycast
                if (enemyHealth != null && hitEnemies.Add(enemyHealth))
                {
                    // Apply damage
                    enemyHealth.TakeDamage(damage);

                    // Spawn effects at the actual hit point
                    CreateDamageNumber(enemyHealth.gameObject, hit.point);
                    CreateHitEffect(bloodHitEffect, false, hit.point); // Spawn blood at hit point

                    // Increment count and check penetration limit
                    enemiesHitCount++;
                    if (enemiesHitCount > maxPenetration)
                    {
                        // We have damaged the maximum number of enemies, stop processing hits
                        break;
                    }
                }
                // If already hit this enemy OR invalid enemy, continue raycast processing (penetration)
            }
            else
            {
                // Hit a non-enemy obstacle (wall, etc.)
                CreateHitEffect(sparksHitEffect, true, hit.point); // Spawn sparks at hit point
                // Stop processing further hits; the conceptual "bullet" stops here
                break;
            }
        }
        // Damage application is complete. The visual bullet continues briefly.
    }

    // --- Visual Bullet Collision Handling (Optional) ---
    // Keep the collider as a TRIGGER for the visual bullet
    // This function now only handles destroying the VISUAL bullet and its effects
    // when it hits a non-enemy obstacle.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent collision logic if the bullet object is already destroyed or pending
        if (this == null || !gameObject.activeSelf) return;

        // Ignore specified layers
        if (ignoredLayers.Contains(other.gameObject.layer))
        {
            return;
        }

        // If the VISUAL bullet hits anything other than an enemy
        if (!other.CompareTag("Enemy"))
        {
            // Spawn sparks where the visual bullet hits
            CreateHitEffect(sparksHitEffect, true, transform.position);
            // Destroy the visual bullet immediately
            DestroyVisualBullet();
        }
        // Do nothing if the visual bullet trigger hits an enemy - damage was instant via raycast.
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            CreateHitEffect(sparksHitEffect, true, collision.transform.position);
            Destroy(gameObject);
        }
    }

    // Creates a hit effect (blood/sparks) at a specific location
    void CreateHitEffect(GameObject hitEffectPrefab, bool reverseParticleDirection, Vector3 spawnPosition)
    {
        if (hitEffectPrefab == null) return;

        // We still use _direction for orienting the particle effect properly
        float angle = Mathf.Atan2(-_direction.y, -_direction.x) * Mathf.Rad2Deg;
        float angleCorrection = reverseParticleDirection ? 90f : -90f;
        Quaternion effectRotation = Quaternion.Euler(0, 0, angle + angleCorrection);

        // Instantiate effect at the specific hit point determined by the raycast or trigger
        Instantiate(hitEffectPrefab, spawnPosition, effectRotation);
    }

    // Creates the floating damage number text at a specific location
    void CreateDamageNumber(GameObject enemyGameObject, Vector3 hitPosition)
    {
        if (damageNumber == null) return;

        // Spawn near the actual hit point, slightly offset upwards
        Vector3 spawnPos = hitPosition + Vector3.up * 0.5f;
        GameObject spawnedNumber = Instantiate(damageNumber, spawnPos, Quaternion.identity);

        FloatingHealthNumber numberComponent = spawnedNumber.GetComponent<FloatingHealthNumber>();
        if (numberComponent != null)
        {
            numberComponent.SetText(damage.ToString());
        }
        else
        {
            Debug.LogError("Damage Number Prefab is missing the FloatingHealthNumber component!", damageNumber);
        }
    }

    // Method to cleanly destroy the visual bullet and stop its effects
    void DestroyVisualBullet()
    {
        // Stop Rigidbody movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true; // Prevent further physics interactions
        }
        // Destroy the main bullet GameObject
        Destroy(gameObject);
    }

    public void SetDirection(Vector2 dir)
    {
        _direction = dir.normalized;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    // Weapon script MUST call this *immediately* after Instantiate
    // It sets the direction for the initial raycast and visual bullet velocity
    public void Initialize(Vector2 direction, float speed)
    {
        _direction = direction.normalized;
        if (rb != null)
        {
            rb.linearVelocity = _direction * speed;
        }
        else
        {
            Debug.LogError("Cannot set velocity - Rigidbody2D is missing!", this);
        }
    }
}