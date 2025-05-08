using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] float timeToEnableCollider = 1f;
    [SerializeField, Tooltip("Set to zero to disable.")] float despawnTimer = 0f;

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (!collider) { return; }
        collider.enabled = false;
        StartCoroutine(Timer(collider));
    }

    IEnumerator Timer(Collider2D collider)
    {
        yield return new WaitForSeconds(timeToEnableCollider);
        collider.enabled = true;
        if (despawnTimer > 0) { Destroy(gameObject, despawnTimer); }
    }
}