using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float lifeTime = 1;
    [SerializeField] float hitEffectTime = 1;
    [SerializeField] GameObject hitEffectPrefab;

    // Changing this value here does nothing.
    [Tooltip("Accessed by weapons when instantiating bullets.")]
    public float damage = 0;

    void Start()
    {
        Destroy(gameObject, lifeTime); // Despawn timer to reduce lag when bullets don't despawn through collisions.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        if (other.gameObject.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);
            //print("Dealt " + damage + " damage.");
        }
        Destroy(hitEffect, hitEffectTime);
        Destroy(gameObject);
    } 
}