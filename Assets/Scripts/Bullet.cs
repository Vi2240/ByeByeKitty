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

    private void OnCollisionEnter2D(Collision2D other)
    {
        GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        if (other.gameObject.tag == "Enemy")
        {
            Destroy(other.gameObject);
        }
        Destroy(hitEffect, hitEffectTime);
        Destroy(gameObject);
    }   
}
