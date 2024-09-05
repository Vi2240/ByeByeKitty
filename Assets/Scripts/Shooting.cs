using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] float bulletForce = 20f;

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rigidbody = bullet.GetComponent<Rigidbody2D>();
        rigidbody.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);
    }
}