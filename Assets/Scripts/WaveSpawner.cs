using NUnit.Framework.Internal;
using System.Collections;
using System;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{

    [SerializeField] GameObject normalEnemy, speedEnemy, slowEnemy, rangeEnemy;
    [SerializeField] GameObject[] spawnPoints;

    [Header("ChanceToSpawnEnemy")]
    [SerializeField] float NormalEnemy_ChanceToSpawn = 95f;
    [SerializeField] float SpeedEnemy_ChanceToSpawn = 2.5f;
    [SerializeField] float SlowEnemy_ChanceToSpawn = 1f;
    [SerializeField] float RangeEnemy_ChanceToSpawn = 0.5f;

    [Header("Timers")]
    [SerializeField] float spawnRate;

    Objective objective;

    private void Start()
    {
        objective = GetComponent<Objective>();
        StartCoroutine(Spawner());
    }

    GameObject RandomizeEnemyToSpawn()
    {
        float maxNumber = 101;

        if (NormalEnemy_ChanceToSpawn + SpeedEnemy_ChanceToSpawn + SlowEnemy_ChanceToSpawn + RangeEnemy_ChanceToSpawn != maxNumber)
        {
            NormalEnemy_ChanceToSpawn = Math.Abs(SpeedEnemy_ChanceToSpawn + SlowEnemy_ChanceToSpawn + RangeEnemy_ChanceToSpawn - maxNumber);
        }

        float Number = UnityEngine.Random.Range(0, maxNumber);

        if (Number >= 0 && Number < RangeEnemy_ChanceToSpawn)
        {
            return rangeEnemy;
        }

        if (Number >= RangeEnemy_ChanceToSpawn && Number < SlowEnemy_ChanceToSpawn)
        {
            return slowEnemy;
        }

        if (Number >= SlowEnemy_ChanceToSpawn && Number < SpeedEnemy_ChanceToSpawn)
        {
            return speedEnemy;
        }

        if (Number >= SpeedEnemy_ChanceToSpawn && Number < NormalEnemy_ChanceToSpawn + 1)
        {
            return normalEnemy;
        }

        return null;
    }

    IEnumerator Spawner()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnRate);

        while (objective.CanSpawn)
        {
            yield return wait;
            GameObject enemyToSpawn = RandomizeEnemyToSpawn();
            int randomSpawnPoint = UnityEngine.Random.Range(0, spawnPoints.Length);

            Instantiate(enemyToSpawn, spawnPoints[randomSpawnPoint].transform.position, Quaternion.identity);
        }
    }
}
