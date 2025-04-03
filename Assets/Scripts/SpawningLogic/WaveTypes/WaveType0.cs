using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A concrete wave implementation (WaveType0) that spawns enemies based on difficulty and probabilities.
/// </summary>
public class WaveType0 : Wave
{
	[Header("Enemy Prefabs")]
	public GameObject normalEnemy;
	public GameObject speedEnemy;
	public GameObject slowEnemy;
	public GameObject rangeEnemy;

	[Header("Reference Points for Spawning")]
	public Transform[] fixedSpawnPoints;    // For Fixed mode.
	public Transform[] objectivePositions;  // For AreaAroundPosition mode.

	// Define which spawn type this wave uses.
	public SpawnType spawnType = SpawnType.AreaAroundPlayers;

	public override IEnumerator ExecuteWave()
	{
		// Adjust wait time based on spawnRate and difficulty.
		float waitTime = spawnRate / (difficulty + 1);

		// Configure enemy spawn chances based on difficulty.
		int normalEnemyChance, speedEnemyChance, slowEnemyChance, rangeEnemyChance;
		switch (difficulty)
		{
			case 0:
				normalEnemyChance = 10;
				speedEnemyChance  = 0;
				slowEnemyChance   = 5;
				rangeEnemyChance  = 0;
				break;
			case 1:
				normalEnemyChance = 25;
				speedEnemyChance  = 0;
				slowEnemyChance   = 12;
				rangeEnemyChance  = 0;
				break;
			case 2:
				normalEnemyChance = 40;
				speedEnemyChance  = 0;
				slowEnemyChance   = 20;
				rangeEnemyChance  = 0;
				break;
			default:
				normalEnemyChance = 55;
				speedEnemyChance  = 0;
				slowEnemyChance   = 25;
				rangeEnemyChance  = 0;
				break;
		}
		int totalEnemies = normalEnemyChance + speedEnemyChance + slowEnemyChance + rangeEnemyChance;

		// Main spawning loop.
		while (totalEnemies > 0)
		{
			// Calculate how many enemies to spawn this loop.
			int spawnsPerLoop = 5 + 5 * difficulty;
			spawnsPerLoop = (spawnsPerLoop > totalEnemies) ? totalEnemies : spawnsPerLoop;

			for (int i = 0; i < spawnsPerLoop; i++)
			{
				int maxNumber = normalEnemyChance + speedEnemyChance + slowEnemyChance + rangeEnemyChance;
				int spawnNum = Random.Range(0, maxNumber);
				GameObject enemyToSpawn = null;

				// Decide which enemy to spawn.
				if (spawnNum >= maxNumber - rangeEnemyChance)
				{
					rangeEnemyChance--;
					enemyToSpawn = rangeEnemy;
				}
				else if (spawnNum >= normalEnemyChance + speedEnemyChance)
				{
					slowEnemyChance--;
					enemyToSpawn = slowEnemy;
				}
				else if (spawnNum >= normalEnemyChance)
				{
					speedEnemyChance--;
					enemyToSpawn = speedEnemy;
				}
				else
				{
					normalEnemyChance--;
					enemyToSpawn = normalEnemy;
				}
				totalEnemies--;

				// Determine the spawn position based on the selected spawn type.
				Vector3 spawnPos = Vector3.zero;
				switch (spawnType)
				{
					case SpawnType.Fixed:
						spawnPos = GetValidSpawnPosition(SpawnType.Fixed, fixedSpawnPoints);
						break;
					case SpawnType.AreaAroundPosition:
						spawnPos = GetValidSpawnPosition(SpawnType.AreaAroundPosition, positions_tmp);
						break;
					case SpawnType.AreaAroundPlayers:
					default:
						spawnPos = GetValidSpawnPosition(SpawnType.AreaAroundPlayers);
						break;
				}
				GameObject enemyInstance = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                enemyInstance.transform.SetParent(enemyParent.transform, true);
            }
            yield return new WaitForSeconds(waitTime);
		}
	}
}