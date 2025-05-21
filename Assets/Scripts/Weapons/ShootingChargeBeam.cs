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
    private string chargeUpSoundName = "BeamCharge";
    private bool isFiring = false;
    private float currentChargeTime = 0f;
    private Coroutine beamCoroutine;
    private Coroutine damageTickCoroutine;
    private HashSet<EnemyHealth> enemiesDamagedThisTick = new HashSet<EnemyHealth>();

    private AudioSource chargeUpSoundSource; // To store the charging sound AudioSource

    protected override void Awake()
    {
        base.Awake();

        lineRenderer = GetComponentInChildren<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("No line renderer found on " + gameObject.name, this);
            this.enabled = false;
            return;
        }
        lineRenderer.enabled = false;

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
            chargeVisualSprite.transform.localScale = initialChargeScale;
        }
        else
        {
            Debug.LogWarning("No charge visual sprite assigned to " + gameObject.name, this);
        }

        FindUIElements();
        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        AudioPlayer.Current.PlaySfxAtPoint("Equip_Beam", transform.position);
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
        base.Update();

        HandleInputs();
        UpdateChargeVisual();

        if (isFiring)
        {
            DrawBeam(maxBeamLength);
        }
    }

    private void FindUIElements()
    {
        if (magCapacityText == null)
        {
            GameObject ammoCountObject = GameObject.FindGameObjectWithTag(AmmoCountTag);
            if (ammoCountObject == null)
            {
                // Debug.LogWarning("UI_AmmoCount object not found in scene.");
                return;
            }

            magCapacityText = ammoCountObject.GetComponent<TextMeshProUGUI>();
            if (magCapacityText == null)
            {
                // Debug.LogWarning("TextMeshProUGUI component not found on UI_AmmoCount object.");
                return;
            }

            if (magCapacityText.transform.childCount > 0)
            {
                Transform childTransform = magCapacityText.transform.GetChild(0);
                inventoryAmmoText = childTransform.GetComponent<TextMeshProUGUI>();
                if (inventoryAmmoText == null)
                {
                    // Debug.LogWarning("TextMeshProUGUI component not found on child of UI_AmmoCount object.");
                }
            }
            // else Debug.LogWarning("UI_AmmoCount object has no children for inventory ammo text.");
        }
    }

    private void HandleInputs()
    {
        if (Input.GetMouseButtonDown(0) && !isCharging && !isFiring && canFire && InventoryAndBuffs.energyAmmo >= ammoCostPerShot)
        {
            StartCharging();
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            playerMovement.EnableNerfedMovement();
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Min(currentChargeTime, chargeUpTime);
        }

        if (isCharging && Input.GetMouseButtonUp(0))
        {
            playerMovement.DisableNerfedMovement();
            if (currentChargeTime >= chargeUpTime)
            {
                if (InventoryAndBuffs.energyAmmo >= ammoCostPerShot)
                {
                    FireBeam();
                }
                else
                {
                    CancelCharge();
                    if (AudioPlayer.Current != null) AudioPlayer.Current.PlayUISfx("Empty_Chamber");
                }
            }
            else
            {
                CancelCharge(); // Released too early
            }
        }

        // This handles cancellation if mouse button is released OR if ammo drops below cost while charging
        // Note: GetMouseButtonUp is for the frame it's released. GetMouseButton is for while it's held.
        // So the above GetMouseButtonUp block handles intended firing/cancellation.
        // This one is more of a safety for edge cases or if ammo is depleted by other means during charge.
        if (isCharging && (!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0)) || (isCharging && InventoryAndBuffs.energyAmmo < ammoCostPerShot))
        {
            CancelCharge();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
        canFire = false; // Prevent base class cooldown from resetting too early

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = true;
            chargeVisualSprite.transform.localScale = initialChargeScale;
        }

        // Play Charge Sound
        StopChargeUpSound(); // Stop any previous instance
        if (AudioPlayer.Current != null && !string.IsNullOrEmpty(chargeUpSoundName) && projectileSpawnLocation != null)
        {
            chargeUpSoundSource = AudioPlayer.Current.PlayLoopingSfx(
                chargeUpSoundName,
                projectileSpawnLocation.position, // Sound from weapon tip
                1.0f,      // volumeScale
                null,      // parentTo (null means it parents to AudioPlayer's container)
                false      // shouldLoop = false (play once)
            );
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
        AudioPlayer.Current.PlaySfxAtPoint("BeamSFX", transform.position, 1f);
        isCharging = false;
        isFiring = true;

        StopChargeUpSound(); // Stop charge sound when firing

        InventoryAndBuffs.energyAmmo -= ammoCostPerShot;
        UpdateAmmoUI();

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
        }

        if (beamCoroutine != null) StopCoroutine(beamCoroutine);
        beamCoroutine = StartCoroutine(BeamActiveTimer());

        if (damageTickCoroutine != null) StopCoroutine(damageTickCoroutine);
        damageTickCoroutine = StartCoroutine(DamageTickTimer());

        StartCoroutine(ShootCooldown()); // Start base weapon cooldown AFTER firing

        if (lineRenderer != null) lineRenderer.enabled = true;
        DrawBeam(maxBeamLength);
        ApplyDamage(); // Apply first damage tick immediately
    }

    private void CancelCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
        canFire = true; // Allow firing again (or cooldown to manage it)

        StopChargeUpSound(); // Stop the charge sound

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
        }
    }

    private void StopChargeUpSound()
    {
        if (chargeUpSoundSource != null && AudioPlayer.Current != null)
        {
            AudioPlayer.Current.StopLoopingSfx(chargeUpSoundSource);
            chargeUpSoundSource = null; // Clear the reference
        }
    }

    private IEnumerator BeamActiveTimer()
    {
        yield return new WaitForSeconds(beamDuration);
        StopFiring();
    }

    private IEnumerator DamageTickTimer()
    {
        yield return new WaitForSeconds(damageTickInterval); // Wait for the interval *after* the initial immediate damage
        while (isFiring)
        {
            ApplyDamage();
            yield return new WaitForSeconds(damageTickInterval);
        }
    }

    private void ApplyDamage()
    {
        if (!isFiring || projectileSpawnLocation == null) return;

        enemiesDamagedThisTick.Clear();
        RaycastHit2D[] allHits = Physics2D.RaycastAll(projectileSpawnLocation.position, transform.right, maxBeamLength);

        foreach (RaycastHit2D hit in allHits)
        {
            // Add tag filtering here if needed, similar to your other beam script
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
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

        float finalDamage = Mathf.Round(damagePerHit * InventoryAndBuffs.playerDamageMultiplier);
        enemy.TakeDamage(finalDamage);
        if (damageNumberPrefab != null)
        {
            GameObject dn = Instantiate(damageNumberPrefab, enemy.transform.position, Quaternion.identity);
            var floatingNumber = dn.GetComponent<FloatingHealthNumber>();
            if (floatingNumber != null) floatingNumber.SetText((finalDamage).ToString());
            else Debug.LogWarning("Damage Number Prefab does not have FloatingHealthNumber component.", dn);
        }
    }

    private void DrawBeam(float length)
    {
        if (lineRenderer == null || projectileSpawnLocation == null) return;
        Vector3 endPoint = projectileSpawnLocation.position + (Vector3)transform.right * length;
        lineRenderer.SetPosition(0, projectileSpawnLocation.position);
        lineRenderer.SetPosition(1, endPoint);
    }

    private void StopFiring()
    {
        isFiring = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        if (beamCoroutine != null) StopCoroutine(beamCoroutine);
        if (damageTickCoroutine != null) StopCoroutine(damageTickCoroutine);
        beamCoroutine = null;
        damageTickCoroutine = null;
        // canFire is reset by the ShootCooldown() in WeaponBase or by CancelCharge
    }

    private void ResetState()
    {
        isCharging = false;
        isFiring = false;
        currentChargeTime = 0f;

        StopChargeUpSound(); // Ensure sound is stopped

        if (lineRenderer != null) lineRenderer.enabled = false;

        if (chargeVisualSprite != null)
        {
            chargeVisualSprite.enabled = false;
            chargeVisualSprite.transform.localScale = initialChargeScale;
        }

        // Stop all relevant coroutines
        if (beamCoroutine != null) { StopCoroutine(beamCoroutine); beamCoroutine = null; }
        if (damageTickCoroutine != null) { StopCoroutine(damageTickCoroutine); damageTickCoroutine = null; }
        // If ShootCooldown is a Coroutine variable in WeaponBase and you need to stop it here:
        // if (shootCooldownCoroutine != null) { StopCoroutine(shootCooldownCoroutine); shootCooldownCoroutine = null; }
        // For now, assuming ShootCooldown manages itself or is fine to just run its course.
        // We also call StopCoroutine(ShootCooldown()) in the original script if it was a direct call to StartCoroutine.
        // If WeaponBase.ShootCooldown() returns a coroutine and it's stored, you might need to stop it.
        // For simplicity, let's assume WeaponBase handles its cooldown state correctly or it's not stored.

        canFire = true; // Ready to be used again
    }

    private void UpdateAmmoUI()
    {
        if (magCapacityText != null) // inventoryAmmoText might be null if not found
        {
            if (inventoryAmmoText != null) inventoryAmmoText.SetText("");
            magCapacityText.SetText(InventoryAndBuffs.energyAmmo.ToString());
        }
    }

    protected override void Fire() { /* Not used directly by this charge beam logic */ }
}