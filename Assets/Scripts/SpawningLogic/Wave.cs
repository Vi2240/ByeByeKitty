// Wave.cs (Abstract base class) - Minor changes if any, ensure it's compatible.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for a wave.
/// </summary>
public abstract class Wave : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject normalEnemy;
    public GameObject meleeEnemy;
    public GameObject slowEnemy;
    public GameObject rangeEnemy;
    public GameObject bossEnemy;

    public GameObject enemyParent;
    public int difficulty = 1;        // e.g., 0, 1, 2, etc.
    public float spawnRate = 3;       // How frequently enemies spawn (interpreted by concrete wave).
    public Transform[] players;       // List of all players (used for distance validation).
    public Transform[] positions_tmp; // Temporary variable for setting a position array by WaveManager

    // The radius used when spawning enemies around a reference point.
    public float? spawnRadius = 20f;
    // Minimum allowed distance from any player.
    public float? minDistanceFromPlayer = 3f;
    // Minimum allowed distance from point.
    public float? minDistanceFromPoint = 7.5f;

    protected struct EnemyComposition
    {
        public int normalCount;
        public int meleeCount;
        public int slowCount;
        public int rangeCount;
    }

    protected abstract EnemyComposition GetEnemyCompositionForDifficulty(int currentDifficulty);

    /// <summary>
    /// Every concrete wave must implement its execution logic.
    /// This coroutine should yield until the wave is considered complete.
    /// </summary>
    public abstract IEnumerator ExecuteWave();

    protected virtual IEnumerator SpawnEnemyLoop(string waveTypeName, SpawnType effectiveSpawnType, Transform[] fixedSpawns, Transform[] objectiveSpawnsForArea) // Added parameters for spawn points
    {
        // Get the composition from the child class
        EnemyComposition composition = GetEnemyCompositionForDifficulty(this.difficulty);

        int normalEnemyChance = composition.normalCount; // Use local mutable copies for spawning
        int meleeEnemyChance = composition.meleeCount;
        int slowEnemyChance = composition.slowCount;
        int rangeEnemyChance = composition.rangeCount;

        int totalEnemiesToSpawnThisWave = normalEnemyChance + meleeEnemyChance + slowEnemyChance + rangeEnemyChance;
        int enemiesSpawnedThisWave = 0;

        if (totalEnemiesToSpawnThisWave == 0) {
            Debug.LogWarning($"{waveTypeName}: No enemies defined for difficulty {this.difficulty}. Wave will complete immediately.");
            yield break; // Exit if no enemies to spawn
        }

        Debug.Log($"{waveTypeName}: Total enemies to spawn this wave: {totalEnemiesToSpawnThisWave} for difficulty {this.difficulty}");

        // Determine waitTime (you had this logic, keep it or adjust)
        float waitTime = 10f; // Default or from spawnRate
        if (this.spawnRate <= 0)
        {
            Debug.LogWarning("SpawnRate is zero or negative, defaulting waitTime to 1 second.");
            waitTime = 1f;
        }
        // else if (this.difficulty + 1 > 0 && this.spawnRate > 0) {
        //     waitTime = this.spawnRate / (this.difficulty + 1);
        // }


        while (enemiesSpawnedThisWave < totalEnemiesToSpawnThisWave)
        {
            int spawnsPerLoop = Mathf.Max(1, 5 + 5 * this.difficulty);
            spawnsPerLoop = Mathf.Min(spawnsPerLoop, totalEnemiesToSpawnThisWave - enemiesSpawnedThisWave);

            for (int i = 0; i < spawnsPerLoop; i++)
            {
                if (enemiesSpawnedThisWave >= totalEnemiesToSpawnThisWave) break;

                List<GameObject> availableEnemyTypesForSpawn = new List<GameObject>();
                if (normalEnemyChance > 0 && normalEnemy != null) availableEnemyTypesForSpawn.Add(normalEnemy);
                if (meleeEnemyChance > 0 && meleeEnemy != null) availableEnemyTypesForSpawn.Add(meleeEnemy);
                if (slowEnemyChance > 0 && slowEnemy != null) availableEnemyTypesForSpawn.Add(slowEnemy);
                if (rangeEnemyChance > 0 && rangeEnemy != null) availableEnemyTypesForSpawn.Add(rangeEnemy);

                if (availableEnemyTypesForSpawn.Count == 0)
                {
                    // This should ideally not happen if totalEnemiesToSpawnThisWave > 0
                    // and initial chances are positive. It means we ran out of specific types.
                    // To prevent infinite loops, we might need to break or adjust logic.
                    // For now, count as spawned to ensure loop termination.
                    Debug.LogWarning($"{waveTypeName}: Ran out of specific enemy types to spawn, but more were expected. Check composition logic.");
                    enemiesSpawnedThisWave++;
                    continue;
                }

                int choiceIndex = Random.Range(0, availableEnemyTypesForSpawn.Count);
                GameObject enemyToSpawn = availableEnemyTypesForSpawn[choiceIndex];

                if (enemyToSpawn == normalEnemy) normalEnemyChance--;
                else if (enemyToSpawn == meleeEnemy) meleeEnemyChance--;
                else if (enemyToSpawn == slowEnemy) slowEnemyChance--;
                else if (enemyToSpawn == rangeEnemy) rangeEnemyChance--;

                Vector3 spawnPos = Vector3.zero;
                // Use the 'effectiveSpawnType' passed to this method.
                // Also, pass the correct spawn point arrays.
                switch (effectiveSpawnType)
                {
                    case SpawnType.Fixed:
                        spawnPos = GetValidSpawnPosition(SpawnType.Fixed, fixedSpawns);
                        break;
                    case SpawnType.AreaAroundPosition:
                        // 'positions_tmp' is the general one from WaveManager,
                        // but we passed 'objectiveSpawnsForArea' which might be specific
                        spawnPos = GetValidSpawnPosition(SpawnType.AreaAroundPosition, objectiveSpawnsForArea ?? this.positions_tmp);
                        break;
                    case SpawnType.AreaAroundPlayers:
                    default:
                        spawnPos = GetValidSpawnPosition(SpawnType.AreaAroundPlayers, this.players); // 'this.players' is fine here
                        break;
                }

                if (enemyToSpawn != null)
                {
                    GameObject enemyInstance = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                    if (this.enemyParent != null)
                    {
                        enemyInstance.transform.SetParent(this.enemyParent.transform, true);
                    }
                    enemiesSpawnedThisWave++;
                }
            }
            if (enemiesSpawnedThisWave < totalEnemiesToSpawnThisWave)
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
        Debug.Log($"{waveTypeName} finished spawning {enemiesSpawnedThisWave} enemies.");
    }

    /// <summary>
    /// Gets a valid spawn position based on the provided spawn type and an array of reference points.
    /// For Fixed, it simply returns one of the fixed positions without checking distance.
    /// For AreaAroundPosition, it returns a random point within a circle around a randomly chosen reference point.
    /// For AreaAroundPlayers, it does the same but uses the players list.
    /// </summary>
    /// <param name="spawnTypeToUse">The spawn mode to use.</param>
    /// <param name="referencePoints">An array of reference points (for Fixed or AreaAroundPosition). If null for AreaAroundPlayers, uses 'this.players'.</param>
    /// <param name="_minDistanceFromPoint">(Optional) Minimum distance from the reference point (for area spawning).</param>
    /// <param name="_minDistanceFromPlayer">(Optional) Minimum distance from players area spawning can spawn (if fails 10 times it spawns anyways).</param>
    /// <param name="_spawnRadius">(Optional) The radius for the cirle that confines the spawning area around a point.</param>
    /// <returns>A valid spawn position.</returns>
    protected Vector3 GetValidSpawnPosition(SpawnType spawnTypeToUse, Transform[] referencePoints = null, float? _minDistanceFromPoint = null, float? _minDistanceFromPlayer = null, float? _spawnRadius = null)
    {
        float currentMinDistanceFromPoint = _minDistanceFromPoint ?? this.minDistanceFromPoint ?? 0f;
        float currentMinDistanceFromPlayer = _minDistanceFromPlayer ?? this.minDistanceFromPlayer ?? 0f;
        float currentSpawnRadius = _spawnRadius ?? this.spawnRadius ?? 0f;

        currentMinDistanceFromPoint = Mathf.Abs(currentMinDistanceFromPoint);
        currentSpawnRadius = Mathf.Abs(currentSpawnRadius);

        // Ensure spawn radius is at least as large as min distance from point if both are positive
        if (currentMinDistanceFromPoint > 0 && currentSpawnRadius > 0 && currentMinDistanceFromPoint >= currentSpawnRadius)
        {
            currentSpawnRadius = currentMinDistanceFromPoint + 1f; // Ensure radius is slightly larger
        }


        Transform[] effectiveReferencePoints = referencePoints;
        if (spawnTypeToUse == SpawnType.AreaAroundPlayers)
        {
            effectiveReferencePoints = this.players; // AreaAroundPlayers always uses the 'players' array
        }
        else if (effectiveReferencePoints == null || effectiveReferencePoints.Length == 0)
        {
            effectiveReferencePoints = this.positions_tmp; // Fallback for Fixed/AreaAroundPosition if specific not given
        }


        if (effectiveReferencePoints == null || effectiveReferencePoints.Length == 0)
        {
            Debug.LogWarning($"GetValidSpawnPosition: No reference points available for spawn type {spawnTypeToUse}. Returning Vector3.zero.");
            return Vector3.zero;
        }

        if (spawnTypeToUse == SpawnType.Fixed)
        {
            return effectiveReferencePoints[Random.Range(0, effectiveReferencePoints.Length)].position;
        }

        Vector3 spawnPos = Vector3.zero;
        int attempts = 10; // Max attempts to find a valid position
        bool foundValid = false;

        for (int i = 0; i < attempts; i++)
        {
            Transform refPoint = effectiveReferencePoints[Random.Range(0, effectiveReferencePoints.Length)];
            if (refPoint == null) continue; // Skip if a reference point is null

            switch (spawnTypeToUse)
            {
                case SpawnType.AreaAroundPosition:
                case SpawnType.AreaAroundPlayers: // Both use similar circular area logic
                    Vector2 randomDirection = Random.insideUnitCircle.normalized;
                    // Ensure minDistance < spawnRadius for Random.Range to be valid
                    float actualMinDistance = (spawnTypeToUse == SpawnType.AreaAroundPosition) ? currentMinDistanceFromPoint : 0f; // Players don't need min distance from *themselves* as the center
                    if (actualMinDistance >= currentSpawnRadius && currentSpawnRadius > 0)
                    { // If min dist is too large for radius
                        actualMinDistance = currentSpawnRadius * 0.5f; // Adjust to be within radius
                        Debug.LogWarning($"MinDistanceFromPoint ({currentMinDistanceFromPoint}) was >= SpawnRadius ({currentSpawnRadius}). Adjusted for random range.");
                    }
                    else if (currentSpawnRadius <= 0)
                    { // If radius is zero or negative, just use the point
                        spawnPos = refPoint.position;
                        break;
                    }

                    float randomDistance = Random.Range(actualMinDistance, currentSpawnRadius);
                    Vector2 randomOffset = randomDirection * randomDistance;

                    spawnPos = new Vector3(refPoint.position.x + randomOffset.x,
                                           refPoint.position.y + randomOffset.y,
                                           refPoint.position.z); // Assuming 2D, keep Z
                    break;
                default: // Should have been caught by Fixed or earlier checks
                    Debug.LogError("GetValidSpawnPosition: Unexpected spawnType in loop: " + spawnTypeToUse);
                    return refPoint.position; // Failsafe
            }

            if (IsSpawnValid(spawnPos, currentMinDistanceFromPlayer))
            {
                foundValid = true;
                break;
            }
        }

        if (!foundValid)
        {
            //Debug.LogWarning($"Could not find a spawn position respecting minDistanceFromPlayer ({currentMinDistanceFromPlayer}u) after {attempts} attempts for spawn type {spawnTypeToUse}. Using last attempt.");
        }
        return spawnPos;
    }

    /// <summary>
    /// Checks that the provided position is not too close to any player.
    /// </summary>
    protected bool IsSpawnValid(Vector3 pos, float? minDistance = null)
    {
        float currentMinDistance = minDistance ?? this.minDistanceFromPlayer ?? 0f;
        if (currentMinDistance <= 0) return true; // No validation needed if distance is zero or negative

        if (players == null || players.Length == 0)
        {
            //Debug.LogWarning("IsSpawnValid: No players assigned to Wave, cannot validate distance. Assuming valid.");
            return true; // Or false, depending on desired behavior
        }

        foreach (Transform player in players)
        {
            if (player == null) continue;
            if (Vector3.Distance(pos, player.position) < currentMinDistance)
                return false;
        }
        return true;
    }
    
    public void SpawnBossEnemy(Vector3 spawnPos)
    {
        if (bossEnemy != null)
        {
            GameObject enemyInstance = Instantiate(bossEnemy, spawnPos, Quaternion.identity);
            if (enemyParent != null)
            {
                enemyInstance.transform.SetParent(enemyParent.transform, true);
            }
        }
        else
        {
            Debug.LogWarning("Boss enemy prefab is not assigned or is null.");
        }
    }
}