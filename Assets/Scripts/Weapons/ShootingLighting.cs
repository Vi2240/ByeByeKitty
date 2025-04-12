using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShootingLightning : WeaponBase
{
    // UI References - Found at runtime
    private TextMeshProUGUI magCapacityText;
    private TextMeshProUGUI inventoryAmmoText;
    private const string AmmoCountTag = "UI_AmmoCount"; // Use const for tags

    [Header("Chain Settings")]
    [SerializeField] private float chainingRange = 5f;
    [SerializeField] private int maxChainDepth = 3;       // Max number of jumps (0 = initial hit only)
    [SerializeField] private int maxTargetsPerChain = 2;  // Max branches from one enemy

    [Header("Visuals")]
    [SerializeField] private GameObject lightningPrefab;        // Prefab for the lightning visual
    [SerializeField] private Transform lightningSpawnLocation; // Assign the barrel tip transform

    protected override void Start()
    {
        base.Start();

        // Ensure lightningSpawnLocation is assigned
        if (lightningSpawnLocation == null)
        {
            Debug.LogError("Lightning Spawn Location is not assigned!", this);
            lightningSpawnLocation = transform;
        }

        FindUIElements();
        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        FindUIElements();
        UpdateAmmoUI();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButton(0) && canFire && InventoryAndBuffs.energyAmmo > 0)
        {
            Fire();
        }
        else if (Input.GetMouseButtonDown(0) && canFire && InventoryAndBuffs.energyAmmo <= 0)
        {
            // Play 'out of ammo' sound/effect
        }
    }

    private void FindUIElements()
    {
        if (magCapacityText != null) return;
    
        GameObject ammoCountGO = GameObject.FindGameObjectWithTag(AmmoCountTag);
        if (ammoCountGO != null)
        {
            magCapacityText = ammoCountGO.GetComponent<TextMeshProUGUI>();
            if (magCapacityText != null)
            {
                // Find child only if parent is found
                if (magCapacityText.transform.childCount > 0)
                {
                    Transform childTransform = magCapacityText.transform.GetChild(0);
                    inventoryAmmoText = childTransform.GetComponent<TextMeshProUGUI>();
                    if (inventoryAmmoText == null) Debug.LogWarning($"ShootingLightning: Found '{AmmoCountTag}' but couldn't find child TextMeshProUGUI.", this);
                }
            }
        }  
    }

    protected override void Fire()
    {
        if (!canFire || InventoryAndBuffs.energyAmmo <= 0) return;

        // Find the initial target*relative to the spawn location
        GameObject initialTarget = FindClosestEnemy(lightningSpawnLocation.position, chainingRange, null);
        if (initialTarget == null)
        {
            StartCoroutine(ShootCooldown());
            return;
        }

        InventoryAndBuffs.energyAmmo--;
        UpdateAmmoUI();
        StartCoroutine(ShootCooldown());

        ChainLightning(initialTarget, 0, new HashSet<GameObject>(), lightningSpawnLocation.position, lightningSpawnLocation);
    }

    private void ChainLightning(GameObject target, int depth, HashSet<GameObject> hitEnemies, Vector3 startPosition, Transform initialAnchor)
    {
        if (depth >= maxChainDepth || target == null || !hitEnemies.Add(target))
        {
            return;
        }

        float finalDamage = Mathf.Round(damagePerHit * InventoryAndBuffs.playerDamageMultiplier);
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(finalDamage);
        }
        else
        {
            Debug.LogWarning($"Target {target.name} is missing EnemyHealth component.", target);
        }

        Vector3 endPosition = target.transform.position;
        SpawnLightningVisual(startPosition, endPosition, (depth == 0) ? initialAnchor : null);

        List<GameObject> nextTargets = FindClosestEnemies(target.transform.position, chainingRange, hitEnemies, maxTargetsPerChain);

        foreach (GameObject nextTarget in nextTargets)
        {
            ChainLightning(nextTarget, depth + 1, hitEnemies, endPosition, null);
        }
    }

    private GameObject FindClosestEnemy(Vector3 position, float range, HashSet<GameObject> excludedEnemies)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // This can cause lag if there are too many enemies. Look at optimizations later
        GameObject closestEnemy = null;
        float closestDistSqr = range * range;

        foreach (GameObject enemy in enemies)
        {
            if (excludedEnemies != null && excludedEnemies.Contains(enemy)) continue;

            Vector3 offset = enemy.transform.position - position;
            float distSqr = offset.sqrMagnitude;

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }

    private List<GameObject> FindClosestEnemies(Vector3 position, float range, HashSet<GameObject> excludedEnemies, int maxCount)
    {
        List<KeyValuePair<float, GameObject>> enemiesInRange = new List<KeyValuePair<float, GameObject>>();
        float rangeSqr = range * range;

        // Maybe try use Physics2D.OverlapCircleAll for optimization
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in allEnemies)
        {
            if (excludedEnemies != null && excludedEnemies.Contains(enemy)) continue;

            Vector3 offset = enemy.transform.position - position;
            float distSqr = offset.sqrMagnitude;

            if (distSqr <= rangeSqr)
            {
                enemiesInRange.Add(new KeyValuePair<float, GameObject>(distSqr, enemy));
            }
        }

        enemiesInRange.Sort((a, b) => a.Key.CompareTo(b.Key));

        List<GameObject> closestEnemies = new List<GameObject>();
        int count = Mathf.Min(maxCount, enemiesInRange.Count);
        for (int i = 0; i < count; i++)
        {
            closestEnemies.Add(enemiesInRange[i].Value);
        }

        return closestEnemies;
    }

    void SpawnLightningVisual(Vector3 start, Vector3 end, Transform startAnchor = null)
    {
        if (lightningPrefab == null) return;

        // Instantiate the visual at the initial start position
        GameObject lightning = Instantiate(lightningPrefab, start, Quaternion.identity);

        // Perform initial setup (visual might be adjusted later by Lighting script if anchored)
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance > 0.01f) // Avoid division by zero or tiny scales
        {
            lightning.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction.normalized);
            lightning.transform.position = start + direction / 2f;
            lightning.transform.localScale = new Vector3(0.1f, distance, 1f);
        }
        else
        {
            // Handle edge case where start and end are virtually the same
            lightning.transform.position = start;
            lightning.transform.localScale = new Vector3(0.1f, 0.1f, 1f); // Minimal size
        }


        // Get the Lighting component, initialize it and pass the anchor.
        Lighting lightingComponent = lightning.GetComponent<Lighting>();
        if (lightingComponent != null)
        {
            lightingComponent.Initialize(end, startAnchor);
        }
        else
        {
            Debug.LogWarning("Lightning prefab is missing the 'Lighting' component.", lightningPrefab);
        }
    }

    private void UpdateAmmoUI()
    {
        if (magCapacityText != null && inventoryAmmoText != null)
        {
            inventoryAmmoText.SetText("");
            magCapacityText.SetText(InventoryAndBuffs.energyAmmo.ToString());
        }
    }
}