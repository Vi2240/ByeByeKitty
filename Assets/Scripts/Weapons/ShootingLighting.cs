using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShootingLightning : WeaponBase
{
    TextMeshProUGUI magCapacityText;
    TextMeshProUGUI inventoryAmmoText;
    [SerializeField] float chainingRange = 5f;
    [SerializeField] int maxChainDepth = 3;
    [SerializeField] int maxTargetsPerChain = 2;
    [SerializeField] GameObject lightningPrefab;        // Prefab for the lightning travel visual
    [SerializeField] Transform lightningSpawnLocation;

    private void Start()
    {
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        inventoryAmmoText.SetText("");
        magCapacityText.SetText(Inventory.energyAmmo.ToString());
        base.Start();
    }

    private void OnEnable()
    {
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        inventoryAmmoText.SetText("");
        magCapacityText.SetText(Inventory.energyAmmo.ToString());
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetMouseButton(0) && canFire)
        {
            Fire();
        }
    }

    protected override void Fire()
    {
        StartCoroutine(ShootCooldown());

        GameObject initialTarget = FindClosestEnemy(transform.position);
        if (initialTarget == null || Inventory.energyAmmo <= 0)
        {
            //Debug.Log("No close enemies or no ammo.");
            return;
        }

        Inventory.energyAmmo--;
        magCapacityText.SetText(Inventory.energyAmmo.ToString());
        ChainLightning(initialTarget, 0, new HashSet<GameObject>(), lightningSpawnLocation.position);      
    }

    private GameObject FindClosestEnemy(Vector3 position)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closestEnemy = null;
        float closestDistance = chainingRange;

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

    private void ChainLightning(GameObject target, int depth, HashSet<GameObject> hitEnemies, Vector3 startPosition)
    {
        if (depth >= maxChainDepth || target == null || hitEnemies.Contains(target)) return;

        hitEnemies.Add(target);
        target.GetComponent<EnemyHealth>().TakeDamage(damagePerHit);
        // Visualize the lightning strike
        Vector3 endPosition = target.transform.position;
        SpawnLightningVisual(startPosition, endPosition);

        foreach (GameObject nextTarget in FindClosestEnemies(target.transform.position, hitEnemies))
        {
            ChainLightning(nextTarget, depth + 1, hitEnemies, target.transform.position);
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
        // create the lightning at the start position
        GameObject lightning = Instantiate(lightningPrefab, start, Quaternion.identity);
        
        // Set the lightning's direction and size (visual)
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        lightning.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        lightning.transform.localScale = new Vector3(0.1f, distance, 1f);
        lightning.transform.position = start + direction / 2;

        // initializes the object and passes the end position, where we'll be spawnsing the end sprite
        lightning.GetComponent<Lighting>().Initialize(end);
    }
}