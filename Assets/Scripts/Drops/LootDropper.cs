using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField, Tooltip("The Loot Table asset defining possible drops.")]
    private LootTable lootTable;

    [SerializeField, Tooltip("Minimum number of items to drop.")]
    [Min(0)]
    private int minDrops = 1;

    [SerializeField, Tooltip("Maximum number of items to drop.")]
    [Min(1)]
    private int maxDrops = 1;

    [Header("Drop Location")]
    [SerializeField, Tooltip("Optional: Specific point where items should spawn. If null, uses this GameObject's position.")]
    private Transform dropSpawnPoint;


    /// <summary>
    /// Call this method to trigger the item drop process (e.g., on enemy death).
    /// </summary>
    public void DropItems()
    {
        if (lootTable == null)
        {
            Debug.LogError($"ItemDropper on '{gameObject.name}' is missing a LootTable reference.", this);
            return;
        }

        // Determine how many items to drop this time
        int numberOfDrops = Random.Range(minDrops, maxDrops + 1); // maxDrops is inclusive

        if (numberOfDrops <= 0) return; // Don't drop anything if calculated drops is 0 or less

        Vector3 spawnPosition = (dropSpawnPoint != null) ? dropSpawnPoint.position : transform.position;

        //Debug.Log($"Dropping {numberOfDrops} items from {lootTable.name} at {spawnPosition}");

        for (int i = 0; i < numberOfDrops; i++)
        {
            GameObject itemToDropPrefab = lootTable.PickLootDrop();

            if (itemToDropPrefab != null)
            {
                // Instantiate the chosen item at the determined position
                Instantiate(itemToDropPrefab, spawnPosition, Quaternion.identity);
                // Optional: Add some random offset to the spawn position so items don't stack perfectly
                // Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                // Instantiate(itemToDropPrefab, spawnPosition + randomOffset, Quaternion.identity);

                // Optional: Add velocity/force to the dropped item's Rigidbody if it has one
            }
            else
            {
                // Log if the loot table failed to provide an item (might indicate an empty table or issue)
                Debug.LogWarning($"LootTable '{lootTable.name}' returned null on drop attempt {i + 1}/{numberOfDrops} for '{gameObject.name}'.", this);
            }
        }
    }

    // Ensure maxDrops is always at least minDrops in the editor
    private void OnValidate()
    {
        if (maxDrops < minDrops)
        {
            maxDrops = minDrops;
        }
    }

    // --- Example Usage (Remove or adapt for your game) ---
    // Example: Call DropItems when the object is destroyed
    // void OnDestroy()
    // {
    //     // Make sure this is only called if the game isn't quitting,
    //     // and potentially only if killed by the player, etc.
    //     if (gameObject.scene.isLoaded) // Basic check to avoid errors during scene unload/quit
    //     {
    //          DropItems();
    //     }
    // }

    // Example: Call DropItems with a key press for testing
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.L))
    //     {
    //         DropItems();
    //     }
    // }
}