using UnityEngine;
using System.Collections.Generic;

public class LootManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static LootManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- Manager Controlled Settings ---
    [Header("Loot Control")]
    [SerializeField, Tooltip("The default Loot Table used if no specific table is provided by the request.")]
    private LootTable defaultLootTable; // Manager controls the default table

    [SerializeField, Range(0f, 1f), Tooltip("The overall probability (0 to 1) that *any* loot will drop from any request handled by this manager.")]
    private float overallDropChance = 0.75f; // Manager controls the overall chance

    [Header("Global Drop Style")]
    [SerializeField] private bool useRandomOffset = true;
    [SerializeField] private Vector2 dropOffsetRange = new Vector2(0.5f, 0.5f);

    public LootTable CurrentDefaultLootTable
    {
        get { return defaultLootTable; }
        set
        {
            if (value != null)
            {
                defaultLootTable = value;
                Debug.Log($"LootManager default table set to: {value.name}");
            }
            else
            {
                Debug.LogWarning("Attempted to set default LootTable to null.");
            }
        }
    }
    

    /// <summary>
    /// Updates the default loot table used by the LootManager.
    /// </summary>
    /// <param name="newLootTable">The LootTable Scriptable Object to set as the new default.</param>
    public void UpdateLootTable(LootTable newLootTable)
    {
        if (newLootTable != null)
        {
            defaultLootTable = newLootTable;
            Debug.Log($"LootManager default loot table updated to: {newLootTable.name}");
        }
        else
        {
            Debug.LogWarning("LootManager: Attempted to update default loot table with a null LootTable.");
        }
    }

    /// <summary>
    /// Centralized method called by enemies (or other sources) to request loot drops.
    /// Uses the manager's overallDropChance. Uses the provided overrideLootTable if not null, otherwise uses the manager's defaultLootTable.
    /// </summary>
    /// <param name="dropPosition">The world position where loot should spawn.</param>
    /// <param name="minCount">Minimum items to drop (determined by caller).</param>
    /// <param name="maxCount">Maximum items to drop (determined by caller).</param>
    /// <param name="overrideLootTable">Optional: A specific LootTable to use for this request, bypassing the default.</param>
    public void RequestLootDrop(Vector3 dropPosition, int minCount, int maxCount, LootTable overrideLootTable = null)
    {
        // 1. Check manager's overall drop chance
        if (Random.value > overallDropChance)
        {
            return; // Failed chance check
        }

        // 2. Determine which LootTable to use
        LootTable tableToUse = overrideLootTable != null ? overrideLootTable : defaultLootTable;

        // 3. Validate the selected LootTable
        if (tableToUse == null)
        {
            Debug.LogError($"LootManager: No valid LootTable selected for drop request. Default is '{defaultLootTable?.name}', Override was '{(overrideLootTable == null ? "null" : overrideLootTable.name)}'.", this);
            return;
        }

        // 4. Determine number of drops for THIS request (using caller's min/max)
        int numberOfDrops = Random.Range(minCount, maxCount + 1);
        if (numberOfDrops <= 0)
        {
            return; // No drops this time
        }

        for (int i = 0; i < numberOfDrops; i++)
        {
            GameObject itemToDropPrefab = tableToUse.PickLootDrop();

            if (itemToDropPrefab != null)
            {
                // Apply random offset to spawn location.
                Vector3 finalSpawnPosition = dropPosition;
                if (useRandomOffset)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(-dropOffsetRange.x, dropOffsetRange.x), Random.Range(-dropOffsetRange.y, dropOffsetRange.y), 0);
                    finalSpawnPosition += randomOffset;
                }

                // Find out if it's a weapon it's trying to drop, and wether it sohuld be dropped or not.
                WeaponPickup weaponPickupComponent = itemToDropPrefab.GetComponent<WeaponPickup>();
                bool instantiateThisItem = true;
                if (weaponPickupComponent != null)
                {
                    if (!string.IsNullOrEmpty(weaponPickupComponent.weaponName))
                    {
                        if (InventoryAndBuffs.collectedAndDroppedWeapons != null && InventoryAndBuffs.collectedAndDroppedWeapons.Contains(weaponPickupComponent.weaponName))
                        {
                            instantiateThisItem = false;
                        }
                    }
                    else { Debug.LogError($"Weapon pickup prefab '{itemToDropPrefab.name}' has empty weaponName!", itemToDropPrefab); instantiateThisItem = false; /* Prevent dropping invalid item */}
                }

                if (instantiateThisItem)
                {
                    GameObject droppedItemInstance = Instantiate(itemToDropPrefab, finalSpawnPosition, Quaternion.identity);

                    if (weaponPickupComponent != null && !string.IsNullOrEmpty(weaponPickupComponent.weaponName))
                    {
                        if (InventoryAndBuffs.collectedAndDroppedWeapons != null && !InventoryAndBuffs.collectedAndDroppedWeapons.Contains(weaponPickupComponent.weaponName))
                        {
                            InventoryAndBuffs.collectedAndDroppedWeapons.Add(weaponPickupComponent.weaponName);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"LootTable '{tableToUse.name}' returned null on drop attempt {i + 1}/{numberOfDrops}.", this);
            }
        }
    }
}