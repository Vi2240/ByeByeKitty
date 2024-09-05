using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float hitEffectTime;

    [SerializeField] GameObject hitEffectPrefab;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(hitEffect, hitEffectTime);
        Destroy(gameObject);
    }
}
