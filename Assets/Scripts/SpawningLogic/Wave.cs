using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for a wave.
/// </summary>
public abstract class Wave : MonoBehaviour
{
    public GameObject enemyParent;
    public int difficulty = 1;       // e.g., 0, 1, 2, etc.
    public float spawnRate = 3;      // How frequently enemies spawn.
    public Transform[] players;      // List of all players (used for distance validation).
    public Transform[] positions_tmp;// Temporary variable for setting a position 

    // The radius used when spawning enemies around a reference point.
    public float spawnRadius = 20f;
    // Minimum allowed distance from any player.
    public float minDistance = 3f;

    /// <summary>
    /// Every concrete wave must implement its execution logic.
    /// </summary>
    public abstract IEnumerator ExecuteWave();

    /// <summary>
    /// Gets a valid spawn position based on the provided spawn type and an array of reference points.
    /// For Fixed, it simply returns one of the fixed positions without checking distance.
    /// For AreaAroundPosition, it returns a random point within a circle around a randomly chosen reference point.
    /// For AreaAroundPlayers, it does the same but uses the players list.
    /// </summary>
    /// <param name="spawnType">The spawn mode to use.</param>
    /// <param name="referencePoints">An array of reference points (for Fixed or AreaAroundPosition).</param>
    /// <returns>A valid spawn position.</returns>
    protected Vector3 GetValidSpawnPosition(SpawnType spawnType, Transform[] referencePoints = null)
    {
        // For Fixed spawn type, directly return a random fixed position without validation.
        if (spawnType == SpawnType.Fixed && referencePoints != null && referencePoints.Length > 0)
        {
            return referencePoints[Random.Range(0, referencePoints.Length)].position;
        }
        
        // Spawn area type, a bit more complicated logic
        Vector3 spawnPos = Vector3.zero;
        int attempts = 10;
        while (attempts-- > 0)
        {
            switch (spawnType)
            {
                case SpawnType.AreaAroundPosition:
                    // Choose a random reference point and then add a random offset.
                    if (referencePoints != null && referencePoints.Length > 0)
                    {
                        Transform refPoint = referencePoints[Random.Range(0, referencePoints.Length)];
                        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                        spawnPos = new Vector3(refPoint.position.x + randomOffset.x,
                                               refPoint.position.y,
                                               refPoint.position.z + randomOffset.y);
                    }
                    break;

                case SpawnType.AreaAroundPlayers:
                    // Choose a random player from the players list and then add a random offset.
                    if (players != null && players.Length > 0)
                    {
                        Transform player = players[Random.Range(0, players.Length)];
                        Vector2 offset = Random.insideUnitCircle * spawnRadius;
                        spawnPos = new Vector3(player.position.x + offset.x,
                                               player.position.y,
                                               player.position.z + offset.y);
                    }
                    break;
                default:
                    // Default to fixed spawn type if edge case is reatched.
                    if (referencePoints != null && referencePoints.Length > 0)
                        spawnPos = referencePoints[Random.Range(0, referencePoints.Length)].position;
                    break;
            }

            // For non-Fixed types, validate the spawn position.
            if (spawnType == SpawnType.Fixed || IsSpawnValid(spawnPos))
                break;
        }
        return spawnPos; // Returns the randomized spawnPos, if all iterations failed it'll spawn the latest attempt anyway.
    }

    /// <summary>
    /// Checks that the provided position is not too close to any player.
    /// </summary>
    /// <param name="pos">The potential spawn position.</param>
    /// <returns>True if valid, otherwise false.</returns>
    protected bool IsSpawnValid(Vector3 pos)
    {
        foreach (Transform player in players)
        {
            if (Vector3.Distance(pos, player.position) < minDistance)
                return false;
        }
        return true;
    }
}