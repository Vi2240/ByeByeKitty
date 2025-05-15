// Wave.cs (Abstract base class) - Minor changes if any, ensure it's compatible.

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
    public float spawnRate = 3;       // How frequently enemies spawn (interpreted by concrete wave).
    public Transform[] players;       // List of all players (used for distance validation).
    public Transform[] positions_tmp; // Temporary variable for setting a position array by WaveManager

    // The radius used when spawning enemies around a reference point.
    public float? spawnRadius = 20f;
    // Minimum allowed distance from any player.
    public float? minDistanceFromPlayer = 3f;
    // Minimum allowed distance from point.
    public float? minDistanceFromPoint = 7.5f;

    /// <summary>
    /// Every concrete wave must implement its execution logic.
    /// This coroutine should yield until the wave is considered complete.
    /// </summary>
    public abstract IEnumerator ExecuteWave();

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
        if (currentMinDistanceFromPoint > 0 && currentSpawnRadius > 0 && currentMinDistanceFromPoint >= currentSpawnRadius) {
             currentSpawnRadius = currentMinDistanceFromPoint + 1f; // Ensure radius is slightly larger
        }


        Transform[] effectiveReferencePoints = referencePoints;
        if (spawnTypeToUse == SpawnType.AreaAroundPlayers) {
            effectiveReferencePoints = this.players; // AreaAroundPlayers always uses the 'players' array
        } else if (effectiveReferencePoints == null || effectiveReferencePoints.Length == 0) {
            effectiveReferencePoints = this.positions_tmp; // Fallback for Fixed/AreaAroundPosition if specific not given
        }


        if (effectiveReferencePoints == null || effectiveReferencePoints.Length == 0) {
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

        for(int i=0; i < attempts; i++)
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
                    if (actualMinDistance >= currentSpawnRadius && currentSpawnRadius > 0) { // If min dist is too large for radius
                        actualMinDistance = currentSpawnRadius * 0.5f; // Adjust to be within radius
                         Debug.LogWarning($"MinDistanceFromPoint ({currentMinDistanceFromPoint}) was >= SpawnRadius ({currentSpawnRadius}). Adjusted for random range.");
                    } else if (currentSpawnRadius <=0) { // If radius is zero or negative, just use the point
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

            if (IsSpawnValid(spawnPos, currentMinDistanceFromPlayer)) {
                foundValid = true;
                break;
            }
        }

        if (!foundValid) {
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

        if (players == null || players.Length == 0) {
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
}