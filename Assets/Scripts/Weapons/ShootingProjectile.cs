using System.Collections;
using UnityEngine;
using TMPro;

public class ShootingProjectile : WeaponBase
{
    TextMeshProUGUI magCapacityText;
    TextMeshProUGUI inventoryAmmoText;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField, Tooltip("Wether the player can or cannot hold down left click to continue firing.")]
    bool automaticShooting = false;
    [SerializeField] float bulletForce = 25f;
    [SerializeField] bool isMachineGun, isSniper, isPistol;
    Coroutine saveReloadCoroutine;
    bool canPlayEmptySFX = true;
    private Coroutine playEmptySFXCoroutine;

    private void Awake()
    {
        base.Awake();
        currentMagAmmoCount = magazineSizeMax;
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        UpdateUI();
    }


    private void OnEnable()
    {
        AudioPlayer.Current.PlaySfxAtPoint("Pickup_Weapon", transform.position);
        if (magCapacityText == null || inventoryAmmoText == null) { return; }
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        UpdateUI();
        StopCoroutine(ReloadDelay());
        isReloading = false;
        canReload = true;
        canFire = true;

        canPlayEmptySFX = true;
        if (playEmptySFXCoroutine != null)
        {
            StopCoroutine(playEmptySFXCoroutine);
            playEmptySFXCoroutine = null;
        }
    }


    protected override void Update()
    {
        base.Update();

        if (automaticShooting)
        {
            if (Input.GetMouseButton(0))
            {
                if (currentMagAmmoCount > 0)
                {
                    if (canFire)
                    {
                        Fire();
                    }
                }
                else
                {
                    if (canPlayEmptySFX && !isReloading)
                    {
                        AudioPlayer.Current.PlaySfxAtPoint("Empty_Chamber", transform.position);
                        canPlayEmptySFX = false;
                        if (playEmptySFXCoroutine != null)
                        {
                            StopCoroutine(playEmptySFXCoroutine);
                        }
                        playEmptySFXCoroutine = StartCoroutine(EmptyClickSFXCooldown());
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && canFire)
            {
                if (currentMagAmmoCount > 0) { Fire(); }
                else if (!isReloading) { AudioPlayer.Current.PlaySfxAtPoint("Empty_Chamber", transform.position); }
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) { Reload(); }
    }

    private IEnumerator EmptyClickSFXCooldown()
    {
        yield return new WaitForSeconds(fireCooldown);
        canPlayEmptySFX = true;
        playEmptySFXCoroutine = null;
    }

    protected override void Fire()
    {
        canFire = false;
        StartCoroutine(ShootCooldown());
        GameObject bullet_ = Instantiate(bulletPrefab, projectileSpawnLocation.position, gameObject.transform.rotation * new Quaternion(0f, 0f, 90, -90));
        bullet_.GetComponent<Rigidbody2D>().AddForce(bullet_.transform.up * bulletForce, ForceMode2D.Impulse);

        float finalDamage = Mathf.Round(damagePerHit * InventoryAndBuffs.playerDamageMultiplier);
        if (isMachineGun)
        {
            bullet_.GetComponent<MachineGunBullet>().SetDirection(transform.right);
            bullet_.GetComponent<MachineGunBullet>().SetDamage(finalDamage);
            AudioPlayer.Current.PlaySfxAtPoint("Shoot_MachineGun", transform.position);
        }
        else if (isSniper)
        {
            bullet_.GetComponent<SniperBullet>().SetDirection(transform.right);
            bullet_.GetComponent<SniperBullet>().SetDamage(finalDamage);
            AudioPlayer.Current.PlaySfxAtPoint("Shoot_Sniper", transform.position);
        }
        else if (isPistol)
        {
            bullet_.GetComponent<MachineGunBullet>().SetDirection(transform.right);
            bullet_.GetComponent<MachineGunBullet>().SetDamage(finalDamage);
            AudioPlayer.Current.PlaySfxAtPoint("Shoot_Pistol", transform.position);
        }

        --currentMagAmmoCount;
        UpdateUI();
    }

    private void Reload()
    {
        if (currentMagAmmoCount < magazineSizeMax && !isReloading)
        {
            StartCoroutine(ReloadDelay());
        }
    }

    private IEnumerator ReloadDelay()
    {
        isReloading = true;
        playerMovement.EnableNerfedMovement();
        AudioPlayer.Current.PlaySfxAtPoint("Eject_Magazine", transform.position);
        StartCoroutine(InsertMagazine());

        yield return new WaitForSeconds(reloadTime);
        if (isPistol)
        {
            currentMagAmmoCount = magazineSizeMax;
        }
        else
        {
            int bulletsToReload = Mathf.Min(magazineSizeMax - currentMagAmmoCount, InventoryAndBuffs.ammo);
            currentMagAmmoCount += bulletsToReload;
            InventoryAndBuffs.ammo -= bulletsToReload;
        }

        UpdateUI();
        isReloading = false;
        playerMovement.DisableNerfedMovement();

        if (isMachineGun) AudioPlayer.Current.PlaySfxAtPoint("Cock_MachineGun", transform.position);
        if (isSniper) AudioPlayer.Current.PlaySfxAtPoint("Cock_MachineGun", transform.position);
        if (isPistol) AudioPlayer.Current.PlaySfxAtPoint("Cock_Pistol", transform.position);
    }

    private IEnumerator InsertMagazine()
    {
        yield return new WaitForSeconds(reloadTime * 0.6f);
        AudioPlayer.Current.PlaySfxAtPoint("Insert_Magazine", transform.position);
    }

    private void UpdateUI()
    {
        if (isPistol)
        {
            magCapacityText.SetText(currentMagAmmoCount + " / inf");
            inventoryAmmoText.SetText(" ");
        }
        else
        {
            magCapacityText.SetText(currentMagAmmoCount + " / " + magazineSizeMax);
            inventoryAmmoText.SetText(InventoryAndBuffs.ammo.ToString());
        }
    }
}