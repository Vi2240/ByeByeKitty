using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct WaveProgressionEntryConfig
{
    [Tooltip("The type of wave to spawn for this step in the progression.")]
    public WaveType waveType;

    [Tooltip("The base difficulty level for this specific wave.")]
    public int difficulty;

    [Tooltip("Index of the LootTable from 'Available Loot Tables' list on WaveProgressionManager. Use -1 for no specific loot table change after this wave.")]
    public int lootTableIndexForNextPeriod;
}

[System.Serializable]
public struct WaveProgressionEntry
{
    public WaveType waveType;
    public int difficulty;
    public LootTable lootTableForNextPeriod;
}


[System.Serializable]
public struct WaveProgressionPhase
{
    [Tooltip("Descriptive name for this phase (e.g., 'Early Game', 'Mid-Game Challenge', 'Boss Buildup').")]
    public string phaseName;

    [Tooltip("The sequence of individual waves that make up this phase. Configure using indices for loot tables.")]
    public List<WaveProgressionEntryConfig> waveEntriesInPhase;

    [Tooltip("How many times this entire phase should run before moving to the next phase. (-1 for infinity, 0 for skip)")]
    public int runPhaseCount;

    [Tooltip("If true, the difficulty of waves within this phase will increment each time THIS PHASE repeats.")]
    public bool incrementDifficultyOnPhaseRepeat;

    [Tooltip("The maximum additional difficulty that can be added by phase repeats for THIS phase.")]
    public int maxDifficultyAmplifierForPhase;

    [Tooltip("If true, the difficulty amplifier for this phase will be reset when this phase starts. " +
             "If false, the amplifier carries over from the previous phase's repeats (less common).")]
    public bool resetAmplifierOnPhaseStart;
}

public class WaveProgressionManager : MonoBehaviour
{
    [Header("Available Resources")]
    [Tooltip("Define all LootTable ScriptableObjects that can be referenced by index in wave entries. Order matters!")]
    public List<LootTable> availableLootTables = new List<LootTable>();

    [Header("Game Progression Setup")]
    [Tooltip("Define the overall sequence of game phases. Each phase contains its own wave sequence.")]
    public List<WaveProgressionPhase> gamePhases = new List<WaveProgressionPhase>();

    // --- Overall Progression State ---
    private int currentPhaseIndex = 0;
    private int currentPhaseRepeatCounter = 0;
    private int currentWaveInPhaseIndex = 0;

    // --- Difficulty Amplification State ---
    private int currentPhaseDifficultyAmplifier = 0;

    // --- Global Looping (after all phases complete) ---
    [Header("Global Looping (After All Phases)")]
    [Tooltip("If true, the entire sequence of phases will loop back to the beginning after the last phase completes all its repeats.")]
    public bool loopAllPhases = true;

    [Tooltip("If true and loopAllPhases is true, a global difficulty amplifier increases each time the entire set of phases loops.")]
    public bool incrementDifficultyOnGlobalLoop = true;
    private int globalLoopDifficultyAmplifier = 0;

    /// <summary>
    /// Tries to get the next wave entry parameters, including the resolved LootTable.
    /// </summary>
    /// <param name="entry">The output WaveProgressionEntry with resolved LootTable.</param>
    /// <returns>True if a wave entry was successfully retrieved, false otherwise.</returns>
    public bool TryGetNextWaveEntry(out WaveProgressionEntry entry)
    {
        entry = default; // Initialize out parameter

        if (gamePhases == null || gamePhases.Count == 0)
        {
            Debug.LogError("WaveProgressionManager: 'gamePhases' list is empty!", this);
            return false;
        }

        while (true) // Loop to handle state transitions (new phase, new repeat, etc.)
        {
            if (!ValidateAndAdvanceGlobalPhaseState())
            {
                return false; // End of all progression
            }

            WaveProgressionPhase currentPhase = gamePhases[currentPhaseIndex];

            if (ShouldSkipCurrentPhase(currentPhase))
            {
                AdvanceToNextPhase(true);
                continue;
            }

            if (ShouldAdvanceFromCurrentPhaseRepeats(currentPhase))
            {
                AdvanceToNextPhase(true); // True to evaluate amplifier reset for the new phase
                continue;
            }

            // --- At this point, we are in a valid phase and within its allowed repeats (or it's infinite) ---

            if (currentPhase.waveEntriesInPhase == null || currentPhase.waveEntriesInPhase.Count == 0)
            {
                Debug.LogWarning($"WaveProgressionManager: Phase '{currentPhase.phaseName}' (Index {currentPhaseIndex}) has no wave entries! Treating as a completed repeat.");
                HandleEndOfWavesInCurrentRepeat(currentPhase); // Still need to increment repeat counter etc.
                continue;
            }

            if (currentWaveInPhaseIndex >= currentPhase.waveEntriesInPhase.Count)
            {
                HandleEndOfWavesInCurrentRepeat(currentPhase);
                continue;
            }

            // If all checks pass, we have a wave to process and return
            entry = ResolveAndProcessCurrentWaveEntry(currentPhase);
            return true;
        }
    }

    private bool ValidateAndAdvanceGlobalPhaseState()
    {
        if (currentPhaseIndex >= gamePhases.Count)
        {
            if (loopAllPhases)
            {
                Debug.Log("WaveProgressionManager: All phases completed. Looping back to Phase 0.");
                currentPhaseIndex = 0;
                currentPhaseRepeatCounter = 0;
                currentWaveInPhaseIndex = 0;
                if (incrementDifficultyOnGlobalLoop)
                {
                    globalLoopDifficultyAmplifier++;
                }
                currentPhaseDifficultyAmplifier = 0;
                return true;
            }
            else
            {
                Debug.Log("WaveProgressionManager: All phases completed. No global loop. End of progression.");
                return false;
            }
        }
        return true;
    }

    private bool ShouldSkipCurrentPhase(WaveProgressionPhase phase)
    {
        if (phase.runPhaseCount == 0)
        {
            Debug.Log($"WaveProgressionManager: Phase '{phase.phaseName}' (Index {currentPhaseIndex}) has runPhaseCount = 0. Skipping.");
            return true;
        }
        return false;
    }

    private void AdvanceToNextPhase(bool evaluateAmplifierReset)
    {
        currentPhaseIndex++;
        currentPhaseRepeatCounter = 0;
        currentWaveInPhaseIndex = 0;
        // Only reset amplifier if the *newly selected* phase dictates it
        if (evaluateAmplifierReset && currentPhaseIndex < gamePhases.Count && gamePhases[currentPhaseIndex].resetAmplifierOnPhaseStart)
        {
            currentPhaseDifficultyAmplifier = 0;
            Debug.Log($"Phase difficulty amplifier reset for new phase '{gamePhases[currentPhaseIndex].phaseName}'.");
        }
    }

    private bool ShouldAdvanceFromCurrentPhaseRepeats(WaveProgressionPhase phase)
    {
        if (phase.runPhaseCount != -1 && currentPhaseRepeatCounter >= phase.runPhaseCount)
        {
            Debug.Log($"WaveProgressionManager: Phase '{phase.phaseName}' (Index {currentPhaseIndex}) finished its {phase.runPhaseCount} repeats. Attempting to move to next phase.");
            return true;
        }
        return false;
    }

    private void HandleEndOfWavesInCurrentRepeat(WaveProgressionPhase currentPhase)
    {
        string repeatCountDisplay = (currentPhase.runPhaseCount == -1 ? "Infinite" : currentPhase.runPhaseCount.ToString());
        Debug.Log($"WaveProgressionManager: Finished all waves in current repeat ({currentPhaseRepeatCounter + 1}/{repeatCountDisplay}) of phase '{currentPhase.phaseName}'.");

        currentPhaseRepeatCounter++;
        currentWaveInPhaseIndex = 0;

        bool willRepeatThisPhaseAgain = (currentPhase.runPhaseCount == -1) || (currentPhaseRepeatCounter < currentPhase.runPhaseCount);

        if (willRepeatThisPhaseAgain && currentPhase.incrementDifficultyOnPhaseRepeat)
        {
            currentPhaseDifficultyAmplifier = Mathf.Min(currentPhaseDifficultyAmplifier + 1, currentPhase.maxDifficultyAmplifierForPhase);
            Debug.Log($"Phase difficulty amplifier for '{currentPhase.phaseName}' increased to {currentPhaseDifficultyAmplifier}.");
        }
    }

    private WaveProgressionEntry ResolveAndProcessCurrentWaveEntry(WaveProgressionPhase currentPhase)
    {
        WaveProgressionEntryConfig configEntry = currentPhase.waveEntriesInPhase[currentWaveInPhaseIndex];

        WaveProgressionEntry entryToReturn = new WaveProgressionEntry
        {
            waveType = configEntry.waveType,
            difficulty = configEntry.difficulty // Base difficulty, will be modified
            // lootTableForNextPeriod is resolved next
        };

        // Resolve LootTable from index
        if (configEntry.lootTableIndexForNextPeriod >= 0 &&
            configEntry.lootTableIndexForNextPeriod < availableLootTables.Count)
        {
            entryToReturn.lootTableForNextPeriod = availableLootTables[configEntry.lootTableIndexForNextPeriod];
        }
        else if (configEntry.lootTableIndexForNextPeriod != -1) // Index is invalid but not the "no change" signal
        {
            Debug.LogWarning($"WaveProgressionManager: LootTable index {configEntry.lootTableIndexForNextPeriod} for phase '{currentPhase.phaseName}', wave {currentWaveInPhaseIndex + 1} is out of bounds (Available: {availableLootTables.Count}). No specific loot table will be applied.");
            entryToReturn.lootTableForNextPeriod = null;
        }
        else // lootTableIndexForNextPeriod is -1 (no change)
        {
            entryToReturn.lootTableForNextPeriod = null;
        }

        // Apply difficulty amplifiers
        int finalDifficulty = entryToReturn.difficulty + currentPhaseDifficultyAmplifier + globalLoopDifficultyAmplifier;
        entryToReturn.difficulty = Mathf.Max(0, finalDifficulty); // Ensure difficulty is not negative

        string repeatCountDisplay = (currentPhase.runPhaseCount == -1 ? "Infinite" : currentPhase.runPhaseCount.ToString());
        Debug.Log($"WaveProgressionManager: Providing Wave - Phase '{currentPhase.phaseName}' (Repeat {currentPhaseRepeatCounter + 1}/{repeatCountDisplay}), " +
                  $"WaveInPhase {currentWaveInPhaseIndex + 1}/{currentPhase.waveEntriesInPhase.Count}. " +
                  $"Type: {entryToReturn.waveType}, BaseDiff: {configEntry.difficulty}, PhaseAmp: {currentPhaseDifficultyAmplifier}, GlobalAmp: {globalLoopDifficultyAmplifier}, FinalDiff: {entryToReturn.difficulty}. " +
                  $"LootAfter: {(entryToReturn.lootTableForNextPeriod != null ? entryToReturn.lootTableForNextPeriod.name : (configEntry.lootTableIndexForNextPeriod == -1 ? "NoChange" : "InvalidIndex"))}");

        currentWaveInPhaseIndex++;
        return entryToReturn;
    }

    public void ResetProgression()
    {
        currentPhaseIndex = 0;
        currentPhaseRepeatCounter = 0;
        currentWaveInPhaseIndex = 0;
        currentPhaseDifficultyAmplifier = 0;
        globalLoopDifficultyAmplifier = 0;
        Debug.Log("WaveProgressionManager: Full progression, counters, and difficulty amplifiers reset.", this);
    }
}