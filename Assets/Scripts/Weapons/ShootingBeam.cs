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
    private AudioSource loopingLaser;
    private List<EnemyHealth> enemiesToDamageThisTick = new List<EnemyHealth>();

    [SerializeField] private List<string> ignoreTags = new List<string> { "Objective", "Player", "Pickup" };


    protected override void Awake()
    {
        base.Awake();
        lineRenderer = GetComponentInChildren<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        GameObject ammoCountObject = GameObject.FindGameObjectWithTag("UI_AmmoCount");
        if (ammoCountObject != null)
        {
            magCapacityText = ammoCountObject.GetComponent<TextMeshProUGUI>();
            if (magCapacityText != null && magCapacityText.transform.childCount > 0)
            {
                inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
        }
        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        AudioPlayer.Current.PlaySfxAtPoint("Equip_ElectricityGun", transform.position);
        canFire = true;
        GameObject ammoCountObject = GameObject.FindGameObjectWithTag("UI_AmmoCount");
        if (ammoCountObject != null)
        {
            magCapacityText = ammoCountObject.GetComponent<TextMeshProUGUI>();
            if (magCapacityText != null && magCapacityText.transform.childCount > 0)
            {
                inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
        }
        UpdateAmmoUI();

        // Reset state in case it was disabled mid-fire
        if (beamActive)
        {
            beamActive = false;
            if (lineRenderer != null) lineRenderer.enabled = false;
        }
        if (AudioPlayer.Current != null && loopingLaser != null)
        {
            AudioPlayer.Current.StopLoopingSfx(loopingLaser);
            loopingLaser = null;
        }
    }

    private void OnDisable()
    {
        if (beamActive)
        {
            beamActive = false;
            if (lineRenderer != null) lineRenderer.enabled = false;
        }
        if (AudioPlayer.Current != null && loopingLaser != null)
        {
            AudioPlayer.Current.StopLoopingSfx(loopingLaser);
            loopingLaser = null;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0) && !beamActive && InventoryAndBuffs.energyAmmo > 0)
        {
            if (AudioPlayer.Current != null)
            {
                AudioPlayer.Current.PlaySfxAtPoint("StartShootingLaser", transform.position);
                if (loopingLaser == null) // Start looping sound only if not already playing
                {
                    loopingLaser = AudioPlayer.Current.PlayLoopingSfx("ShootingLaserLoop", transform.position);
                }
            }
            beamActive = true;
            if (lineRenderer != null) lineRenderer.enabled = true;
        }
        else if ((Input.GetMouseButtonUp(0) || InventoryAndBuffs.energyAmmo <= 0) && beamActive)
        {
            beamActive = false;
            if (lineRenderer != null) lineRenderer.enabled = false;
            if (AudioPlayer.Current != null && loopingLaser != null)
            {
                AudioPlayer.Current.StopLoopingSfx(loopingLaser);
                loopingLaser = null;
            }
        }
        else if (Input.GetMouseButtonDown(0) && InventoryAndBuffs.energyAmmo <= 0) // Clicked with no ammo
        {
            if (AudioPlayer.Current != null)
            {
                AudioPlayer.Current.PlaySfxAtPoint("Empty_Chamber", transform.position);
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
                    beamActive = false;
                    if (lineRenderer != null) lineRenderer.enabled = false;
                    if (AudioPlayer.Current != null && loopingLaser != null)
                    {
                        AudioPlayer.Current.StopLoopingSfx(loopingLaser);
                        loopingLaser = null;
                    }
                }
            }
        }
    }

    private void ProcessBeamHits(out float beamEndPointDistance, out List<EnemyHealth> enemiesFound)
    {
        enemiesToDamageThisTick.Clear();
        enemiesFound = enemiesToDamageThisTick;

        float firstObstructionDistance = maxBeamLength;
        if (projectileSpawnLocation == null)
        {
            beamEndPointDistance = 0f;
            return;
        }
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
        if (lineRenderer == null || projectileSpawnLocation == null) return;
        lineRenderer.SetPosition(0, projectileSpawnLocation.position);
        lineRenderer.SetPosition(1, projectileSpawnLocation.position + (Vector3)transform.right * currentLength);
    }

    private void UpdateAmmoUI()
    {
        if (inventoryAmmoText != null) inventoryAmmoText.SetText("");
        if (magCapacityText != null) magCapacityText.SetText(InventoryAndBuffs.energyAmmo.ToString());
    }

    protected override void Fire() { /* Not used in beam */ }
}