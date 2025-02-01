using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float lifeTime = 2f;
    [SerializeField] float hitEffectTime = 1;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] LayerMask ignoredLayers; 

    public float damage = 0; // Changing this does nothing. Accessed by shooting script.
    private Vector2 _direction;

    void Start()
    {
        _direction = GetComponent<Rigidbody2D>().linearVelocity.normalized;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Skip ignored layers (like objectives)
        if (ignoredLayers.Contains(other.gameObject.layer)) return;

        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyHealth>().TakeDamage(damage);
        }
        CreateHitEffect();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Skip ignored layers
        if (ignoredLayers.Contains(collision.gameObject.layer)) { return; }

        CreateHitEffect();
    }

    void CreateHitEffect()
    {
        if (hitEffectPrefab == null) { return; }

        // Get rotation from bullet direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        Quaternion effectRotation = Quaternion.Euler(-90, 0, angle);
        print(angle.ToString());
        GameObject effect = Instantiate(hitEffectPrefab, transform.position, effectRotation);

        Destroy(effect, hitEffectTime);
        Destroy(gameObject);
    }

    public void SetDirection(Vector2 dir)
    {
        _direction = dir.normalized;
    }
}


// Layer mask checks
public static class LayerMaskExtensions
{
    public static bool Contains(this LayerMask mask, int layer)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}