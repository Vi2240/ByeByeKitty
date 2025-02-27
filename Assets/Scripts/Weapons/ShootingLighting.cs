using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingLighting : MonoBehaviour
{
    [SerializeField] float fireCooldown = 1f;
    [SerializeField] float damagePerHit = 10f;
    [SerializeField] float chainingRange = 5f;
    [SerializeField] int maxChainDepth = 3;
    [SerializeField] int maxTargetsPerChain = 2;
    [SerializeField] GameObject lightningPrefab; // Prefab for the lightning visual

    Camera mainCam;
    float timer;
    bool canFire = true;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canFire)
        {
            Fire();
        }
    }

    void Fire()
    {
        canFire = false;
        StartCoroutine(ShootDelay());

        GameObject initialTarget = FindClosestEnemy(transform.position);
        if (initialTarget != null)
        {
            ChainLightning(initialTarget, 0, new HashSet<GameObject>(), transform.position);
        }
    }

    GameObject FindClosestEnemy(Vector3 position)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    void ChainLightning(GameObject target, int currentDepth, HashSet<GameObject> hitEnemies, Vector3 startPosition)
    {
        if (currentDepth >= maxChainDepth || target == null || hitEnemies.Contains(target))
        {
            return;
        }

        hitEnemies.Add(target);
        target.GetComponent<EnemyHealth>().TakeDamage(damagePerHit);

        // Visualize the lightning strike
        Vector3 endPosition = target.transform.position;
        SpawnLightningVisual(startPosition, endPosition);

        List<GameObject> nextTargets = FindClosestEnemies(target.transform.position, hitEnemies);
        foreach (GameObject nextTarget in nextTargets)
        {
            ChainLightning(nextTarget, currentDepth + 1, hitEnemies, endPosition);
        }
    }

    List<GameObject> FindClosestEnemies(Vector3 position, HashSet<GameObject> hitEnemies)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> closestEnemies = new List<GameObject>();

        foreach (GameObject enemy in enemies)
        {
            if (!hitEnemies.Contains(enemy) && Vector3.Distance(position, enemy.transform.position) <= chainingRange)
            {
                closestEnemies.Add(enemy);
            }
        }

        closestEnemies.Sort((a, b) => Vector3.Distance(position, a.transform.position).CompareTo(Vector3.Distance(position, b.transform.position)));
        return closestEnemies.GetRange(0, Mathf.Min(maxTargetsPerChain, closestEnemies.Count));
    }

    void SpawnLightningVisual(Vector3 start, Vector3 end)
    {
        GameObject lightning = Instantiate(lightningPrefab, start, Quaternion.identity);
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        lightning.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        lightning.transform.localScale = new Vector3(0.1f, distance, 1f);
        lightning.transform.position = start + direction / 2;
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }
}
