using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShootingChargeBeam : WeaponBase
{
    // UI References - Found at runtime
    private TextMeshProUGUI magCapacityText;
    private TextMeshProUGUI inventoryAmmoText;
    private const string AmmoCountTag = "UI_AmmoCount";

    [Header("Beam Settings")]
    [SerializeField] private float maxBeamLength = 20f;
    [SerializeField] private float beamDuration = 0.5f;
    [SerializeField] private float damageTickInterval = 0.1f;
    [SerializeField] private int ammoCostPerShot = 5;

    [Header("Charge Settings")]
    [SerializeField] private float chargeUpTime = 1.5f;
    [SerializeField] private SpriteRenderer chargeVisualSprite;
    [SerializeField] private Vector3 initialChargeScale = Vector3.zero;
    [SerializeField] private float maxChargeScale = 4f;

    [Header("Damage")]
    [SerializeField] private GameObject damageNumberPrefab;

    private LineRenderer lineRenderer;
    private bool isCharging = false;
    private bool isFiring = false; // Tracks if the beam is currently active
    private float currentChargeTime = 0f;
    private Coroutine beamCoroutine;
    private Coroutine damageTickCoroutine;
    private HashSet<EnemyHealth> enemiesDamagedThisTick = new HashSet<EnemyHealth>();

    protected override void Start()
    {
        base.Start();

        lineRenderer = GetComponentInChildren<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("No line render found.", this);
            this.enabled = false;
            return;
        }
        lineRenderer.enabled = false; // Start disabled

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
            chargeVisualSprite.transform.localScale = initialChargeScale;
        }
        else
        {
            Debug.LogWarning("No charge ball assigned.", this);
        }

        FindUIElements();
        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        FindUIElements();
        UpdateAmmoUI();
        ResetState();
    }

    private void OnDisable()
    {
        ResetState();
    }

    protected override void Update()
    {
        base.Update(); // Handles cooldown timer

        HandleInputs();
        UpdateChargeVisual();

        // *** ADDED: Continuously redraw the beam while firing ***
        if (isFiring)
        {
            // Ensure the beam visually follows the weapon's orientation
            DrawBeam(maxBeamLength);
            // Note: Damage application is still handled by the DamageTickTimer coroutine
        }
    }

    private void FindUIElements()
    {
        // Reduced redundancy slightly
        if (magCapacityText == null)
        {
            GameObject ammoCountObject = GameObject.FindGameObjectWithTag(AmmoCountTag);
            if (ammoCountObject == null)
            {
                return;
            }

            magCapacityText = ammoCountObject.GetComponent<TextMeshProUGUI>();
            if (magCapacityText == null)
            {
                return;
            }

            // Find child only if parent is found and has the component
            if (magCapacityText.transform.childCount > 0) // Safer check than GetChild(0) directly
            {
                Transform childTransform = magCapacityText.transform.GetChild(0);
                inventoryAmmoText = childTransform.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void HandleInputs()
    {
        // Start Charging
        if (Input.GetMouseButtonDown(0) && !isCharging && !isFiring && canFire && Inventory.energyAmmo >= ammoCostPerShot)
        {
            StartCharging();
        }

        // Hold Charge
        if (isCharging && Input.GetMouseButton(0))
        {
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Min(currentChargeTime, chargeUpTime); // Clamp time
        }

        // Release Charge
        if (isCharging && Input.GetMouseButtonUp(0))
        {
            if (currentChargeTime >= chargeUpTime)
            {
                if (Inventory.energyAmmo >= ammoCostPerShot)
                {
                    FireBeam();
                }
                else
                {
                    CancelCharge();
                }
            }
            else
            {
                CancelCharge();
            }
        }

        // Cancel Charge (mouse release or out of ammo)
        if (isCharging && (!Input.GetMouseButton(0) || Inventory.energyAmmo < ammoCostPerShot))
        {
            CancelCharge();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
        canFire = false;

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = true;
            chargeVisualSprite.transform.localScale = initialChargeScale;
        }
    }

    private void UpdateChargeVisual()
    {
        if (isCharging && chargeVisualSprite != null)
        {
            float chargeRatio = Mathf.Clamp01(currentChargeTime / chargeUpTime);
            chargeVisualSprite.transform.localScale = Vector3.Lerp(initialChargeScale, Vector3.one * maxChargeScale, chargeRatio);
        }
    }

    private void FireBeam()
    {
        isCharging = false;
        isFiring = true; // Set firing state to true

        Inventory.energyAmmo -= ammoCostPerShot;
        UpdateAmmoUI();

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
        }

        // Start timers
        if (beamCoroutine != null) StopCoroutine(beamCoroutine);
        beamCoroutine = StartCoroutine(BeamActiveTimer()); // This timer will eventually set isFiring = false

        if (damageTickCoroutine != null) StopCoroutine(damageTickCoroutine);
        damageTickCoroutine = StartCoroutine(DamageTickTimer()); // This timer applies damage periodically

        StartCoroutine(ShootCooldown()); // Start weapon cooldown

        // *** Initial setup of the beam ***
        lineRenderer.enabled = true; // Enable the renderer
        DrawBeam(maxBeamLength); // Draw the first frame immediately
        ApplyDamage(); // Apply first damage tick immediately
    }

    private void CancelCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
        canFire = true;

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
        }
    }

    // This coroutine now ONLY controls the duration and stops the firing state
    private IEnumerator BeamActiveTimer()
    {
        yield return new WaitForSeconds(beamDuration);
        StopFiring(); // Call the method to clean up the firing state
    }

    // This coroutine handles periodic damage (no change needed here)
    private IEnumerator DamageTickTimer()
    {
        // Wait for the interval *after* the initial immediate damage
        yield return new WaitForSeconds(damageTickInterval);

        while (isFiring) // Continue ticking as long as the beam is active
        {
            ApplyDamage();
            yield return new WaitForSeconds(damageTickInterval);
        }
    }


    private void ApplyDamage()
    {
        // Raycast needs to happen each tick to find enemies currently in the beam's path
        if (!isFiring || projectileSpawnLocation == null) return;

        enemiesDamagedThisTick.Clear();
        // *** Raycast direction MUST match the DrawBeam direction ***
        RaycastHit2D[] allHits = Physics2D.RaycastAll(projectileSpawnLocation.position, transform.right, maxBeamLength);

        foreach (RaycastHit2D hit in allHits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                // Add returns true if the element was added (wasn't already present)
                if (enemyHealth != null && enemiesDamagedThisTick.Add(enemyHealth))
                {
                    HitEnemy(enemyHealth);
                }
            }
        }
    }

    private void HitEnemy(EnemyHealth enemy)
    {
        if (enemy == null) return;
        enemy.TakeDamage(damagePerHit); // Use base class damage value
        if (damageNumberPrefab != null)
        {
            GameObject dn = Instantiate(damageNumberPrefab, enemy.transform.position, Quaternion.identity);
            var floatingNumber = dn.GetComponent<FloatingHealthNumber>();
            if (floatingNumber != null) floatingNumber.SetText(damagePerHit.ToString());
            else Debug.LogWarning("Damage Number Prefab does not have FloatingHealthNumber component.", dn);
        }
    }

    // Draws the beam based on current weapon orientation
    private void DrawBeam(float length)
    {
        if (lineRenderer == null || projectileSpawnLocation == null) return;
        // Use transform.right to get the current forward direction of the weapon
        Vector3 endPoint = projectileSpawnLocation.position + (Vector3)transform.right * length;
        lineRenderer.SetPosition(0, projectileSpawnLocation.position);
        lineRenderer.SetPosition(1, endPoint);
    }

    // Stops the firing state and cleans up visuals/coroutines
    private void StopFiring()
    {
        isFiring = false; // Critical: Stops the drawing in Update and the damage ticks
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false; // Disable the renderer
        }

        // Ensure coroutines are stopped if they somehow haven't finished
        if (beamCoroutine != null) StopCoroutine(beamCoroutine);
        if (damageTickCoroutine != null) StopCoroutine(damageTickCoroutine);
        beamCoroutine = null;
        damageTickCoroutine = null;
        // canFire is handled by ShootCooldown
    }

    // Reset state for disable/enable cycles
    private void ResetState()
    {
        isCharging = false;
        isFiring = false; // Ensure firing stops on reset
        currentChargeTime = 0f;

        if (lineRenderer != null) lineRenderer.enabled = false;

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
            chargeVisualSprite.transform.localScale = initialChargeScale;
        }

        // Stop all potentially running coroutines related to firing/charging
        if (beamCoroutine != null) StopCoroutine(beamCoroutine);
        if (damageTickCoroutine != null) StopCoroutine(damageTickCoroutine);
        StopCoroutine(ShootCooldown()); // Also stop base cooldown if running

        beamCoroutine = null;
        damageTickCoroutine = null;
        canFire = true; // Ready to fire again
    }

    private void UpdateAmmoUI()
    {
        if (magCapacityText != null && inventoryAmmoText != null)
        {
            inventoryAmmoText.SetText("");
            magCapacityText.SetText(Inventory.energyAmmo.ToString());
        }
    }

    protected override void Fire() { /* Not used directly */ }
}