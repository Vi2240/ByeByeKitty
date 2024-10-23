using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float lifeTime = 1;
    [SerializeField] float hitEffectTime = 1;
    [SerializeField] GameObject hitEffectPrefab;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(hitEffect, hitEffectTime);
        Destroy(gameObject);
    }   
}
