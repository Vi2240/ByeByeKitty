using UnityEngine;
using System.Collections.Generic;

public class LootTableProgressionManager : MonoBehaviour
{
    [System.Serializable]
    public struct WaveLootTableEntry
    {
        public int afterWaveNumber; // The loot table to use *after* this wave number completes
        public LootTable lootTable;
    }

    [Tooltip("Define loot tables to be used after specific wave numbers. Sorted by afterWaveNumber ascending.")]
    public List<WaveLootTableEntry> waveLootProgression = new List<WaveLootTableEntry>();

    private int currentWaveNumberForLoot = 0; // Tracks the number of continuous waves completed

    /// <summary>
    /// Call this when a continuous wave completes.
    /// </summary>
    public void OnContinuousWaveCompleted()
    {
        currentWaveNumberForLoot++;
        UpdateLootManagerTable();
    }

    /// <summary>
    /// Can be called to reset the wave count, e.g., when starting a new game or level.
    /// </summary>
    public void ResetWaveCount()
    {
        currentWaveNumberForLoot = 0;
        UpdateLootManagerTable(); // Update to the initial table if defined for wave 0
    }

    private void UpdateLootManagerTable()
    {
        if (LootManager.Instance == null)
        {
            Debug.LogError("LootTableProgressionManager: LootManager.Instance is null. Cannot update loot table.");
            return;
        }

        LootTable tableToSet = GetLootTableForCurrentWave();
        if (tableToSet != null)
        {
            LootManager.Instance.UpdateLootTable(tableToSet);
        }
        else
        {
            // Optional: Could revert to a very basic default or log that no specific table was found for this progression point
            Debug.LogWarning($"LootTableProgressionManager: No specific loot table found for wave completion number {currentWaveNumberForLoot}. LootManager's default will remain unchanged unless it was null.");
            // If LootManager.Instance.CurrentDefaultLootTable is null, you might want to set it to a fallback here
            // if (LootManager.Instance.CurrentDefaultLootTable == null && someFallbackLootTable != null) {
            //     LootManager.Instance.UpdateLootTable(someFallbackLootTable);
            // }
        }
    }

    /// <summary>
    /// Gets the appropriate loot table based on the number of waves completed.
    /// It will pick the table defined for the highest 'afterWaveNumber' that is less than or equal to currentWaveNumberForLoot.
    /// </summary>
    private LootTable GetLootTableForCurrentWave()
    {
        LootTable selectedTable = null;
        // Iterate in reverse to find the most recent applicable entry
        for (int i = waveLootProgression.Count - 1; i >= 0; i--)
        {
            if (currentWaveNumberForLoot >= waveLootProgression[i].afterWaveNumber)
            {
                selectedTable = waveLootProgression[i].lootTable;
                break; // Found the most relevant one
            }
        }
        return selectedTable;
    }

    // Optional: Call this at the start if you want to initialize the LootManager with the first table
    void Start()
    {
        // Make sure LootManager is ready
        if (LootManager.Instance != null) {
             UpdateLootManagerTable(); // Sets the initial loot table based on wave 0
        } else {
            // Could wait a frame or use a coroutine if LootManager might not be ready in Start
            Debug.LogWarning("LootTableProgressionManager: LootManager not ready in Start. Initial loot table might not be set immediately.");
        }
    }
}