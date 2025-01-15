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
    [SerializeField] AudioSource gunshotFX;
    [SerializeField] AudioSource reloadFX;

    [SerializeField, Tooltip("Wether the player can or cannot hold down left click to continue firing.")] 
    bool automaticShooting = false;

    [SerializeField] float fireCooldown;
    [SerializeField] float bulletForce = 25f;
    [SerializeField] float reloadTime = 1f;
    [SerializeField] float bulletDamage = 5f;

    [SerializeField] int magazineSize;

    Vector3 mousePos;
    Camera mainCam;
    float defaultYscale;
    TextMeshProUGUI magCapacityText;
    CircleCollider2D shootRange;
    //private PlayerMovement movementScript;

    int magazine;
    float timer;
    float requiredMouseDistanceFromPlayer = 1.5f;

    bool canFire = true;
    bool canReload = true;
    bool isRealoding = false;

    void Start()
    {
        defaultYscale = weaponSpriteTransform.localScale.y;
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        magazine = magazineSize;
        //shootRange = GameObject.FindGameObjectWithTag("ShootRange").GetComponent<CircleCollider2D>();
        //weaponRotation = GetComponentInChildren<SpriteRenderer>();
        //movementScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        magCapacityText.SetText(magazine + " / " + Inventory.ammo);
    }

    // Switch the text in UI to match this weapon when it is enabled in different scripts
    private void OnEnable()
    {
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        magCapacityText.SetText(magazine + " / " + Inventory.ammo);
    }

    void Update()
    {
        WeaponRotation();

        if (automaticShooting)
        {
            if (Input.GetMouseButton(0) && canFire && magazine > 0 && MouseFarEnoughFromPlayer() == true && !isRealoding)
            {
                Fire();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && canFire && magazine > 0 && MouseFarEnoughFromPlayer() == true && !isRealoding)
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
        magazine -= 1;
        //gunshot.Play();
        print("Shot");
        magCapacityText.SetText(magazine + " / " + Inventory.ammo);
    }

    void Reload()
    {
        if (Inventory.ammo > 0)
        {
            if (magazine < magazineSize && canReload)
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
        int bulletsToTake = magazineSize - magazine;

        Inventory.ammo -= bulletsToTake;
        if (Inventory.ammo < 0)
        {
            bulletsToRemoveFromMag = Mathf.Abs(Inventory.ammo);
            Inventory.ammo = 0;
        }

        magazine = magazineSize - bulletsToRemoveFromMag;

        magCapacityText.SetText(magazine + " / " + Inventory.ammo);

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