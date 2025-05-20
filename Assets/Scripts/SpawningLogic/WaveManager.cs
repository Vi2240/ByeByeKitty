// WaveManager.cs

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages waves and supports running multiple wave coroutines concurrently,
/// as well as a continuous cycle of randomized waves.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Quick fix boss spawn")]
    [SerializeField] GameObject bossPrefab;
    [SerializeField] Transform bossSpawnPoint;

    [Header("Wave Prefabs")]
    public NormalEnemyWave normalEnemyWave;  // Prefab with the WaveType0 component.
    public RangedEnemyWave rangedEnemyWave;
    public AgressiveEnemyWave agressiveEnemyWave;
    // public List<WaveBasePrefabMapping> wavePrefabs; // Example: struct WaveBasePrefabMapping { WaveType type; Wave prefab; }

    [Header("Continuous Wave Cycle Settings")]
public List<WaveType> availableWaveTypes = new List<WaveType> { WaveType.NormalEnemyWave };
    public List<int> availableDifficulties = new List<int> { 0, 1, 2 }; // Default difficulties
    public SpawnType continuousWaveSpawnType = SpawnType.AreaAroundPosition;
    public float delayBetweenWaves = 5.0f;

    [Header("Default Wave Parameters (for manual starts if not overridden)")]
    public int defaultDifficulty = 1;
    public float defaultSpawnRate = 1f;

    [Header("Global References")]
    public Transform[] players;              // List of all players for distance checking.
    public Transform[] fixedSpawnPoints;     // For Fixed spawn mode.
    public Transform[] objectivePositions;   // For AreaAroundPosition spawn mode (can be used if continuousWaveTarget is not set).
    public GameObject enemyParent;

    // A list to track all running wave coroutines (includes manually started and the active continuous wave).
    private List<Coroutine> runningWaves = new List<Coroutine>();
    private Coroutine continuousWaveCoroutine;
    private Coroutine activeContinuousWaveInstance; // The specific wave instance started by the continuous cycle
    private Transform currentContinuousWaveTarget;
    
    [Header("Progression References")]
    [Tooltip("Optional: Assign a LootTableProgressionManager to update loot tables after waves.")]
    public LootTableProgressionManager lootProgressionManager;
    [SerializeField] bool lootProgressionIsPerCycle = true; // If true, resets the wave count when starting continuous waves

    [Tooltip("Optional: Assign a WaveProgressionManager to control the sequence of wave types and difficulties.")]
    public WaveProgressionManager waveProgressionManager;
    [SerializeField] bool waveProgressionIsPerCycle = true;

    /// <summary>
    /// Starts a wave of the given type and tracks its coroutine.
    /// Sets the wave difficulty to the passed int.
    /// </summary>
    /// <returns>The Coroutine handle for the started wave.</returns>
    public Coroutine StartWave(WaveType waveType, SpawnType spawnType, int difficulty, float spawnRateToUse, Transform[] positions)
    {
        Wave waveInstance = null;

        switch (waveType)
        {
            case WaveType.NormalEnemyWave:
                waveInstance = Instantiate(normalEnemyWave, transform);
                break;
            case WaveType.RangedEnemyWave:
                waveInstance = Instantiate(rangedEnemyWave, transform);
                break;
            case WaveType.AgressiveEnemyWave:
                waveInstance = Instantiate(agressiveEnemyWave, transform);
                break;
            default:
                Debug.LogError("Wave type " + waveType + " not supported or prefab not assigned.");
                return null;
        }

        if (waveInstance == null)
        {
            Debug.LogError($"Failed to instantiate wave for type {waveType}. Check prefab assignment.");
            return null;
        }


        
        waveInstance.enemyParent = enemyParent;
        waveInstance.difficulty = difficulty;
        waveInstance.spawnRate = spawnRateToUse;
        waveInstance.players = players;
        waveInstance.positions_tmp = positions;

        // --- TYPE-SPECIFIC SETUP ---
        if (waveInstance is NormalEnemyWave normalEnemyWaveInstance)
        {
            normalEnemyWaveInstance.fixedSpawnPoints = fixedSpawnPoints;
            normalEnemyWaveInstance.objectivePositions = objectivePositions;
            normalEnemyWaveInstance.spawnType = spawnType;
        }
        else if (waveInstance is RangedEnemyWave rangedEnemyWaveInstance)
        {
            rangedEnemyWaveInstance.fixedSpawnPoints = fixedSpawnPoints;
            rangedEnemyWaveInstance.objectivePositions = objectivePositions;
            rangedEnemyWaveInstance.spawnType = spawnType;
        }
        else if (waveInstance is AgressiveEnemyWave agressiveEnemyWaveInstance)
        {
            agressiveEnemyWaveInstance.fixedSpawnPoints = fixedSpawnPoints;
            agressiveEnemyWaveInstance.objectivePositions = objectivePositions;
            agressiveEnemyWaveInstance.spawnType = spawnType;
        }
        // ---END OF TYPE-SPECIFIC SETUP ---

        Debug.Log($"Starting Wave: {waveType}, Difficulty: {difficulty}, SpawnType: {spawnType}, SpawnRate: {spawnRateToUse}");
        Coroutine c = StartCoroutine(waveInstance.ExecuteWave());
        runningWaves.Add(c);
        return c;        
    }

    /// <summary>
    /// Starts a wave using default spawn rate.
    /// </summary>
    public Coroutine StartWave(WaveType waveType, SpawnType spawnType, int difficulty, Transform[] positions)
    {
        return StartWave(waveType, spawnType, difficulty, defaultSpawnRate, positions);
    }

    /// <summary>
    /// Starts a wave using default difficulty and spawn rate.
    /// </summary>
    public Coroutine StartWave(WaveType waveType, SpawnType spawnType, Transform[] positions)
    {
        return StartWave(waveType, spawnType, defaultDifficulty, defaultSpawnRate, positions);
    }

    /// <summary>
    /// Starts a wave using default difficulty and spawn rate for a single position.
    /// </summary>
    public Coroutine StartWave(WaveType waveType, SpawnType spawnType, Transform position)
    {
        return StartWave(waveType, spawnType, defaultDifficulty, defaultSpawnRate, new Transform[] { position });
    }


    public void StartWaveWithDelay(WaveType waveType, SpawnType spawnType, int difficulty, float spawnRateToUse, Transform[] positions, float delay)
    {
        StartCoroutine(DelayedWaveStart(waveType, spawnType, difficulty, spawnRateToUse, positions, delay));
    }
    public void StartWaveWithDelay(WaveType waveType, SpawnType spawnType, int difficulty, Transform[] positions, float delay)
    {
        StartCoroutine(DelayedWaveStart(waveType, spawnType, difficulty, defaultSpawnRate, positions, delay));
    }
    public void StartWaveWithDelay(WaveType waveType, SpawnType spawnType, int difficulty, Transform position, float delay)
    {
        StartCoroutine(DelayedWaveStart(waveType, spawnType, difficulty, defaultSpawnRate, new Transform[] { position }, delay));
    }

    private IEnumerator DelayedWaveStart(WaveType waveType, SpawnType spawnType, int difficulty, float spawnRateToUse, Transform[] positions, float delay)
    {
        yield return new WaitForSeconds(delay);
        Coroutine waveCoroutine = StartWave(waveType, spawnType, difficulty, spawnRateToUse, positions);
        if (waveCoroutine != null)
        {
            StartCoroutine(RemoveCoroutineWhenFinished(waveCoroutine));
        }
    }

    /// <summary>
    /// Starts the continuous cycle of randomly selected waves.
    /// </summary>
    public void StartContinuousWaves(Transform spawnTarget, Wrapper<bool> refrenceFlag)
    {
        if (availableWaveTypes == null || availableWaveTypes.Count == 0)
        {
            Debug.LogError("Cannot start continuous waves: availableWaveTypes list is empty or null.");
            return;
        }
        if (availableDifficulties == null || availableDifficulties.Count == 0)
        {
            Debug.LogError("Cannot start continuous waves: availableDifficulties list is empty or null.");
            return;
        }

        currentContinuousWaveTarget = spawnTarget;
        if (currentContinuousWaveTarget == null && continuousWaveSpawnType != SpawnType.Fixed && continuousWaveSpawnType != SpawnType.AreaAroundPlayers)
        {
             Debug.LogWarning("Continuous wave target is null, and spawn type might require it. Using objectivePositions or fixedSpawnPoints as fallback if available.");
        }

        if (refrenceFlag == null)
        {
            Debug.Log("Reference flag must be set.");
        }   

        if (continuousWaveCoroutine != null)
        {
            StopCoroutine(continuousWaveCoroutine);
        }
        Debug.Log("Starting continuous wave cycle.");
        continuousWaveCoroutine = StartCoroutine(RunWaveCycle(refrenceFlag));
    }


    private IEnumerator RunWaveCycle(Wrapper<bool> refrenceFlag)
    {
        if (refrenceFlag == null)
        {
            Debug.LogError("RunWaveCycle: refrenceFlag is null. Stopping cycle.");
            yield break;
        }

        if (!refrenceFlag.value)
        {
            Debug.Log("Continuous wave cycle stopped by reference flag.");
            yield break; // Exit the cycle if the flag is false
        }

        // Optionals: Reset progression systems if configured
        if (lootProgressionIsPerCycle && lootProgressionManager != null)
        {
            lootProgressionManager.ResetWaveCount();
        }
        if (waveProgressionIsPerCycle && waveProgressionManager != null)
        {
            waveProgressionManager.ResetProgression();
        }

        WaveProgressionEntry entryForCurrentWave = default;
        LootTable lootTableToSetAfterThisWave = null;

        while (refrenceFlag.value)
        {
            WaveType selectedWaveType;
            int selectedDifficulty;

            // --- GET WAVE PARAMETERS FROM PROGRESSION MANAGER OR FALLBACK ---
            if (waveProgressionManager != null)
            {
                if (waveProgressionManager.TryGetNextWaveEntry(out entryForCurrentWave))
                {
                    selectedWaveType = entryForCurrentWave.waveType;
                    selectedDifficulty = entryForCurrentWave.difficulty;
                    lootTableToSetAfterThisWave = entryForCurrentWave.lootTableForNextPeriod;
                }
                else
                {
                    // Progression manager indicated end of sequence and not looping/repeating
                    Debug.Log("RunWaveCycle: Wave progression finished. Stopping continuous waves.");
                    refrenceFlag.value = false; // Signal to stop the cycle
                    break; // Exit while loop
                }
            }
            else
            {
                if (availableWaveTypes.Count <= 0) { /* error */ yield break; }
                selectedWaveType = availableWaveTypes[Random.Range(0, availableWaveTypes.Count)];
                selectedDifficulty = availableDifficulties[Random.Range(0, availableDifficulties.Count)];
                lootTableToSetAfterThisWave = null;
            }
            // --- END OF GETTING WAVE PARAMETERS ---

            Transform[] positionsToUse;

            switch (continuousWaveSpawnType)
            {
                case SpawnType.AreaAroundPosition:
                    if (currentContinuousWaveTarget != null)
                        positionsToUse = new Transform[] { currentContinuousWaveTarget };
                    else if (objectivePositions != null && objectivePositions.Length > 0)
                        positionsToUse = objectivePositions; // Fallback
                    else
                    {
                        Debug.LogError("Cannot start AreaAroundPosition wave for continuous cycle: No target or objectivePositions defined.");
                        yield return new WaitForSeconds(delayBetweenWaves); // Wait and try next cycle
                        continue;
                    }
                    break;
                case SpawnType.Fixed:
                    positionsToUse = fixedSpawnPoints; // Uses fixedSpawnPoints directly from WaveManager
                    break;
                case SpawnType.AreaAroundPlayers:
                    positionsToUse = players; // Uses players directly
                    break;
                default:
                    Debug.LogError($"Unsupported continuousWaveSpawnType: {continuousWaveSpawnType}");
                    yield return new WaitForSeconds(delayBetweenWaves); // Wait and try next cycle
                    continue;
            }

            Debug.Log($"Continuous Cycle: Starting new wave - Type: {selectedWaveType}, Difficulty: {selectedDifficulty}, SpawnType: {continuousWaveSpawnType}");
            activeContinuousWaveInstance = StartWave(selectedWaveType, continuousWaveSpawnType, selectedDifficulty, defaultSpawnRate, positionsToUse);

            if (activeContinuousWaveInstance == null)
            {
                Debug.LogWarning("Continuous Cycle: Failed to start a wave instance. Will retry after delay.");
            }
            else
            {
                yield return activeContinuousWaveInstance; // Wait for the wave to complete

                if (runningWaves.Contains(activeContinuousWaveInstance))
                {
                    runningWaves.Remove(activeContinuousWaveInstance);
                }

                // -- THIS IS WHERE THE LOOT TABLES ARE UPDATED --
                Debug.Log("Continuous Cycle: Wave finished.");
                if (lootTableToSetAfterThisWave != null && LootManager.Instance != null)
                {
                    LootManager.Instance.UpdateLootTable(lootTableToSetAfterThisWave);
                }
                else
                {
                    Debug.LogWarning("WaveManager: LootProgressionManager not assigned. Loot tables will not be updated automatically after waves.");
                }
                // -- END OF LOOT TABLES UPDATE --
            }

            activeContinuousWaveInstance = null; // Clear the active instance

            if (!refrenceFlag.value)
            {
                Debug.Log("RunWaveCycle: refrenceFlag became false during wave execution or before delay. Exiting loop.");
                break; // Exit while loop
            }

            Debug.Log("Continuous Cycle: Wave finished. Waiting for next wave...");
            yield return new WaitForSeconds(delayBetweenWaves);
        }
        
        Debug.Log("RunWaveCycle: Loop has exited.");
        continuousWaveCoroutine = null;
    }

    public void StartBossWave()
    {
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            Debug.Log("Starting boss wave.");
            GameObject bossInstance = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            if (enemyParent != null)
            {
                bossInstance.transform.SetParent(enemyParent.transform, true);
            }
        }
        else
        {
            Debug.LogError("Boss prefab or spawn point is not assigned.");
        }      
    }

    /// <summary>
    /// Stops the continuous wave cycle and the currently active wave from that cycle.
    /// </summary>
    public void StopContinuousWaves()
    {
        Debug.Log("Stopping continuous wave cycle.");
        if (continuousWaveCoroutine != null)
        {
            StopCoroutine(continuousWaveCoroutine);
            continuousWaveCoroutine = null;
        }
        if (activeContinuousWaveInstance != null)
        {
            StopCoroutine(activeContinuousWaveInstance);
            runningWaves.Remove(activeContinuousWaveInstance); // Ensure it's removed if it was added
            activeContinuousWaveInstance = null;
        }
    }
    
    /// <summary>
    /// Stops all currently running wave coroutines, including the continuous cycle.
    /// </summary>
    public void EndAllWaves()
    {
        Debug.Log("Ending all waves.");
        StopContinuousWaves(); // Stop the cycle and its active wave first

        // Stop any other manually started waves
        foreach (Coroutine waveCoroutine in new List<Coroutine>(runningWaves)) // Iterate over a copy
        {
            if (waveCoroutine != null) // Check if it wasn't already stopped (e.g. activeContinuousWaveInstance)
            {
                StopCoroutine(waveCoroutine);
            }
        }
        runningWaves.Clear();

        // Also destroy any instantiated wave GameObjects
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Wave>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    // Helper coroutine to remove a wave from runningWaves list once it's finished
    private IEnumerator RemoveCoroutineWhenFinished(Coroutine coroutine)
    {
        yield return coroutine; // Wait for the coroutine to complete
        if (runningWaves.Contains(coroutine))
        {
            runningWaves.Remove(coroutine);
            // Debug.Log($"Coroutine (hash: {coroutine.GetHashCode()}) finished naturally and removed from runningWaves.");
        }
    }
}