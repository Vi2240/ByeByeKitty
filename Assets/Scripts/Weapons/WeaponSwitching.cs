using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This class holds data for each weapon.
[System.Serializable]
public class WeaponEntry
{
    public string Name;             // Name used for collection checks and potentially UI in the future.
    public GameObject WeaponObject; // Weapon game object (child of player).
    public GameObject PickupPrefab; // The prefab to instantiate when dropping.
}

public class WeaponSwitching : MonoBehaviour
{
    // Use a single list for all weapon definitions.
    [SerializeField] List<WeaponEntry> allWeapons = new List<WeaponEntry>();
    [SerializeField] bool startWithFirstWeapon = true; // Option to start with the first weapon in the list.

    List<WeaponEntry> collectedWeapons = new List<WeaponEntry>();
    WeaponEntry currentWeapon = null; // Reference to the currently active weapon entry.
    int currentWeaponCollectedIndex = -1; // Index within the collectedWeapons list.

    void Start()
    {
        if (allWeapons == null || allWeapons.Count == 0)
        {
            Debug.LogError("WeaponSwitching: 'All Weapons' list is not set up in the Inspector!", this);
            enabled = false; // Disable script if no weapons are defined.
            return;
        }

        // Ensure all starting weapon objects are inactive.
        foreach (var entry in allWeapons)
        {
            if (entry.WeaponObject != null)
            {
                entry.WeaponObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"WeaponSwitching: Weapon '{entry.Name}' has no WeaponObject assigned.", this);
            }
            if (entry.PickupPrefab == null)
            {
                Debug.LogWarning($"WeaponSwitching: Weapon '{entry.Name}' has no PickupPrefab assigned (needed for dropping).", this);
            }
        }

        collectedWeapons.Clear(); // Start with no weapons collected.

        // --- Add Starting Weapon(s) ---
        if (startWithFirstWeapon && allWeapons.Count > 0)
        {
            TryToCollectWeapon(allWeapons[0]);
            // Automatically switch to the first collected weapon.
            SwitchToIndex(0);
        }

        // ---- For testing ----
        Debug.LogWarning("WeaponSwitching: Collecting all weapons for testing purposes.");
        foreach (var weaponEntry in allWeapons)
        {
            TryToCollectWeapon(weaponEntry);
        }
        // Ensure the first weapon is active if multiple were added for testing.
        if (collectedWeapons.Count > 0) {
           SwitchToIndex(0);
        }
    }

    void Update()
    {
        // Check weapon switching input.
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (i < collectedWeapons.Count)
                {
                    SwitchToIndex(i);
                }
                else
                {
                    Debug.Log($"WeaponSwitching: No weapon collected at slot {i + 1}.");
                    // Reminder to add "empty slot sound" here latre.
                }
                break;
            }
        }

        // For dropping weapons.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropCurrentWeapon();
        }

        // Scroll wheel switching.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && collectedWeapons.Count > 1) // Only switch if more than one weapon collected.
        {
            int nextIndex = currentWeaponCollectedIndex;
            if (scroll > 0f) // Scroll up
            {
                nextIndex--;
                if (nextIndex < 0) { nextIndex = collectedWeapons.Count - 1; } // Wrap around
            }
            else if (scroll < 0f) // Scroll down
            {
                nextIndex++;
                if (nextIndex >= collectedWeapons.Count) { nextIndex = 0; } // Wrap around
            }
            SwitchToIndex(nextIndex);
        }
    }

    public bool CollectWeaponByName(string weaponName)
    {
        // Find the weapon definition in the master list
        WeaponEntry weaponToCollect = allWeapons.FirstOrDefault(w => w.Name == weaponName);

        if (weaponToCollect == null)
        {
            Debug.LogWarning($"WeaponSwitching: Attempted to collect unknown weapon '{weaponName}'.", this);
            return false;
        }

        // Try to add it, it will handle duplicates.
        bool collected = TryToCollectWeapon(weaponToCollect);

        if (collected)
        {
            Debug.Log($"WeaponSwitching: Collected '{weaponName}'.");
        }
        else
        {
            Debug.Log($"WeaponSwitching: Already have weapon '{weaponName}'.");
        }

        return collected; // Return true if the weapon got collected, otherise false.
    }

    private bool TryToCollectWeapon(WeaponEntry weaponEntry)
    {
        if (weaponEntry == null || weaponEntry.WeaponObject == null) return false; // Error handling

        // Check if this the weapon has already been collected.
        if (!collectedWeapons.Any(w => w.WeaponObject == weaponEntry.WeaponObject))
        {
            collectedWeapons.Add(weaponEntry);
            if (currentWeapon != weaponEntry)
            {
                weaponEntry.WeaponObject.SetActive(false);
            }
            return true;
        }
        return false; // Returns false if the weapon is already collected. 
    }


    private void SwitchToIndex(int index)
    {
        // Check if index is out of bounds.
        if (index < 0 || index >= collectedWeapons.Count)
        {
            Debug.LogError($"WeaponSwitching: Attempted to switch to invalid collected index {index}. Collected count: {collectedWeapons.Count}", this);
            return;
        }

        WeaponEntry targetWeapon = collectedWeapons[index];

        // Only switch to other weapons.
        if (currentWeapon == targetWeapon) { return; }

        // Deactivate current weapon if a weapon is active.
        if (currentWeapon != null && currentWeapon.WeaponObject != null)
        {
            currentWeapon.WeaponObject.SetActive(false);
        }

        // Activate the new weapon.
        if (targetWeapon != null && targetWeapon.WeaponObject != null)
        {
            targetWeapon.WeaponObject.SetActive(true);
            currentWeapon = targetWeapon;
            currentWeaponCollectedIndex = index;
            Debug.Log($"WeaponSwitching: Switched to '{currentWeapon.Name}' (Collected Index: {index}).");
        }
        else
        {
            Debug.LogError($"WeaponSwitching: Target weapon or its object is null at collected index {index}.", this);
            currentWeapon = null;
            currentWeaponCollectedIndex = -1;
        }
    }

    private void DropCurrentWeapon()
    {
        if (currentWeapon == null) { return; }

        if (currentWeapon.PickupPrefab == null)
        {
            Debug.LogWarning($"WeaponSwitching: Cannot drop '{currentWeapon.Name}' because no PickupPrefab is assigned.", this);
            return;
        }

        // Instantiate the pickup item slightly below the player.
        float dropOffsetY = 0.5f; // How far below the player's to drop the item.
        Vector3 dropPosition = transform.position - (Vector3.up * dropOffsetY);

        // Deactivate the weapon object.
        if (currentWeapon.WeaponObject != null)
        {
            currentWeapon.WeaponObject.SetActive(false);
        }

        // Remove the weapon from the collected list.
        int removedIndex = currentWeaponCollectedIndex;
        collectedWeapons.RemoveAt(removedIndex);

        // Switch to another weapon or unarmed state.
        currentWeapon = null;
        currentWeaponCollectedIndex = -1;

        if (collectedWeapons.Count > 0)
        {
            // Switch to the weapon that is now at the same index, or the last one if the dropped weapon was last.
            int nextIndex = Mathf.Clamp(removedIndex, 0, collectedWeapons.Count - 1);
            SwitchToIndex(nextIndex);
        }
        else
        {
            Debug.Log("WeaponSwitching: No weapons left after dropping.");
        }
    }
}