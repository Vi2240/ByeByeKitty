using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShootingBeam : WeaponBase
{
    TextMeshProUGUI magCapacityText;
    TextMeshProUGUI inventoryAmmoText;
    [SerializeField] GameObject damageNumber;
    [SerializeField] float maxBeamLength = 10f;

    private LineRenderer lineRenderer;
    private bool beamActive = false;

    private List<EnemyHealth> enemiesToDamageThisTick = new List<EnemyHealth>();

    // Tags to ignore for beam collision.
    [SerializeField] private List<string> ignoreTags = new List<string> { "Objective", "Player", "Pickup" };


    protected override void Start()
    {
        base.Start();
        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineRenderer.enabled = false;

        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        inventoryAmmoText.SetText("");
        magCapacityText.SetText(InventoryAndBuffs.energyAmmo.ToString());

        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        UpdateAmmoUI();
    }

    private void OnDisable()
    {
        if (beamActive)
        {
            beamActive = false;
            lineRenderer.enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0) && InventoryAndBuffs.energyAmmo > 0)
        {
            beamActive = true;
            lineRenderer.enabled = true;
        }
        else if (Input.GetMouseButtonUp(0) || InventoryAndBuffs.energyAmmo <= 0)
        {
            if (Input.GetMouseButtonDown(0) && InventoryAndBuffs.energyAmmo <= 0) { /* Add out of ammo sound */ }

            if (beamActive)
            {
                beamActive = false;
                lineRenderer.enabled = false;
            }
        }

        if (beamActive)
        {
            ProcessBeamHits(out float effectiveBeamEndDistance, out enemiesToDamageThisTick);
            DrawBeam(effectiveBeamEndDistance);

            if (canFire)
            {
                if (InventoryAndBuffs.energyAmmo > 0)
                {
                    InventoryAndBuffs.energyAmmo--;
                    UpdateAmmoUI();
                    StartCoroutine(ShootCooldown());

                    foreach (EnemyHealth enemy in enemiesToDamageThisTick)
                    {
                        if (enemy != null)
                        {
                            HitEnemy(enemy);
                        }
                    }
                }
                else
                {
                    // Turns off beam if out of ammo.
                    beamActive = false;
                    lineRenderer.enabled = false;
                }
            }
        }
    }

    // Finds the first hit that should stop the beam while ignoring tags in the ignoreTags list.
    private void ProcessBeamHits(out float beamEndPointDistance, out List<EnemyHealth> enemiesFound)
    {
        // Clear the list from the previous frame.
        enemiesToDamageThisTick.Clear();
        enemiesFound = enemiesToDamageThisTick;

        float firstObstructionDistance = maxBeamLength;
        RaycastHit2D[] allHits = Physics2D.RaycastAll(projectileSpawnLocation.position, transform.right, maxBeamLength);

        foreach (RaycastHit2D hit in allHits)
        {
            bool shouldIgnore = false;
            foreach (string tag in ignoreTags)
            {
                if (hit.collider.CompareTag(tag))
                {
                    shouldIgnore = true;
                    break;
                }
            }

            if (!shouldIgnore)
            {
                firstObstructionDistance = Mathf.Min(firstObstructionDistance, hit.distance);
            }
        }

        // This is the final distance the beam should visually travel to.
        beamEndPointDistance = firstObstructionDistance;

        foreach (RaycastHit2D hit in allHits)
        {
            if (hit.collider.CompareTag("Enemy") && hit.distance <= beamEndPointDistance)
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null && !enemiesFound.Contains(enemyHealth))
                {
                    enemiesFound.Add(enemyHealth);
                }
            }
        }
    }

    private void HitEnemy(EnemyHealth enemy)
    {
        float finalDamage = Mathf.Round(damagePerHit * InventoryAndBuffs.playerDamageMultiplier);
        enemy.TakeDamage(finalDamage);
        if (damageNumber != null)
        {
            Instantiate(damageNumber, enemy.transform.position, Quaternion.identity).GetComponent<FloatingHealthNumber>().SetText(finalDamage.ToString());
        }
    }

    private void DrawBeam(float currentLength)
    {
        lineRenderer.SetPosition(0, projectileSpawnLocation.position);
        lineRenderer.SetPosition(1, projectileSpawnLocation.position + (Vector3)transform.right * currentLength);
    }

    private void UpdateAmmoUI()
    {
        inventoryAmmoText.SetText("");
        magCapacityText.SetText(InventoryAndBuffs.energyAmmo.ToString());
    }

    protected override void Fire() { /* Not used in beam */ }
}