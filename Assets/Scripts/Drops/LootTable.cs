using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Added for Sum()

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Inventory/Loot Table", order = 1)]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class LootDropItem
    {
        public GameObject itemPrefab;
        [Min(1)]
        public int weight = 1;
    }

    public List<LootDropItem> possibleDrops = new List<LootDropItem>();

    // Cache the total weight to avoid recalculating every time
    private int _cachedTotalWeight = -1; // Start with invalid value
    private bool _needsRecalculation = true;

    /// <summary>
    /// Calculates or retrieves the cached total weight of all items in the loot table.
    /// </summary>
    private int GetTotalWeight()
    {
        // If cache is invalid OR the list was modified (difficult to track perfectly without deeper hooks), recalculate
        if (_needsRecalculation || _cachedTotalWeight < 0)
        {
            // Using LINQ Sum for conciseness
            _cachedTotalWeight = possibleDrops?.Sum(drop => drop?.weight ?? 0) ?? 0;
            _needsRecalculation = false; // Reset flag after calculation
            //Debug.Log($"Recalculated Total Weight for '{this.name}': {_cachedTotalWeight}");
        }
        return _cachedTotalWeight;
    }

    /// <summary>
    /// Picks a random item prefab from the loot table based on assigned weights.
    /// </summary>
    /// <returns>The GameObject prefab of the chosen item, or null if the table is empty, weights are invalid, or pick fails.</returns>
    public GameObject PickLootDrop()
    {
        if (possibleDrops == null || possibleDrops.Count == 0)
        {
            // Debug.LogWarning($"LootTable '{this.name}' has no possible drops defined."); // Less verbose - might be intentional
            return null;
        }

        int totalWeight = GetTotalWeight();

        if (totalWeight <= 0)
        {
            // This case implies all weights are zero or negative, or list is empty but not null.
            // Only warn if there are actually items defined but weights are wrong.
            if (possibleDrops.Any(d => d != null && d.itemPrefab != null)) // Check if there *should* be valid drops
            {
                Debug.LogWarning($"LootTable '{this.name}' has a total weight of {totalWeight}. Check item weights are positive.", this);
            }
            return null; // Cannot pick if total weight is not positive
        }

        // Generate a random value within the total weight range
        // Using int range here is often more robust with integer weights
        int randomValue = Random.Range(0, totalWeight); // Range(int, int) max is exclusive

        // Determine which drop corresponds to the random value
        int cumulativeWeight = 0;
        foreach (var drop in possibleDrops)
        {
            // Ensure drop and prefab are valid before processing
            if (drop == null || drop.itemPrefab == null || drop.weight <= 0) continue;

            cumulativeWeight += drop.weight;
            if (randomValue < cumulativeWeight) // Check if the random value falls within this item's weight range
            {
                return drop.itemPrefab;
            }
        }

        // Fallback: Should rarely be reached if logic is sound and totalWeight > 0
        // Could happen due to floating point inaccuracies if using floats, or if all items were filtered out (e.g., null prefabs)
        Debug.LogError($"LootTable '{this.name}': Failed to pick a drop despite TotalWeight={totalWeight}. RandomValue={randomValue}. Returning null.");
        return null; // Return null instead of first item to clearly indicate failure
    }

    // Invalidate cache when values change in the editor
    private void OnValidate()
    {
        _needsRecalculation = true; // Mark cache for recalculation

        if (possibleDrops == null)
        {
            Debug.LogWarning($"LootTable '{this.name}': Possible Drops list is null.", this);
            return; // Exit early if list is null
        }

        // Basic checks during validation
        bool hasValidEntries = false;
        int tempTotalWeight = 0;
        foreach (var item in possibleDrops)
        {
            if (item == null)
            {
                Debug.LogWarning($"LootTable '{this.name}': Found a null entry in Possible Drops.", this);
                continue;
            }
            if (item.itemPrefab == null)
            {
                Debug.LogWarning($"LootTable '{this.name}': Found an entry with a missing Item Prefab.", this);
            }
            if (item.weight < 1)
            {
                Debug.LogWarning($"LootTable '{this.name}': Item '{item.itemPrefab?.name ?? "NULL Prefab"}' has a weight less than 1 ({item.weight}). It won't be dropped.", this);
            }
            else
            {
                tempTotalWeight += item.weight; // Only add valid weights
                if (item.itemPrefab != null) hasValidEntries = true;
            }
        }

        if (hasValidEntries && tempTotalWeight <= 0)
        {
            Debug.LogError($"LootTable '{this.name}': Contains valid item prefabs, but the total weight is {tempTotalWeight}. Ensure items intended to drop have positive weights.", this);
        }
        // No need to call GetTotalWeight() here, just flag for recalc
    }
}