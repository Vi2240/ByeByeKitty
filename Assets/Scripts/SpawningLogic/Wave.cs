using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for a wave.
/// </summary>
public abstract class Wave : MonoBehaviour
{
    public GameObject enemyParent;
    public int difficulty = 1;        // e.g., 0, 1, 2, etc.
    public float spawnRate = 3;       // How frequently enemies spawn.
    public Transform[] players;       // List of all players (used for distance validation).
    public Transform[] positions_tmp; // Temporary variable for setting a position 

    // The radius used when spawning enemies around a reference point.
    public float? spawnRadius = 20f;
    // Minimum allowed distance from any player.
    public float? minDistanceFromPlayer = 3f;
    // Minimum allowed distance from point.
    public float? minDistanceFromPoint = 7.5f;

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
    /// <param name="_minDistanceFromPoint">(Optional) Minimum distance from the reference point (for area spawning).</param>
    /// <param name="_minDistanceFromPlayer">(Optional) Minimum distance from players area spawning can spawn (if fails 10 times it spawns anyways).</param>
    /// <param name="_spawnRadius">(Optional) The radius for the cirle that confines the spawning area around a point.</param>
    /// <returns>A valid spawn position.</returns>
    protected Vector3 GetValidSpawnPosition(SpawnType spawnType, Transform[] referencePoints = null, float? _minDistanceFromPoint = null, float? _minDistanceFromPlayer = null, float? _spawnRadius = null)
    {
        // Make sure minDistanceFrom Point and Player are set to a value in priority order of, input to function, this functions value, 0f.
        float minDistanceFromPoint    = _minDistanceFromPoint  ?? (float?)this.minDistanceFromPoint    ?? 0f;
        float minDistanceFromPlayer   = _minDistanceFromPlayer ?? (float?)this.minDistanceFromPlayer   ?? 0f;
        float spawnRadius             = _spawnRadius           ?? (float?)this.spawnRadius             ?? 0f;

        minDistanceFromPoint    = Mathf.Abs(minDistanceFromPoint);
        spawnRadius             = Mathf.Abs(spawnRadius);

        spawnRadius             = (minDistanceFromPoint >= spawnRadius) ? minDistanceFromPoint : spawnRadius;

        // If no reference points are provided, use the temporary positions.
        if (referencePoints == null || referencePoints.Length == 0)
            referencePoints = positions_tmp;

        // If no reference points are still provided, return Vector3.zero.
        if (referencePoints == null || referencePoints.Length == 0)
            return Vector3.zero;
    
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
                        
                        Vector2 randomDirection = Random.insideUnitCircle.normalized;
                        float randomDistance = Random.Range(minDistanceFromPoint, spawnRadius);
                        Vector2 randomOffset = randomDirection * randomDistance;

                        spawnPos = new Vector3(refPoint.position.x + randomOffset.x,
                                               refPoint.position.y + randomOffset.y,
                                               refPoint.position.z);
                    }
                    break;

                case SpawnType.AreaAroundPlayers:
                    // Choose a random player from the players list and then add a random offset.
                    if (players != null && players.Length > 0)
                    {
                        Transform player = players[Random.Range(0, players.Length)];
                        Vector2 offset = Random.insideUnitCircle * spawnRadius;
                        spawnPos = new Vector3(player.position.x + offset.x,
                                               player.position.y + offset.y,
                                               player.position.z);
                    }
                    break;
                default:
                    // Default to fixed spawn type if edge case is reatched.
                    if (referencePoints != null && referencePoints.Length > 0)
                        spawnPos = referencePoints[Random.Range(0, referencePoints.Length)].position;
                    break;
            }

            // For non-Fixed types, validate the spawn position.
            if (spawnType == SpawnType.Fixed || IsSpawnValid(spawnPos, minDistanceFromPlayer))
                break;
        }
        return spawnPos; // Returns the randomized spawnPos, if all iterations failed it'll spawn the latest attempt anyway.
    }

    /// <summary>
    /// Checks that the provided position is not too close to any player.
    /// </summary>
    /// <param name="pos">The potential spawn position.</param>
    /// <returns>True if valid, otherwise false.</returns>
    protected bool IsSpawnValid(Vector3 pos, float? minDistanceFromPlayer = null)
    {
        minDistanceFromPlayer = minDistanceFromPlayer ?? (float?)this.minDistanceFromPlayer ?? 0f;

        foreach (Transform player in players)
        {
            if (Vector3.Distance(pos, player.position) < minDistanceFromPlayer)
                return false;
        }
        return true;
    }
}