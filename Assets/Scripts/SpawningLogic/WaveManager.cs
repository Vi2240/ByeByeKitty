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

    [Header("Wave Settings")]
    public WaveType0 waveType0Prefab;  // Prefab with the WaveType0 component.
    // TODO: Consider a more generic way to handle prefabs if more WaveTypes are added
    // public List<WaveBasePrefabMapping> wavePrefabs; // Example: struct WaveBasePrefabMapping { WaveType type; Wave prefab; }

    [Header("Continuous Wave Cycle Settings")]
    public List<WaveType> availableWaveTypes = new List<WaveType> { WaveType.WaveType0 }; // Default with WaveType0
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
            case WaveType.WaveType0:
                waveInstance = Instantiate(waveType0Prefab, transform);
                break;
            // Extend with other wave types as needed.
            // case WaveType.WaveType1:
            //     waveInstance = Instantiate(waveType1Prefab, transform);
            //     break;
            default:
                Debug.LogError("Wave type " + waveType + " not supported or prefab not assigned.");
                return null;
        }

        if (waveInstance != null)
        {
            waveInstance.enemyParent = enemyParent;
            waveInstance.difficulty = difficulty;
            waveInstance.spawnRate = spawnRateToUse;
            waveInstance.players = players;
            // Specific setup for WaveType0 if needed, or rely on Wave base class for these
            if (waveInstance is WaveType0 waveType0Instance)
            {
                waveType0Instance.fixedSpawnPoints = fixedSpawnPoints;
                waveType0Instance.objectivePositions = objectivePositions; // These might be redundant if GetValidSpawnPosition primarily uses positions_tmp
            }
            waveInstance.positions_tmp = positions;

            // Set the desired spawn type (Fixed, AreaAroundPosition, or AreaAroundPlayers).
            // This assumes the Wave script itself has a public spawnType field that its ExecuteWave uses.
            // If WaveType0.cs is the only one with 'spawnType', this needs adjustment or ensure Wave.cs has it.
            // For now, assuming WaveType0 specifically handles its spawnType.
            // Let's ensure the specific wave instance (like WaveType0) has its spawnType set if it's a field on it.
            if (waveInstance is WaveType0 specificWave) // Example if WaveType0 has its own spawnType field
            {
                specificWave.spawnType = spawnType;
            }
            // Else, if spawnType is a general property on the abstract Wave class that GetValidSpawnPosition uses, it's more complex.
            // Given WaveType0.cs: public SpawnType spawnType = SpawnType.AreaAroundPlayers;
            // We should set this:
            // waveInstance.spawnType = spawnType; // This line would require 'spawnType' to be a field on the 'Wave' abstract class or all its children.
            // For WaveType0, it has its own public field:
            if (waveInstance is WaveType0 wt0) wt0.spawnType = spawnType;


            Debug.Log($"Starting Wave: {waveType}, Difficulty: {difficulty}, SpawnType: {spawnType}, SpawnRate: {spawnRateToUse}");
            Coroutine c = StartCoroutine(waveInstance.ExecuteWave());
            runningWaves.Add(c);
            return c;
        }
        return null;
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

        while (refrenceFlag.value)
        {
            WaveType selectedWaveType;
            int selectedDifficulty;

            // --- GET WAVE PARAMETERS FROM PROGRESSION MANAGER OR FALLBACK ---
            if (waveProgressionManager != null)
            {
                if (waveProgressionManager.GetNextWaveParameters(out selectedWaveType, out selectedDifficulty))
                {
                    // Parameters successfully retrieved from progression manager
                }
                else
                {
                    // Progression manager indicated end of sequence and not looping/repeating
                    Debug.Log("RunWaveCycle: Wave progression finished. Stopping continuous waves.");
                    refrenceFlag.value = false; // Signal to stop the cycle
                    break; // Exit while loop
                }
            }
            else // Fallback to random selection if no progression manager is assigned
            {
                Debug.LogWarning("RunWaveCycle: WaveProgressionManager not assigned. Falling back to random wave selection.");
                if (availableWaveTypes.Count == 0 || availableDifficulties.Count == 0)
                {
                    Debug.LogError("Wave cycle cannot continue: available wave types or difficulties are empty for random selection.");
                    yield break;
                }
                selectedWaveType = availableWaveTypes[Random.Range(0, availableWaveTypes.Count)];
                selectedDifficulty = availableDifficulties[Random.Range(0, availableDifficulties.Count)];
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

            if (activeContinuousWaveInstance != null)
            {
                yield return activeContinuousWaveInstance; // Wait for the wave to complete

                if (runningWaves.Contains(activeContinuousWaveInstance))
                {
                    runningWaves.Remove(activeContinuousWaveInstance);
                }

                // -- THIS IS WHERE THE LOOT TABLES ARE UPDATED --
                Debug.Log("Continuous Cycle: Wave finished.");
                if (lootProgressionManager != null)
                {
                    lootProgressionManager.OnContinuousWaveCompleted();
                    // The OnContinuousWaveCompleted will internally call LootManager.Instance.UpdateLootTable
                }
                else
                {
                    Debug.LogWarning("WaveManager: LootProgressionManager not assigned. Loot tables will not be updated automatically after waves.");
                }
                // -- END OF LOOT TABLES UPDATE --
            }
            else
            {
                Debug.LogWarning("Continuous Cycle: Failed to start a wave instance. Will retry after delay.");
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