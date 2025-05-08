using UnityEngine;
using System.Collections.Generic;

public class SniperBullet : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float lifeTime = 0.5f;
    [SerializeField] int maxPenetration = 1;
    [SerializeField] float maxRayDistance = 100f;

    [Header("Effects & References")]
    [SerializeField] GameObject bloodHitEffect;
    [SerializeField] GameObject sparksHitEffect;
    [SerializeField] GameObject damageNumber;

    [Header("Collision")]
    [SerializeField] LayerMask ignoredLayers; // Raycast ignores these layers.
    [SerializeField] LayerMask hittableLayers; // Raycast hits these layers.


    float damage;
    private Rigidbody2D rb;
    private Vector2 _direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("SniperBullet requires a Rigidbody2D component for visual movement!", this);
            enabled = false;
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);

        if (_direction == Vector2.zero)
        {
            Debug.LogError("SniperBullet direction was not set before Start! Destroying bullet.", this);
            Destroy(gameObject);
            return;
        }

        // Shoot raycast
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, _direction, maxRayDistance, hittableLayers);

        // Sort hits by distance to process them in order.
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        int enemiesHitCount = 0;
        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>(); // Track which enemies get hit.

        foreach (RaycastHit2D hit in hits)
        {
            if (ignoredLayers.Contains(hit.collider.gameObject.layer)) continue;

            // -- Hit obstacle --
            if (!hit.collider.CompareTag("Enemy")) 
            {
                CreateHitEffect(sparksHitEffect, true, hit.point);
                break;
            }

            // -- Hit Enemy --
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

            if (enemyHealth != null && hitEnemies.Add(enemyHealth))
            {
                enemyHealth.TakeDamage(damage);

                CreateDamageNumber(enemyHealth.gameObject, hit.point);
                CreateHitEffect(bloodHitEffect, false, hit.point);

                // Increase count and check penetration limit.
                enemiesHitCount++;
                if (enemiesHitCount > maxPenetration) break;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent collision logic if the bullet object is already destroyed.
        if (this == null || !gameObject.activeSelf) return;

        // Ignore specified layers.
        if (ignoredLayers.Contains(other.gameObject.layer)) return;

        // If the visual bullet hits anything other than an enemy
        if (!other.CompareTag("Enemy"))
        {
            CreateHitEffect(sparksHitEffect, true, transform.position);
            DestroyVisualBullet();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            CreateHitEffect(sparksHitEffect, true, collision.transform.position);
            Destroy(gameObject);
        }
    }

    void CreateHitEffect(GameObject hitEffectPrefab, bool reverseParticleDirection, Vector3 spawnPosition)
    {
        if (hitEffectPrefab == null) return;

        float angle = Mathf.Atan2(-_direction.y, -_direction.x) * Mathf.Rad2Deg;
        float angleCorrection = reverseParticleDirection ? 90f : -90f;
        Quaternion effectRotation = Quaternion.Euler(0, 0, angle + angleCorrection);
        Instantiate(hitEffectPrefab, spawnPosition, effectRotation);
    }

    void CreateDamageNumber(GameObject enemyGameObject, Vector3 hitPosition)
    {
        if (damageNumber == null) return;

        // Spawn near the actual hit point, slightly above.
        Vector3 spawnPos = hitPosition + Vector3.up * 0.5f;
        GameObject spawnedNumber = Instantiate(damageNumber, spawnPos, Quaternion.identity);

        FloatingHealthNumber numberComponent = spawnedNumber.GetComponent<FloatingHealthNumber>();
        if (numberComponent != null) { numberComponent.SetText(damage.ToString()); }
        else { Debug.LogError("Damage Number Prefab is missing the FloatingHealthNumber component!", damageNumber); }
    }

    void DestroyVisualBullet()
    {
        // Stop Rigidbody movement.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true; // Prevent further physics interactions.
        }
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

    // Weapon script must call this immediately after Instantiate. It sets the direction for the initial raycast and visual bullet velocity.
    public void Initialize(Vector2 direction, float speed)
    {
        _direction = direction.normalized;
        if (rb != null) { rb.linearVelocity = _direction * speed; }
        else { Debug.LogError("Cannot set velocity - Rigidbody2D is missing!", this); }
    }
}