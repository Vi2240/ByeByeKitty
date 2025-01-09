using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using TMPro;
using UnityEngine;

public class ShootingBeam : MonoBehaviour
{
    // Serialize fields
    [SerializeField] GameObject laser;
    [SerializeField] Transform beamSpawnLocation;
    [SerializeField] Transform weaponSpriteTransform;
    [SerializeField] AudioSource gunshotFX;
    [SerializeField] AudioSource reloadFX;

    [SerializeField, Tooltip("Wether the player can or cannot hold down left click to continue firing.")]
    bool automaticShooting = false;

    [SerializeField] float fireCooldown;
    [SerializeField] int splitAmount = 0;
    [SerializeField] float reloadTime = 1f;
    [SerializeField] float bulletDamage = 5f;
    [SerializeField] float beamLength = 5f;

    [SerializeField] int magazineSize;

    UnityEngine.Vector3 mousePos;
    Camera mainCam;
    LineRenderer lineRenderer;
    TextMeshProUGUI magCapacityText;
    CircleCollider2D shootRange;
    RaycastHit2D hit;
    //private PlayerMovement movementScript;

    int magazine;
    float timer;
    float requiredMouseDistanceFromPlayer = 1.5f;
    float savedBeamLength;

    bool canFire = true;
    bool canReload = true;
    bool isRealoding = false;
    bool beamActive = false;

    void Start()
    {
        savedBeamLength = beamLength;
        lineRenderer = GetComponentInChildren<LineRenderer>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        magazine = magazineSize;
        //shootRange = GameObject.FindGameObjectWithTag("ShootRange").GetComponent<CircleCollider2D>();
        //weaponRotation = GetComponentInChildren<SpriteRenderer>();
        //movementScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        magCapacityText = GameObject.FindGameObjectWithTag("UI_AmmoCount").GetComponent<TextMeshProUGUI>();
        magCapacityText.SetText(magazine + " / " + Inventory.ammo);
    }

    void Update()
    {
        WeaponRotation();

        if (beamActive) 
        {
            BeamLogic();
            DrawBeam();
        }

        if (Input.GetMouseButtonDown(0) && magazine > 0 && MouseFarEnoughFromPlayer() == true && !isRealoding)
        {
            lineRenderer.enabled = true;
            beamActive = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lineRenderer.enabled = false;
            beamActive = false;
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
        UnityEngine.Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = UnityEngine.Quaternion.Euler(0, 0, rotZ);

        // Checks to tell if the sprite should be flipped when moving the weapon around the player.
        UnityEngine.Vector3 saveScale = weaponSpriteTransform.localScale;
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

    void BeamLogic()
    {
        hit = Physics2D.Raycast(beamSpawnLocation.position, transform.right, beamLength);

        if (hit.collider == null)
        {
            beamLength = savedBeamLength;
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            beamLength = UnityEngine.Vector2.Distance(beamSpawnLocation.position, hit.transform.position);

            if (canFire)
            {
                Fire();
            }
        }
    }

    void Fire()
    {
        canFire = false;
        StartCoroutine(ShootDelay());

        print("Hit enemy");

        // Remove later, debugging
        Debug.DrawRay(beamSpawnLocation.position, transform.right*beamLength, UnityEngine.Color.red, 5f);

        SplitTotargets(splitAmount);
    }

    void SplitTotargets(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // Find all GameObjects with the tag "Enemy" and sort the list based on distance to get the splitAmount closest enemies.
        List<GameObject> enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        enemies.Sort((a, b) =>
            UnityEngine.Vector2.Distance(transform.position, a.transform.position)
            .CompareTo(UnityEngine.Vector2.Distance(transform.position, b.transform.position)));

        for (int i = 0; i < splitAmount; i++)
        {
            if (enemies[i] != null)
            {
                enemies[i].SetActive(false);
            }
            print("Set to false");
        }

    }
    void DrawBeam()
    {
        lineRenderer.SetPosition(0, beamSpawnLocation.position);
        lineRenderer.SetPosition(1, beamSpawnLocation.position + beamSpawnLocation.right * beamLength);
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
        if (UnityEngine.Vector2.Distance(gameObject.transform.position, mousePos) > requiredMouseDistanceFromPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}