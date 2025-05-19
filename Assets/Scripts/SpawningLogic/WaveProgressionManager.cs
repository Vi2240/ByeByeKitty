using UnityEngine;
using System.Collections.Generic;

// Make sure WaveType and SpawnType enums are accessible if they are defined elsewhere,
// or define them here if they are only for this system.
// public enum WaveType { WaveType0, WaveType1 /*, ... */ }
// public enum SpawnType { Fixed, AreaAroundPosition, AreaAroundPlayers }

[System.Serializable]
public struct WaveProgressionEntry
{
    [Tooltip("The type of wave to spawn for this step in the progression.")]
    public WaveType waveType;

    [Tooltip("The difficulty level for this specific wave.")]
    public int difficulty;

    // Optional: Override other parameters per wave if needed
    // public float spawnRateOverride = -1f; // -1 to use WaveManager's default
    // public SpawnType spawnTypeOverride = SpawnType.AreaAroundPosition; // Or some indicator to use WaveManager's default
    // public float delayAfterThisWave = -1f; // -1 to use WaveManager's default delayBetweenWaves
}

public class WaveProgressionManager : MonoBehaviour
{
    [Tooltip("Define the sequence of waves. The cycle will loop through this list.")]
    public List<WaveProgressionEntry> waveProgressionSequence = new List<WaveProgressionEntry>();

    [Tooltip("If true, the wave sequence will loop back to the beginning after the last entry. If false, it will stop or repeat the last entry.")]
    public bool loopSequence = true;

    [Tooltip("If loopSequence is false, and this is true, the last wave in the sequence will be repeated indefinitely.")]
    public bool repeatLastWaveWhenNotLooping = true;


    private int currentWaveIndex = 0; // Tracks the current position in the waveProgressionSequence

    /// <summary>
    /// Gets the parameters for the next wave in the progression.
    /// Advances the progression index.
    /// </summary>
    /// <param name="waveType">Output: The type of the next wave.</param>
    /// <param name="difficulty">Output: The difficulty of the next wave.</param>
    /// <returns>True if a wave was successfully retrieved, false if the sequence is empty or finished (and not looping/repeating).</returns>
    public bool GetNextWaveParameters(out WaveType waveType, out int difficulty)
    {
        // Set default out values
        waveType = default(WaveType); // Or your most basic wave type
        difficulty = 0;

        if (waveProgressionSequence == null || waveProgressionSequence.Count == 0)
        {
            Debug.LogError("WaveProgressionManager: waveProgressionSequence is empty!", this);
            return false;
        }

        if (currentWaveIndex >= waveProgressionSequence.Count)
        {
            if (loopSequence)
            {
                currentWaveIndex = 0; // Loop back to the start
            }
            else if (repeatLastWaveWhenNotLooping)
            {
                currentWaveIndex = waveProgressionSequence.Count - 1; // Stay on the last wave
            }
            else
            {
                Debug.Log("WaveProgressionManager: End of wave sequence reached and not looping/repeating.", this);
                return false; // End of sequence
            }
        }

        WaveProgressionEntry currentEntry = waveProgressionSequence[currentWaveIndex];
        waveType = currentEntry.waveType;
        difficulty = currentEntry.difficulty;

        Debug.Log($"WaveProgressionManager: Providing Wave {currentWaveIndex + 1}/{waveProgressionSequence.Count} - Type: {waveType}, Difficulty: {difficulty}");

        currentWaveIndex++; // Advance for the next call

        return true;
    }

    public void ResetProgression()
    {
        currentWaveIndex = 0;
        Debug.Log("WaveProgressionManager: Progression reset.", this);
    }

    // Optional: Call this at the start if you want to ensure it's ready
    // void Start()
    // {
    //     ResetProgression(); // Start from the beginning
    // }
}