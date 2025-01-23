using System.Collections;
using UnityEngine;

public class SpawnAnimation : MonoBehaviour
{
    [SerializeField] float spawnDelay = 1.5f;
    [SerializeField] GameObject spawnSprite;
    [SerializeField] GameObject enemySprite;

    float savedSpeed;
    EnemyMovement movementScript;
    EnemyAttack attackScript;

    void Awake()
    {
        movementScript = GetComponent<EnemyMovement>();
        try
        {
            attackScript = GetComponent<EnemyAttack>();
        }
        catch
        {
            print("No attack script found");
        }

        spawnSprite.SetActive(true);
        enemySprite.SetActive(false);

        movementScript.disableMovement = true;

        if (attackScript != null)
        {
            attackScript.disableAttacking = true;
        }

        StartCoroutine(countdown(spawnDelay));
    }

    IEnumerator countdown(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(spawnSprite);
        print("Activated");
        enemySprite.SetActive(true);
        movementScript.disableMovement = false;

        if (attackScript != null)
        {
            attackScript.disableAttacking = false;
        }
    }
}
