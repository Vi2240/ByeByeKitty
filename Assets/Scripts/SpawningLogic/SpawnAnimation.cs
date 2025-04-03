using System.Collections;
using UnityEngine;

public class SpawnAnimation : MonoBehaviour
{
    [SerializeField] float spawnDelay = 1.5f;
    [SerializeField] GameObject spawnSprite;
    [SerializeField] GameObject enemySprite;
    [SerializeField] Collider2D[] colliders;

    float savedSpeed;
    EnemyMovement movementScript;
    EnemyAttack attackScript;
    EnemyHealth healthScript;

    void Awake()
    {
        movementScript = GetComponent<EnemyMovement>();
        healthScript = GetComponent<EnemyHealth>();

        // Try catch needed because not all enemies have an attack script. 
        try
        {
            attackScript = GetComponent<EnemyAttack>();
        }
        catch
        {
            print("No attack script found");
        }

        SpawnLogic();
    }

    void SpawnLogic()
    {
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        spawnSprite.SetActive(true);
        enemySprite.SetActive(false);

        movementScript.disableMovement = true;

        if (attackScript != null)
        {
            attackScript.disableAttacking = true;
        }
        StartCoroutine(healthScript.TemporaryImmunity(spawnDelay));

        StartCoroutine(countdown(spawnDelay));
    }

    IEnumerator countdown(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(spawnSprite);
        enemySprite.SetActive(true);
        movementScript.disableMovement = false;

        if (attackScript != null)
        {
            attackScript.disableAttacking = false;
        }

        foreach (Collider2D collider in colliders)
        {
            collider.enabled = true;
        }
    }
}
