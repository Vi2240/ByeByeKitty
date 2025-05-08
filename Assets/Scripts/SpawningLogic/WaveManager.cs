using System.Collections.Generic;
using System.Collections;
using UnityEditor;

using UnityEngine;

/// <summary>
/// Manages waves and supports running multiple wave coroutines concurrently.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public WaveType0 waveType0Prefab;  // Prefab with the WaveType0 component.
    public int difficulty = 1;
    public float spawnRate = 1f;

    [Header("Global References")]
    public Transform[] players;              // List of all players for distance checking.
    public Transform[] fixedSpawnPoints;     // For Fixed spawn mode.
    public Transform[] objectivePositions;   // For AreaAroundPosition spawn mode.
    public GameObject enemyParent;

    // A list to track all running wave coroutines.
    private List<Coroutine> runningWaves = new List<Coroutine>();

    /// <summary>
    /// Starts a wave of the given type and tracks its coroutine.
    /// Sets the wave difficulty to the passed int.
    /// </summary>
    public void StartWave(WaveType waveType, SpawnType spawnType, int difficulty, Transform[] positions)
    {
        // might remove positions param

        switch (waveType)
        {
            case WaveType.WaveType0:
                // Instantiate a WaveType0, set its parameters, and start its coroutine.
                WaveType0 wave = Instantiate(waveType0Prefab, transform);
                wave.enemyParent = enemyParent;
                wave.difficulty = difficulty;
                wave.spawnRate = spawnRate;
                wave.players = players;
                wave.fixedSpawnPoints = fixedSpawnPoints;
                wave.objectivePositions = objectivePositions;
                wave.positions_tmp = positions;
                // Set the desired spawn type (Fixed, AreaAroundPosition, or AreaAroundPlayers).
                wave.spawnType = spawnType;
                Coroutine c = StartCoroutine(wave.ExecuteWave());
                runningWaves.Add(c);
                break;
            // Extend with other wave types as needed.
            default:
                Debug.LogError("Wave type " + waveType + " not supported.");
                break;
        }
    }

    /// <summary>
    /// Starts a wave of the given type and tracks its corutine
    /// Sets difficulty as WaveManager current difficulty.
    /// </summary>
    /// <param name="waveType"></param>
    public void StartWave(WaveType waveType, SpawnType spawnType, Transform[] positions)
    {
        StartWave(waveType, spawnType, difficulty, positions);
    }

    /// <summary>
    /// Starts a wave of the given type and tracks its corutine
    /// Sets difficulty as WaveManager current difficulty.
    /// </summary>
    /// <param name="waveType"></param>
    public void StartWave(WaveType waveType, SpawnType spawnType, Transform position)
    {
        StartWave(waveType, spawnType, difficulty, new Transform[] {position});
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="waveType"></param>
    /// <param name="spawnType"></param>
    /// <param name="difficulty"></param>
    /// <param name="positions"></param>
    /// <param name="delay"></param>
    public void StartWaveWithDelay(WaveType waveType, SpawnType spawnType, int difficulty, Transform[] positions, float delay)
    { 
        waitForSec(delay);
        StartWave(waveType, spawnType, difficulty, positions);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="waveType"></param>
    /// <param name="spawnType"></param>
    /// <param name="difficulty"></param>
    /// <param name="position"></param>
    /// <param name="delay"></param>
    public void StartWaveWithDelay(WaveType waveType, SpawnType spawnType, int difficulty, Transform position, float delay)
    { 
        waitForSec(delay);
        StartWave(waveType, spawnType, difficulty, new Transform[] {position});
    }

    /// <summary>
    /// Stops all currently running wave coroutines.
    /// </summary>
    public void EndAllWaves()
    {
        foreach (Coroutine waveCoroutine in runningWaves)
        {
            StopCoroutine(waveCoroutine);
        }
        runningWaves.Clear();
    }

    IEnumerator waitForSec(float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);
    }
}