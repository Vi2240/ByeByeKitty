using Unity.Mathematics;
using UnityEngine;

public class MachineGunBullet : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float lifetime = 1f;

    [Header("Effects")]
    [SerializeField] float hitEffectTime = 1f;
    [SerializeField] GameObject bloodHitEffect;
    [SerializeField] GameObject sparksHitEffect;
    [SerializeField] GameObject damageNumber;

    [Header("Collision")]
    [SerializeField] LayerMask ignoredLayers;

    // Internal
    float damage;
    private Vector2 _direction;
    private bool hasDamaged = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (ignoredLayers.Contains(other.gameObject.layer)) return;

        if (other.CompareTag("Enemy") && !hasDamaged)
        {
            DamageEnemy(other.gameObject);
        }
        //else { CreateHitEffect(sparksHitEffect, true); }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !hasDamaged)
        {
            DamageEnemy(collision.gameObject);
        }
        else if (!collision.gameObject.CompareTag("Enemy")) { CreateHitEffect(sparksHitEffect, true); }
        else { Destroy(gameObject); }
    }

    void DamageEnemy(GameObject enemy)
    {
        if (enemy.GetComponent<EnemyHealth>().TakeDamage(damage))
        {            
            hasDamaged = true;
            CreateDamageNumber(enemy);
            CreateHitEffect(bloodHitEffect, false);
            return;
        }
        else { Destroy(gameObject); }
    }

    void CreateHitEffect(GameObject hitEffect, bool reverseParticleDirection)
    {
        // If effect should face the shooter or opposite direciton
        short angleCorrection = 270;
        if (reverseParticleDirection) { angleCorrection = 90; }
        if (hitEffect == null) { print("Missing particle effect"); return; }
        
        // Spawn particle effect
        float angle = Mathf.Atan2(-_direction.y, -_direction.x) * Mathf.Rad2Deg;
        Quaternion effectRotation = Quaternion.Euler(0, 0, angle - angleCorrection);
        Instantiate(hitEffect, transform.position, effectRotation, null);
        Destroy(gameObject);
    }

    void CreateDamageNumber(GameObject enemy)
    {
        var spawned = Instantiate(damageNumber, enemy.transform.position, Quaternion.identity);
        spawned.GetComponent<FloatingHealthNumber>().SetText(damage.ToString());
    }

    public void SetDirection(Vector2 dir)
    {
        _direction = dir.normalized;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
}

public static class LayerMaskExtensions
{
    public static bool Contains(this LayerMask mask, int layer)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}