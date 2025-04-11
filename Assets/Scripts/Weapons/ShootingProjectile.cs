using System.Collections;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ShootingProjectile : WeaponBase
{
    TextMeshProUGUI magCapacityText;
    TextMeshProUGUI inventoryAmmoText;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField, Tooltip("Wether the player can or cannot hold down left click to continue firing.")]
    bool automaticShooting = false;
    [SerializeField] float bulletForce = 25f;
    Coroutine saveReloadCoroutine;
    private void Start()
    {
        base.Start();
        currentMagAmmoCount = magazineSizeMax;
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmoText = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        UpdateUI();
    }


    private void OnEnable()
    {
        if (magCapacityText == null || inventoryAmmoText == null) { return; }
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        UpdateUI();
        StopCoroutine(ReloadDelay());
        isReloading = false;
        canReload = true;
        canFire = true;
    }


    protected override void Update()
    {
        base.Update();

        if (automaticShooting)
        {
            if (Input.GetMouseButton(0) && canFire && currentMagAmmoCount > 0) { Fire(); }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && canFire && currentMagAmmoCount > 0) { Fire(); }
        }

        if (Input.GetKeyDown(KeyCode.R)) { Reload(); }
    }

    protected override void Fire()
    {
        canFire = false;
        // Add reload cancel here
        StartCoroutine(ShootCooldown());
        // Original code instantiation that fires away from the player
        GameObject bullet_ = Instantiate(bulletPrefab, projectileSpawnLocation.position, gameObject.transform.rotation * new Quaternion(0f, 0f, 90, -90));
        bullet_.GetComponent<Rigidbody2D>().AddForce(bullet_.transform.up * bulletForce, ForceMode2D.Impulse);
        bullet_.GetComponent<Bullet>().SetDirection(transform.right);
        bullet_.GetComponent<Bullet>().damage = damagePerHit;
        
        currentMagAmmoCount--;
        magCapacityText.SetText(currentMagAmmoCount + " / " + magazineSizeMax);
    }

    private void Reload()
    {
        if (currentMagAmmoCount < magazineSizeMax && Inventory.ammo > 0 && !isReloading)
        {
            StartCoroutine(ReloadDelay());
        }
    }

    private IEnumerator ReloadDelay()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        int bulletsToReload = Mathf.Min(magazineSizeMax - currentMagAmmoCount, Inventory.ammo);
        currentMagAmmoCount += bulletsToReload;
        Inventory.ammo -= bulletsToReload;
        UpdateUI();
        isReloading = false;
    }

    private void UpdateUI()
    {
        magCapacityText.SetText(currentMagAmmoCount + " / " + magazineSizeMax);
        inventoryAmmoText.SetText(Inventory.ammo.ToString());
    }
}