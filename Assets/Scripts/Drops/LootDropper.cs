using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [Header("Loot Settings")]
    [SerializeField, Tooltip("The Loot Table asset defining possible drops.")]
    private LootTable lootTable;

    [SerializeField, Range(0f, 1f), Tooltip("The overall probability (0 to 1) that *any* loot will drop from this enemy.")]
    private float overallDropChance = 0.75f; // Example: 75% chance to drop anything

    [SerializeField, Tooltip("Minimum number of items to drop *if* the overall drop chance succeeds.")]
    [Min(0)] // Allowing 0 here is useful if overallDropChance is less than 1
    private int minDrops = 1;

    [SerializeField, Tooltip("Maximum number of items to drop *if* the overall drop chance succeeds.")]
    [Min(0)] // Allow 0 for consistency, though usually > minDrops
    private int maxDrops = 1;

    [Header("Drop Location & Style")]
    [SerializeField, Tooltip("Optional: Specific point where items should spawn. If null, uses this GameObject's position.")]
    private Transform dropSpawnPoint;

    [SerializeField, Tooltip("Apply a random offset to each dropped item's position?")]
    private bool useRandomOffset = true;

    [SerializeField, Tooltip("Maximum random offset distance on X and Y axes.")]
    private Vector2 dropOffsetRange = new Vector2(0.5f, 0.5f);

    [SerializeField, Tooltip("Add a small force to dropped items (requires Rigidbody2D)?")]
    private bool addDropForce = true;

    [SerializeField, Tooltip("Range of the magnitude of the force applied.")]
    private Vector2 dropForceMagnitudeRange = new Vector2(1f, 3f);


    /// <summary>
    /// Call this method to attempt dropping items (e.g., on enemy death).
    /// Takes into account the overallDropChance first.
    /// </summary>
    public void AttemptDropItems() // Renamed for clarity
    {
        // 1. Check if loot drops at all based on overall chance
        if (Random.value > overallDropChance) // Random.value is [0.0, 1.0]
        {
            //Debug.Log($"'{gameObject.name}' failed overall drop chance roll.");
            return; // Didn't pass the chance check, drop nothing.
        }

        // --- Passed the overall drop chance check ---

        if (lootTable == null)
        {
            Debug.LogError($"LootDropper on '{gameObject.name}' is missing a LootTable reference.", this);
            return;
        }

        // 2. Determine how many items to drop this time
        int numberOfDrops = Random.Range(minDrops, maxDrops + 1); // maxDrops is inclusive

        if (numberOfDrops <= 0)
        {
            //Debug.Log($"'{gameObject.name}' rolled {numberOfDrops} drops, dropping nothing.");
            return; // Calculated drops is 0 or less
        }

        Vector3 spawnPositionBase = (dropSpawnPoint != null) ? dropSpawnPoint.position : transform.position;

        //Debug.Log($"'{gameObject.name}' dropping {numberOfDrops} items from {lootTable.name} at {spawnPositionBase}");

        // 3. Pick and instantiate items
        for (int i = 0; i < numberOfDrops; i++)
        {
            GameObject itemToDropPrefab = lootTable.PickLootDrop();

            if (itemToDropPrefab != null)
            {
                Vector3 spawnPosition = spawnPositionBase;

                // Optional: Add random offset
                if (useRandomOffset)
                {
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-dropOffsetRange.x, dropOffsetRange.x),
                        Random.Range(-dropOffsetRange.y, dropOffsetRange.y),
                        0);
                    spawnPosition += randomOffset;
                }

                // Instantiate the chosen item
                GameObject droppedItemInstance = Instantiate(itemToDropPrefab, spawnPosition, Quaternion.identity);

                // Optional: Add force if requested and Rigidbody2D exists
                if (addDropForce)
                {
                    Rigidbody2D rb2d = droppedItemInstance.GetComponent<Rigidbody2D>();
                    if (rb2d != null)
                    {
                        Vector2 randomDirection = Random.insideUnitCircle.normalized; // Get a random direction
                        float randomMagnitude = Random.Range(dropForceMagnitudeRange.x, dropForceMagnitudeRange.y);
                        rb2d.AddForce(randomDirection * randomMagnitude, ForceMode2D.Impulse);
                    }
                    else
                    {
                        Debug.LogWarning($"LootDropper on '{gameObject.name}' tried to add force to '{itemToDropPrefab.name}', but it has no Rigidbody2D.", droppedItemInstance);
                    }
                }
            }
            else
            {
                // Log if the loot table failed to provide an item
                Debug.LogWarning($"LootTable '{lootTable.name}' returned null on drop attempt {i + 1}/{numberOfDrops} for '{gameObject.name}'. Check table contents/weights.", this);
            }
        }
    }

    // Ensure maxDrops is always at least minDrops in the editor
    private void OnValidate()
    {
        // Allow minDrops to be 0
        if (minDrops < 0) minDrops = 0;
        if (maxDrops < minDrops)
        {
            maxDrops = minDrops;
        }
        // Also ensure maxDrops isn't negative if minDrops is 0
        if (maxDrops < 0) maxDrops = 0;
    }
}