using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] Item ammo, laserEnergy, heal, experience;

    const string PLAYER_TAG = "Player";

    void Start()
    {
        // Ensure the Collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.isTrigger)
        {
            Debug.LogWarning($"ItemPickup '{gameObject.name}' needs a Collider2D component set to 'Is Trigger'. Disabling pickup.", gameObject);
            enabled = false; // Disable the script
            return;
        }

        if (!ammo.Enabled && !laserEnergy.Enabled && !heal.Enabled && !experience.Enabled)
        {
            // LogWarning for potential setup errors
            Debug.LogWarning($"Pickup '{gameObject.name}' (ID: {gameObject.GetInstanceID()}) has no item type enabled! Destroying.", gameObject);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth);
            bool itemWasPickedUp = false; // Bool to track if any pickup was successful

            // Ammo
            if (ammo.Enabled && Inventory.ammo < Inventory.maxAmmo)
            {
                int ammoToAdd = ammo.Amount;
                int newAmmoTotal = Inventory.ammo + ammoToAdd;
                Inventory.ammo = Mathf.Clamp(newAmmoTotal, 0, Inventory.maxAmmo); // Clamp ensures we don't exceed max or go below 0
                Debug.Log($"Picked up {ammoToAdd} ammo. Current ammo: {Inventory.ammo}");
                itemWasPickedUp = true;
            }

            // Laser energy
            if (laserEnergy.Enabled && Inventory.energyAmmo < Inventory.maxEnergyAmmo)
            {
                int energyToAdd = laserEnergy.Amount;
                int newEnergyTotal = Inventory.energyAmmo + energyToAdd;
                Inventory.energyAmmo = Mathf.Clamp(newEnergyTotal, 0, Inventory.maxEnergyAmmo); // Use Clamp
                Debug.Log($"Picked up {energyToAdd} laser energy. Current energy: {Inventory.energyAmmo}");
                itemWasPickedUp = true;
            }

            // Heal
            if (heal.Enabled)
            {
                // Null check to avoid errors
                if (playerHealth != null)
                {
                    // Heal function returns true if healed, false if already at full health
                    if (playerHealth.Heal(heal.Amount))
                    {
                        Debug.Log($"Healed player by {heal.Amount}.");
                        itemWasPickedUp = true;
                    }
                }
                else
                {
                    // Log a warning if heal is enabled but the player has no PlayerHealth component
                    Debug.LogWarning($"Pickup '{gameObject.name}' has heal enabled, but colliding object '{other.name}' tagged '{PLAYER_TAG}' has no PlayerHealth component.", other.gameObject);
                }
            }

            // Experience
            if (experience.Enabled)
            {
                int expToAdd = experience.Amount;

                Debug.Log($"Gained {expToAdd} experience.");
                itemWasPickedUp = true; // Make sure experience pickup also marks for destruction
            }

            if (itemWasPickedUp)
            {
                // Maybe add effects like sound or particles here before destroying it, like below
                // SoundManager.Instance.PlayPickupSound();
                // Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
        }
    }
}

[System.Serializable]
struct Item
{
    [SerializeField] bool enabled;
    [SerializeField] int amount;

    public bool Enabled => enabled;
    public int Amount => amount;
}