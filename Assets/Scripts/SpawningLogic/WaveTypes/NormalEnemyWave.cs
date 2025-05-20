using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A concrete wave implementation (WaveType2) that spawns enemies based on difficulty and probabilities.
/// </summary>
public class NormalEnemyWave : Wave // Ensure Wave.cs exists and is the base class
{
    const String waveTypeName = "NormalEnemyWave";

    [Header("Reference Points for Spawning (assigned by WaveManager or defaults)")]
    public Transform[] fixedSpawnPoints;    // For Fixed mode.
    public Transform[] objectivePositions;  // For AreaAroundPosition mode if positions_tmp not set by WaveManager.

    // Define which spawn type this wave uses. This will be set by WaveManager.
    public SpawnType spawnType = SpawnType.AreaAroundPlayers; // Default, but WaveManager should override

    public override IEnumerator ExecuteWave()
    {
        Debug.Log($"Executing {waveTypeName}: Difficulty {difficulty}, SpawnRate {spawnRate}, SpawnType {spawnType}");
        // Adjust wait time based on spawnRate and difficulty.
        // Ensure spawnRate is not zero to avoid division by zero.
        //float waitTime = (difficulty + 1 > 0 && spawnRate > 0) ? spawnRate / (difficulty + 1) : 1f;
        float waitTime = 10;
        if (spawnRate <= 0)
        {
            Debug.LogWarning("SpawnRate is zero or negative, defaulting waitTime to 1 second.");
            waitTime = 1f;
        }


        // Configure enemy spawn chances based on difficulty.
        int normalEnemyChance, meleeEnemyChance, slowEnemyChance, rangeEnemyChance;
        switch (difficulty)
        {
            case 0: // Easy
                normalEnemyChance = 10;
                meleeEnemyChance = 3;
                slowEnemyChance = 5;
                rangeEnemyChance = 0;
                break;
            case 1: // Medium
                normalEnemyChance = 25;
                meleeEnemyChance = 8;
                slowEnemyChance = 15;
                rangeEnemyChance = 0;
                break;
            case 2: // Hard
                normalEnemyChance = 40;
                meleeEnemyChance = 12;
                slowEnemyChance = 20;
                rangeEnemyChance = 0;
                break;
            default: // Very Hard / Scaled
                normalEnemyChance = 55  + (difficulty - 3) * 15; // Example scaling for difficulties > 2
                meleeEnemyChance = 16   + (difficulty - 3) * 3;
                slowEnemyChance = 25    + (difficulty - 3) * 5;
                rangeEnemyChance = 0    + (difficulty - 3) * 5;
                break;
        }
        int totalEnemiesToSpawnThisWave = normalEnemyChance + meleeEnemyChance + slowEnemyChance + rangeEnemyChance;
        int enemiesSpawnedThisWave = 0;

        Debug.Log($"{waveTypeName}: Total enemies to spawn this wave: {totalEnemiesToSpawnThisWave}");

        // Main spawning loop.
        while (enemiesSpawnedThisWave < totalEnemiesToSpawnThisWave)
        {
            // Calculate how many enemies to spawn this loop iteration.
            int spawnsPerLoop = Mathf.Max(1, 5 + 5 * difficulty); // Ensure at least 1 spawn if totalEnemies > 0
            spawnsPerLoop = Mathf.Min(spawnsPerLoop, totalEnemiesToSpawnThisWave - enemiesSpawnedThisWave); // Don't overshoot

            for (int i = 0; i < spawnsPerLoop; i++)
            {
                if (enemiesSpawnedThisWave >= totalEnemiesToSpawnThisWave) break; // Safety break

                int remainingNormal = normalEnemyChance;
                int remainingMelee = meleeEnemyChance;
                int remainingSlow = slowEnemyChance;
                int remainingRange = rangeEnemyChance;

                // This logic needs to pick from available counts, not decrement global counts for choice
                List<GameObject> availableEnemyTypesForSpawn = new List<GameObject>();
                if (normalEnemyChance > 0 && normalEnemy != null) availableEnemyTypesForSpawn.Add(normalEnemy);
                if (slowEnemyChance > 0 && slowEnemy != null) availableEnemyTypesForSpawn.Add(slowEnemy);
                // Add speed and range if they were to be used and had prefabs
                // if (speedEnemyChance > 0 && speedEnemy != null) availableEnemyTypesForSpawn.Add(speedEnemy);
                // if (rangeEnemyChance > 0 && rangeEnemy != null) availableEnemyTypesForSpawn.Add(rangeEnemy);

                GameObject enemyToSpawn = null;

                if (availableEnemyTypesForSpawn.Count == 0) {
                    Debug.LogWarning("No enemy types available to spawn or counts depleted.");
                    enemiesSpawnedThisWave++; // Count as spawned to prevent infinite loop if misconfigured
                    continue;
                }

                // Simplified random choice from those still available based on remaining counts
                // This is a bit naive, a weighted random would be better if complex ratios are desired
                // For now, just ensuring we spawn something if counts are > 0
                int choiceIndex = UnityEngine.Random.Range(0, availableEnemyTypesForSpawn.Count);
                enemyToSpawn = availableEnemyTypesForSpawn[choiceIndex];
                
                // Decrement the count for the chosen type
                if (enemyToSpawn == normalEnemy) normalEnemyChance--;
                else if (enemyToSpawn == slowEnemy) slowEnemyChance--;
                // else if (enemyToSpawn == speedEnemy) speedEnemyChance--;
                // else if (enemyToSpawn == rangeEnemy) rangeEnemyChance--;


                // Determine the spawn position based on the selected spawn type.
                Vector3 spawnPos = Vector3.zero;
                switch (spawnType)
                {
                    case SpawnType.Fixed:
                        // fixedSpawnPoints should be set by WaveManager or be available on this prefab
                        spawnPos = GetValidSpawnPosition(SpawnType.Fixed, this.fixedSpawnPoints);
                        break;
                    case SpawnType.AreaAroundPosition:
                        // positions_tmp is set by WaveManager
                        spawnPos = GetValidSpawnPosition(SpawnType.AreaAroundPosition, positions_tmp, null, null, null);
                        break;
                    case SpawnType.AreaAroundPlayers:
                    default:
                        // players is set by WaveManager
                        spawnPos = GetValidSpawnPosition(SpawnType.AreaAroundPlayers); // players array is used internally by this method
                        break;
                }

                if (enemyToSpawn != null) {
                    GameObject enemyInstance = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                    if (enemyParent != null)
                    {
                        enemyInstance.transform.SetParent(enemyParent.transform, true);
                    }
                    enemiesSpawnedThisWave++;
                } else {
                     Debug.LogWarning("EnemyToSpawn was null, but a choice should have been made. Check spawn logic / counts.");
                     // To prevent potential infinite loop if logic error above, count as spawned or break.
                     // For now, if enemyToSpawn is null, it means counts were 0 or prefabs missing.
                     // The loop condition `enemiesSpawnedThisWave < totalEnemiesToSpawnThisWave` should handle this.
                }
            }
            if (enemiesSpawnedThisWave < totalEnemiesToSpawnThisWave) // Only wait if more enemies are to come
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
        Debug.Log($"{waveTypeName} finished spawning {enemiesSpawnedThisWave} enemies.");
    }
}