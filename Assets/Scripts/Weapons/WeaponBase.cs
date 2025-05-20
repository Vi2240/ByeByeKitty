using UnityEngine;
using System.Collections;
// using System.Collections.Generic; // Not strictly needed by this base class as is

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected Transform projectileSpawnLocation;
    [SerializeField] protected Transform weaponSpriteTransform;
    [SerializeField] protected float fireCooldown;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected float damagePerHit;
    [SerializeField] protected int magazineSizeMax;
    //[SerializeField] protected int magazineReservesMax;
    [SerializeField] protected int currentMagAmmoCount;
    [SerializeField] float requiredMouseDistanceFromPlayer = 1.5f;
    [SerializeField] protected bool nerfMovementDuringAction = true;
    
    protected Movement playerMovement;
    protected Vector3 mousePos;
    protected Camera mainCam;
    protected bool canFire = true;
    protected bool canReload = true;
    protected bool isReloading = false;

    private Coroutine _shootCooldownCoroutineRef; // To store and manage the cooldown

    protected virtual void Start() // Kept as Start, ensure mainCam is set appropriately for children
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<Movement>();
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("WeaponBase: Main Camera not found! Weapon rotation will not work.", this);
        }
    }

    protected virtual void OnEnable()
    {
        playerMovement.DisableNerfedMovement();
        canFire = true; // Ensure weapon is ready to fire when enabled
        if (_shootCooldownCoroutineRef != null)
        {
            StopCoroutine(_shootCooldownCoroutineRef);
            _shootCooldownCoroutineRef = null;
        }
    }

    protected virtual void OnDisable()
    {
        playerMovement.DisableNerfedMovement();
        if (_shootCooldownCoroutineRef != null)
        {
            StopCoroutine(_shootCooldownCoroutineRef);
            _shootCooldownCoroutineRef = null;
        }
        canFire = true; // Reset state to ensure it's not stuck if disabled mid-cooldown
    }


    protected virtual void Update()
    {
        if (mainCam != null)
        {
            WeaponRotation();
        }

        //if (!MouseFarEnoughFromPlayer()) { canFire = false; }
    }

    protected void WeaponRotation()
    {
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        if (weaponSpriteTransform != null)
        {
            Vector3 saveScale = weaponSpriteTransform.localScale;
            // This logic seems to intend to flip the sprite based on rotation.
            // Original flipping logic:
            // saveScale.y *= (Mathf.Abs(rotZ) > 90) ? (saveScale.y > 0 ? -1 : 1) : (saveScale.y < 0 ? -1 : 1);
            // Simplified flipping logic to ensure it flips correctly:
            if (Mathf.Abs(rotZ) > 90) // Facing left
            {
                if (saveScale.y > 0) saveScale.y *= -1; // If currently facing right, flip
            }
            else // Facing right
            {
                if (saveScale.y < 0) saveScale.y *= -1; // If currently facing left, flip
            }
            weaponSpriteTransform.localScale = saveScale;
        }
    }

    // This is the method child classes should call instead of directly starting their own coroutine
    // The existing ShootCooldown() in child classes can remain, but its content will be moved here.
    // For minimum changes in child scripts, they will still call StartCoroutine(ShootCooldown())
    // but this ShootCooldown() will now manage the reference.
    protected IEnumerator ShootCooldown()
    {
        // If a child calls StartCoroutine(ShootCooldown()), this method will be executed.
        // We manage the single reference here.
        if (_shootCooldownCoroutineRef != null)
        {
            StopCoroutine(_shootCooldownCoroutineRef); // Stop the old one if any
        }
        // Start the actual logic and store its reference
        _shootCooldownCoroutineRef = StartCoroutine(ExecuteShootCooldownLogicInternal());
        yield return _shootCooldownCoroutineRef; // Optionally wait for it if the child needs to
    }

    private IEnumerator ExecuteShootCooldownLogicInternal()
    {
        canFire = false;
        float cooldown = fireCooldown;
        if (InventoryAndBuffs.playerFireRateMultiplier != 0)
        {
            cooldown = fireCooldown / InventoryAndBuffs.playerFireRateMultiplier;
        }
        else if (fireCooldown > 0) // Prevent division by zero if multiplier is 0 but fireCooldown is positive
        {
            Debug.LogWarning("PlayerFireRateMultiplier is zero, using base fireCooldown.");
        }


        yield return new WaitForSeconds(cooldown);
        canFire = true;
        _shootCooldownCoroutineRef = null; // Clear the reference once done
    }

    protected abstract void Fire();

    bool MouseFarEnoughFromPlayer()
    {
        if (mainCam == null) return false;
        return Vector2.Distance(transform.position, mousePos) > requiredMouseDistanceFromPlayer;
    }
}