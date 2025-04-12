using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

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

    [Header("Starting Options")]
    [SerializeField] bool startWithFirstWeapon = true; // Option to start with the first weapon in the list.
    // --- ADDED SERIALIZE FIELD ---
    [SerializeField] bool collectAllWeaponsOnStartForTesting = false; // If true, grants all weapons at start.

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

        // Ensure all starting weapon objects are inactive and check definitions.
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

        // --- Logic for Adding Starting Weapons ---

        // If the testing flag is enabled, collect all defined weapons.
        if (collectAllWeaponsOnStartForTesting)
        {
            Debug.LogWarning("WeaponSwitching: CollectAllWeaponsOnStartForTesting is TRUE. Collecting all weapons.");
            foreach (var weaponEntry in allWeapons)
            {
                TryToCollectWeapon(weaponEntry); // Attempt to add each weapon
            }
        }
        else if (startWithFirstWeapon && allWeapons.Count > 0)
        {
            TryToCollectWeapon(allWeapons[0]); // Add only the first weapon
        }

        // After potentially adding starting weapons, switch to the first available one (index 0).
        if (collectedWeapons.Count > 0)
        {
            SwitchToIndex(0);
        }
        else
        {
            Debug.Log("WeaponSwitching: Starting with no weapons equipped.");
            // Ensure state is clean if no weapons were added
            currentWeapon = null;
            currentWeaponCollectedIndex = -1;
        }

        // ---- Old For testing block removed as it's replaced by the bool flag ----
    }

    void Update()
    {
        // Check weapon switching input (1-9 keys).
        for (int i = 0; i < 9; i++) // Check keys 1 through 9
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Check if the requested index exists in the *collected* weapons list.
                if (i < collectedWeapons.Count)
                {
                    SwitchToIndex(i);
                }
                else
                {
                    // Debug.Log($"WeaponSwitching: No weapon collected at slot {i + 1}.");
                    // Reminder: Add "empty slot sound" here later.
                }
                break; // Exit loop once a key is processed
            }
        }

        // For dropping weapons.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropCurrentWeapon();
        }

        // Scroll wheel switching.
        HandleScrollWheelSwitching();
    }

    void HandleScrollWheelSwitching()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && collectedWeapons.Count > 1) // Only switch if more than one weapon collected.
        {
            int nextIndex = currentWeaponCollectedIndex;
            if (scroll > 0f) // Scroll up (previous weapon)
            {
                nextIndex--;
                if (nextIndex < 0) { nextIndex = collectedWeapons.Count - 1; } // Wrap around to the end
            }
            else // scroll < 0f (Scroll down - next weapon)
            {
                nextIndex++;
                if (nextIndex >= collectedWeapons.Count) { nextIndex = 0; } // Wrap around to the beginning
            }

            // Only switch if the index actually changed (prevents issues if only 1 weapon)
            if (nextIndex != currentWeaponCollectedIndex)
            {
                SwitchToIndex(nextIndex);
            }
        }
    }

    public bool CollectWeaponByName(string weaponName)
    {
        // Find the weapon definition in the master list. Case-insensitive compare is safer.
        WeaponEntry weaponToCollect = allWeapons.FirstOrDefault(w => w.Name.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase));

        if (weaponToCollect == null)
        {
            Debug.LogWarning($"WeaponSwitching: Attempted to collect unknown weapon '{weaponName}'. Check spelling and definition list.", this);
            return false;
        }

        // Try to add it; this method handles duplicates.
        bool collected = TryToCollectWeapon(weaponToCollect);

        if (collected)
        {
            Debug.Log($"WeaponSwitching: Collected '{weaponName}'.");
            // If this is the *first* weapon collected, switch to it immediately.
            if (collectedWeapons.Count == 1)
            {
                SwitchToIndex(0);
            }
        }
        // else: Weapon was already collected, TryToCollectWeapon returned false. (No message needed here)

        return collected; // Return true if the weapon was newly collected, otherwise false.
    }

    private bool TryToCollectWeapon(WeaponEntry weaponEntry)
    {
        if (weaponEntry?.WeaponObject == null)
        {
            Debug.LogError("WeaponSwitching: Invalid WeaponEntry provided.", this);
            return false;
        }

        if (!collectedWeapons.Any(w => w.Name == weaponEntry.Name))
        {
            collectedWeapons.Add(weaponEntry);

            if (!InventoryAndBuffs.collectedAndDroppedWeapons.Contains(weaponEntry.Name))
            {
                InventoryAndBuffs.collectedAndDroppedWeapons.Add(weaponEntry.Name);
                Debug.Log($"'{weaponEntry.Name}' added to global collected list.");
            }

            weaponEntry.WeaponObject.SetActive(false);
            return true; // Weapon was successfully added to CURRENT inventory.
        }

        // Weapon was already in the CURRENT collectedWeapons list.
        return false;
    }

    // Rest of WeaponSwitching.cs remains the same...

    private void SwitchToIndex(int index)
    {
        // Validate index against the *currently collected* weapons count.
        if (index < 0 || index >= collectedWeapons.Count)
        {
            // This can happen if a weapon was just dropped. Log as warning, not error.
            Debug.LogWarning($"WeaponSwitching: Attempted to switch to invalid collected index {index}. Collected count: {collectedWeapons.Count}", this);
            // Optionally, try to switch to the last available weapon if index is too high?
            // if (collectedWeapons.Count > 0) index = collectedWeapons.Count - 1; else return;
            return; // Exit if index is truly invalid (e.g., -1 or >= count when count > 0)
        }

        // Get the target weapon entry from the *collected* list.
        WeaponEntry targetWeapon = collectedWeapons[index];

        // Prevent switching if the target is somehow null (shouldn't happen with checks)
        if (targetWeapon == null || targetWeapon.WeaponObject == null)
        {
            Debug.LogError($"WeaponSwitching: Target weapon entry or its WeaponObject is null at collected index {index}. Aborting switch.", this);
            return;
        }

        // Only perform actions if switching to a *different* weapon.
        if (currentWeapon == targetWeapon) { return; }

        // Deactivate the previously active weapon, if there was one.
        if (currentWeapon != null && currentWeapon.WeaponObject != null)
        {
            // Check if it's still active before deactivating (safety check)
            if (currentWeapon.WeaponObject.activeSelf)
            {
                currentWeapon.WeaponObject.SetActive(false);
                // Debug.Log($"Deactivated {currentWeapon.Name}.");
            }
        }

        // Activate the new weapon.
        targetWeapon.WeaponObject.SetActive(true);
        currentWeapon = targetWeapon;
        currentWeaponCollectedIndex = index;
        // Debug.Log($"WeaponSwitching: Switched to '{currentWeapon.Name}' (Collected Index: {index}).");
    }

    private void DropCurrentWeapon()
    {
        // Can't drop if no weapon is currently equipped.
        if (currentWeapon == null)
        {
            // Debug.Log("WeaponSwitching: No weapon equipped to drop.");
            return;
        }

        if (currentWeapon.PickupPrefab == null)
        {
            Debug.LogWarning($"WeaponSwitching: Cannot drop '{currentWeapon.Name}' because no PickupPrefab is assigned in its WeaponEntry.", this);
            return;
        }

        Debug.Log($"WeaponSwitching: Dropping '{currentWeapon.Name}'.");

        // Instantiate the pickup item slightly below the player.
        float dropOffsetY = 0.5f; // Adjust as needed for visual placement.
        Vector3 dropPosition = transform.position - (Vector3.up * dropOffsetY);
        Instantiate(currentWeapon.PickupPrefab, dropPosition, Quaternion.identity);

        // --- IMPORTANT: Update state BEFORE removing from list ---
        WeaponEntry droppedWeapon = currentWeapon; // Keep reference for list removal
        int removedIndex = currentWeaponCollectedIndex; // Store the index before modifying list/state

        // Deactivate the weapon object being dropped.
        if (droppedWeapon.WeaponObject != null)
        {
            droppedWeapon.WeaponObject.SetActive(false);
        }

        // Set current weapon state to none temporarily.
        currentWeapon = null;
        currentWeaponCollectedIndex = -1;

        // Remove the weapon from the collected list *using the stored reference*.
        collectedWeapons.Remove(droppedWeapon); // More robust than removing by index if list order changes unexpectedly.

        // Switch to another weapon if available.
        if (collectedWeapons.Count > 0)
        {
            // Try to switch to the weapon that is now at the same index (or the last one if the dropped weapon was last).
            int nextIndex = Mathf.Clamp(removedIndex, 0, collectedWeapons.Count - 1);
            SwitchToIndex(nextIndex);
        }
        else
        {
            Debug.Log("WeaponSwitching: No weapons left after dropping.");
            // Already set currentWeapon = null above.
        }
    }
}