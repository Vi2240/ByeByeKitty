using UnityEngine;
using System.Collections.Generic;

// This attribute allows you to create instances of this class as assets in the Project window
[CreateAssetMenu(fileName = "NewLootTable", menuName = "Inventory/Loot Table", order = 1)]
public class LootTable : ScriptableObject
{
    [System.Serializable] // Makes this visible and editable in the Inspector when used in a List
    public class LootDropItem
    {
        public GameObject itemPrefab; // The item to potentially drop
        [Min(1)] // Min weight
        public int weight = 1;
    }

    // List of all possible items and their weights for this table
    public List<LootDropItem> possibleDrops = new List<LootDropItem>();

    /// <summary>
    /// Calculates the total weight of all items in the loot table.
    /// </summary>
    private int CalculateTotalWeight()
    {
        int totalWeight = 0;
        foreach (var drop in possibleDrops)
        {
            totalWeight += drop.weight;
        }
        return totalWeight;
    }

    /// <summary>
    /// Picks a random item prefab from the loot table based on assigned weights.
    /// </summary>
    /// <returns>The GameObject prefab of the chosen item, or null if the table is empty or weights are invalid.</returns>
    public GameObject PickLootDrop()
    {
        if (possibleDrops == null || possibleDrops.Count == 0)
        {
            Debug.LogWarning($"LootTable '{this.name}' has no possible drops defined.");
            return null;
        }

        int totalWeight = CalculateTotalWeight();
        if (totalWeight <= 0)
        {
            Debug.LogWarning($"LootTable '{this.name}' has a total weight of 0 or less. Check item weights.");
            // Optionally, return the first item or handle as an error
            if (possibleDrops.Count > 0) return possibleDrops[0].itemPrefab;
            return null;
        }


        // Generate a random value within the total weight range
        float randomValue = Random.Range(0f, totalWeight); // Use Range(0f, total) for inclusivity clarity

        // Determine which drop corresponds to the random value
        float cumulativeWeight = 0;
        foreach (var drop in possibleDrops)
        {
            cumulativeWeight += drop.weight;
            if (randomValue < cumulativeWeight) // Use < for clearer range check [0..weight)
            {
                return drop.itemPrefab;
            }
        }

        // Fallback: Should theoretically not be reached if totalWeight > 0
        Debug.LogError($"LootTable '{this.name}': Failed to pick a drop. RandomValue: {randomValue}, TotalWeight: {totalWeight}");
        return (possibleDrops.Count > 0) ? possibleDrops[0].itemPrefab : null; // Return first item as safety
    }

    // Optional: Add validation in the editor
    private void OnValidate()
    {
        int totalW = 0;
        if (possibleDrops != null)
        {
            foreach (var item in possibleDrops)
            {
                if (item.weight < 1)
                {
                    Debug.LogWarning($"LootTable '{this.name}': Item '{item.itemPrefab?.name ?? "NULL"}' has a weight less than 1.", this);
                }
                if (item.itemPrefab == null)
                {
                    Debug.LogWarning($"LootTable '{this.name}': Found an entry with a missing Item Prefab.", this);
                }
                totalW += item.weight;
            }
            if (possibleDrops.Count > 0 && totalW <= 0)
            {
                Debug.LogError($"LootTable '{this.name}': The total weight is {totalW}. Ensure items have positive weights.", this);
            }
        }
        else
        {
            Debug.LogWarning($"LootTable '{this.name}': Possible Drops list is null.", this);
        }
    }
}