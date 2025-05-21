using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public string weaponName;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponSwitching ws = other.GetComponent<WeaponSwitching>();
            if (ws == null) { return; }
            
            if (ws.CollectWeaponByName(weaponName)) { Destroy(gameObject); }
        }
    }
}