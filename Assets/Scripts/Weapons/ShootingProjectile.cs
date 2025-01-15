using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ShootingProjectile : MonoBehaviour
{
    // Serialize fields
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
    //float defaultYscale;
    TextMeshProUGUI magCapacityText;
    TextMeshProUGUI inventoryAmmo;
    AudioPlayer audioPlayer;

    //private PlayerMovement movementScript;

    int currentMagazine;
    float requiredMouseDistanceFromPlayer = 1.5f;

    bool canFire = true;
    bool canReload = true;
    bool isRealoding = false;

    void Start()
    {
        //audioPlayer.SfxPlayer("PumpReload_Sound");
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        currentMagazine = maxMagazine;
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        inventoryAmmo = magCapacityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        inventoryAmmo.SetText(Inventory.ammo.ToString());
        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
    }

    private void OnEnable()
    {
        //audioPlayer.SfxPlayer("PumpReload_Sound");
        // Fixed null error that made zero sense. Look more into this later.
        if (magCapacityText == null || inventoryAmmo == null) { return; }


        // Update the UI to match the ammo type and capacity when this object gets enabled again.
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
        inventoryAmmo.SetText(Inventory.ammo.ToString());

        // Code below fixes a bug where you couldn't shoot or reload if you switched weapon during a reload.
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
            if (Input.GetMouseButton(0) && canFire && currentMagazine > 0 && MouseFarEnoughFromPlayer() == true && !isRealoding)
            {
                Fire();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && canFire && currentMagazine > 0 && MouseFarEnoughFromPlayer() == true && !isRealoding)
            {
                Fire();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    void WeaponRotation()
    {
        // Converts mouse position to rotation and applies it to the gameObject.
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        // Checks to tell if the sprite should be flipped when moving the weapon around the player.
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
        GameObject bullet_ = Instantiate(bullet, bulletSpawnLocation.position, gameObject.transform.rotation * new Quaternion(0f, 0f, 90, -90));
        bullet_.GetComponent<Rigidbody2D>().AddForce(bullet_.transform.up * bulletForce, ForceMode2D.Impulse);
        bullet_.GetComponent<Bullet>().damage = bulletDamage;
        currentMagazine -= 1;
        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
    }

    void Reload()
    {
        if (Inventory.ammo > 0)
        {
            if (currentMagazine < maxMagazine && canReload)
            {
                isRealoding = true;
                canReload = false;
                StartCoroutine(ReloadDelay());
            }
        }
    }

    IEnumerator ReloadDelay()
    {
        yield return new WaitForSeconds(reloadTime);
        int bulletsToRemoveFromMag = 0;
        int bulletsToTake = maxMagazine - currentMagazine;

        Inventory.ammo -= bulletsToTake;
        if (Inventory.ammo < 0)
        {
            bulletsToRemoveFromMag = Mathf.Abs(Inventory.ammo);
            Inventory.ammo = 0;
        }

        currentMagazine = maxMagazine - bulletsToRemoveFromMag;

        magCapacityText.SetText(currentMagazine + " / " + maxMagazine);
        inventoryAmmo.SetText(Inventory.ammo.ToString());

        canReload = true;
        isRealoding = false;
        //reload.Play();
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }

    //Checks that the cursor isn't inside the Shoot Range collider
    bool MouseFarEnoughFromPlayer()
    {
        if (Vector2.Distance(gameObject.transform.position, mousePos) > requiredMouseDistanceFromPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}