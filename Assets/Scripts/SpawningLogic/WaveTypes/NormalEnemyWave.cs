using System.Collections;
using UnityEngine;

/// <summary>
/// A concrete wave implementation (WaveType2) that spawns enemies based on difficulty and probabilities.
/// </summary>
public class NormalEnemyWave : Wave // Ensure Wave.cs exists and is the base class
{
    // Define which spawn type this wave uses. This will be set by WaveManager.
    public SpawnType spawnType = SpawnType.AreaAroundPlayers; // Default, but WaveManager should override
    public Transform[] fixedSpawnPoints;
    public Transform[] objectivePositions;

    protected override EnemyComposition GetEnemyCompositionForDifficulty(int currentDifficulty)
    {
        EnemyComposition composition = new EnemyComposition();
        switch (currentDifficulty)
        {
            case 0: // Easy
                composition.normalCount = 10;
                composition.meleeCount  = 2;
                composition.slowCount   = 5;
                composition.rangeCount  = 0;
                break;
            case 1: // Medium
                composition.normalCount = 25;
                composition.meleeCount  = 3;
                composition.slowCount   = 15;
                composition.rangeCount  = 0;
                break;
            case 2: // Hard
                composition.normalCount = 40;
                composition.meleeCount  = 4;
                composition.slowCount   = 20;
                composition.rangeCount  = 0;
                break;
            default: // Very Hard / Scaled
                composition.normalCount = 55    + (currentDifficulty - 3) * 15;
                composition.meleeCount  = 5     + (currentDifficulty - 3) * 1;
                composition.slowCount   = 25    + (currentDifficulty - 3) * 5;
                composition.rangeCount  = 0; // + (currentDifficulty - 3) * 0;
                break;
        }
        return composition;
    }

    public override IEnumerator ExecuteWave()
    {
        // The 'difficulty' field is inherited and set by WaveManager
        // The 'spawnRate' field is inherited and set by WaveManager
        // The 'spawnType' field is set by WaveManager on this instance.
        // 'fixedSpawnPoints' and 'objectivePositions' are also set by WaveManager on this instance.

        // Get the name for logging (could also be a virtual property in base Wave)
        string waveTypeName = this.GetType().Name; // Gets "NormalEnemyWave"

        Debug.Log($"Executing {waveTypeName}: Difficulty {this.difficulty}, SpawnRate {this.spawnRate}, SpawnType {this.spawnType}");

        // Call the common spawning logic from the base class
        // Pass the effective spawnType and the specific spawn point arrays for this wave instance
        yield return StartCoroutine(
            base.SpawnEnemyLoop(waveTypeName, this.spawnType, this.fixedSpawnPoints, this.objectivePositions)
        );
    }
}