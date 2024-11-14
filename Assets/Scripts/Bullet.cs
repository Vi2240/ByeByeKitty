using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float damage = 5;
    [SerializeField] float lifeTime = 1;
    [SerializeField] float hitEffectTime = 1;
    [SerializeField] GameObject hitEffectPrefab;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        if (other.gameObject.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);
        }
        Destroy(hitEffect, hitEffectTime);
        Destroy(gameObject);
    } 
}
