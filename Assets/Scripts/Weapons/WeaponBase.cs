using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected Transform projectileSpawnLocation;
    [SerializeField] protected Transform weaponSpriteTransform;
    [SerializeField] protected float fireCooldown;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected float damagePerHit;
    [SerializeField] protected int magazineSizeMax;
    [SerializeField] protected int magazineReservesMax;
    [SerializeField] protected int currentMagAmmoCount;
    [SerializeField] float requiredMouseDistanceFromPlayer = 1.5f;


    protected Vector3 mousePos;
    protected Camera mainCam;
    protected bool canFire = true;
    protected bool canReload = true;
    protected bool isReloading = false;


    protected virtual void Start()
    {
        mainCam = Camera.main;
    }


    protected virtual void Update()
    {
        WeaponRotation();
    }


    protected void WeaponRotation()
    {
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        Vector3 saveScale = weaponSpriteTransform.localScale;
        saveScale.y *= (Mathf.Abs(rotZ) > 90) ? (saveScale.y > 0 ? -1 : 1) : (saveScale.y < 0 ? -1 : 1);
        weaponSpriteTransform.localScale = saveScale;
    }


    protected IEnumerator ShootCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }


    protected abstract void Fire(); // Enforce that each weapon must implement Fire()

    bool MouseFarEnoughFromPlayer()
    {
        return Vector2.Distance(transform.position, mousePos) > requiredMouseDistanceFromPlayer;
    }
}