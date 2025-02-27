using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShootingProjectile : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] Transform weaponSpriteTransform;
    [SerializeField, Tooltip("Wether the player can or cannot hold down left click to continue firing.")]
    bool automaticShooting = false;
    [SerializeField] float fireCooldown;
    [SerializeField] float bulletForce = 25f;
    [SerializeField] float reloadTime = 1f;
    [SerializeField] float bulletDamage = 5f;
    [SerializeField] int maxMagazine;

    Vector3 mousePos;
    Camera mainCam;
    TextMeshProUGUI magCapacityText;
    TextMeshProUGUI inventoryAmmo;

    int currentMagazine;
    float requiredMouseDistanceFromPlayer = 1.5f;
    bool canFire = true, canReload = true, isRealoding = false;

    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        currentMagazine = maxMagazine;
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmo = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        inventoryAmmo.SetText(Inventory.ammo.ToString());
        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
    }

    private void OnEnable()
    {
        if (magCapacityText == null || inventoryAmmo == null) { return; }
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
        inventoryAmmo.SetText(Inventory.ammo.ToString());
        StopCoroutine(ReloadDelay());
        isRealoding = false;
        canReload = true;
        canFire = true;
    }

    void Update()
    {
        WeaponRotation();

        if (automaticShooting)
        {
            if (Input.GetMouseButton(0) && canFire && currentMagazine > 0 && MouseFarEnoughFromPlayer() && !isRealoding) { Fire(); }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && canFire && currentMagazine > 0 && MouseFarEnoughFromPlayer() && !isRealoding) { Fire(); }
        }

        if (Input.GetKeyDown(KeyCode.R)) { Reload(); }
    }

    void WeaponRotation()
    {
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        Vector3 saveScale = weaponSpriteTransform.localScale;
        if (rotZ > 90 || rotZ < -90)
        {
            if (saveScale.y > 0)
            {
                saveScale.y *= -1;
                weaponSpriteTransform.localScale = saveScale;
            }
        }
        else
        {
            if (saveScale.y < 0)
            {
                saveScale.y *= -1;
                weaponSpriteTransform.localScale = saveScale;
            }
        }
    }

    void Fire()
    {
        canFire = false;
        StartCoroutine(ShootDelay());
        // Original code instantiation that fires away from the player
        GameObject bullet_ = Instantiate(bullet, bulletSpawnLocation.position, gameObject.transform.rotation * new Quaternion(0f, 0f, 90, -90));
        bullet_.GetComponent<Rigidbody2D>().AddForce(bullet_.transform.up * bulletForce, ForceMode2D.Impulse);
        bullet_.GetComponent<Bullet>().SetDirection(transform.right);
        bullet_.GetComponent<Bullet>().damage = bulletDamage;
        currentMagazine--;
        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
    }

    void Reload()
    {
        if (Inventory.ammo > 0 && currentMagazine < maxMagazine && canReload)
        {
            isRealoding = true;
            canReload = false;
            StartCoroutine(ReloadDelay());
        }
    }

    IEnumerator ReloadDelay()
    {
        yield return new WaitForSeconds(reloadTime);
        int bulletsToTake = maxMagazine - currentMagazine;
        Inventory.ammo -= bulletsToTake;
        if (Inventory.ammo < 0)
        {
            int extra = Mathf.Abs(Inventory.ammo);
            Inventory.ammo = 0;
            currentMagazine = maxMagazine - extra;
        }
        else currentMagazine = maxMagazine;
        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
        inventoryAmmo.SetText(Inventory.ammo.ToString());
        canReload = true;
        isRealoding = false;
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }

    bool MouseFarEnoughFromPlayer()
    {
        return Vector2.Distance(transform.position, mousePos) > requiredMouseDistanceFromPlayer;
    }
}