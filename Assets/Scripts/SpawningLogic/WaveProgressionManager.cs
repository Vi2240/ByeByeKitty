using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct WaveProgressionEntry
{
    [Tooltip("The type of wave to spawn for this step in the progression.")]
    public WaveType waveType;

    [Tooltip("The difficulty level for this specific wave.")]
    public int difficulty;

    [Tooltip("The LootTable that should become the default in LootManager AFTER this wave completes. Assign if desired.")]
    public LootTable lootTableForNextPeriod;

    // Optional: You could still have overrides here if a specific wave in the sequence
    // needs to deviate from the WaveManager's 'continuousWaveSpawnType' or 'delayBetweenWaves'
    // public SpawnType? overrideSpawnType = null; // Use nullable if you want to check if it's set
    // public float? overrideDelayAfterThisWave = null;
}

public class WaveProgressionManager : MonoBehaviour
{
    [Tooltip("Define the sequence of waves. The cycle will loop through this list.")]
    public List<WaveProgressionEntry> waveProgressionSequence = new List<WaveProgressionEntry>();

    [Tooltip("If true, the wave sequence will loop back to the beginning after the last entry. If false, it will stop or repeat the last entry.")]
    public bool loopSequence = true;

    [Tooltip("If loopSequence is false, and this is true, the last wave in the sequence will be repeated indefinitely.")]
    public bool repeatLastWaveWhenNotLooping = true;

    [Tooltip("If true, and loopSequence is true, the difficulty of all waves will increment by 1 each time the sequence loops.")]
    [SerializeField] bool incrementDifficultyOnLoop = true;
    private int currentLoopDifficultyAmplifier = 0; // Stores the added difficulty from looping

    private int currentWaveIndex = 0; // Tracks the current position in the waveProgressionSequence

    public bool TryGetNextWaveEntry(out WaveProgressionEntry entry)
    {
        entry = default;

        if (waveProgressionSequence == null || waveProgressionSequence.Count == 0)
        {
            Debug.LogError("WaveProgressionManager: waveProgressionSequence is empty!", this);
            return false;
        }

        int indexToUseForThisCall = currentWaveIndex;

        if (indexToUseForThisCall >= waveProgressionSequence.Count)
        {
            if (loopSequence)
            {
                indexToUseForThisCall = 0;
                currentLoopDifficultyAmplifier += incrementDifficultyOnLoop ? 1 : 0;
            }
            else if (repeatLastWaveWhenNotLooping)
            {
                indexToUseForThisCall = waveProgressionSequence.Count - 1;
                // No difficulty increment if just repeating the last wave without a full loop
            }
            else
            {
                Debug.Log("WaveProgressionManager: End of wave sequence reached and not looping/repeating.", this);
                return false;
            }
        }

        // Get the base entry
        WaveProgressionEntry baseEntry = waveProgressionSequence[indexToUseForThisCall];

        // Create a temporary entry to modify its difficulty for this call
        entry = baseEntry; 
        entry.difficulty = baseEntry.difficulty + currentLoopDifficultyAmplifier; // Apply the aplifier

        Debug.Log($"WaveProgressionManager: Providing Wave (index {indexToUseForThisCall}, base diff: {baseEntry.difficulty}, amplifier: {currentLoopDifficultyAmplifier}) " +
                  $"Type: {entry.waveType}, Final Diff: {entry.difficulty}, " + // Log final difficulty
                  $"LootAfter: {(entry.lootTableForNextPeriod != null ? entry.lootTableForNextPeriod.name : "None")}");

        // Prepare currentWaveIndex for the NEXT call
        if (repeatLastWaveWhenNotLooping && indexToUseForThisCall == waveProgressionSequence.Count - 1 && !loopSequence)
        {
            currentWaveIndex = waveProgressionSequence.Count - 1;
        }
        else
        {
            currentWaveIndex = indexToUseForThisCall + 1;
        }

        return true;
    }

    public void ResetProgression()
    {
        currentWaveIndex = 0;
        currentLoopDifficultyAmplifier = 0;
        Debug.Log("WaveProgressionManager: Progression and difficulty bonus reset.", this);
    }
}