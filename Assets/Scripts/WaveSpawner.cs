using NUnit.Framework.Internal;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{

    [SerializeField] GameObject normalEnemy, speedEnemy, slowEnemy, rangeEnemy;
    [SerializeField] Collider2D[] colliders; // save all colliders in the are that enemy should not spawn on
    [SerializeField] LayerMask notSpawnLayer; // de layer that it does not have selekten enemys can spawn on

    [SerializeField] float radiusOfArea;

    [Header("ChanceToSpawnEnemy")]
    [SerializeField] float NormalEnemy_ChanceToSpawn = 95f;
    [SerializeField] float SpeedEnemy_ChanceToSpawn = 2.5f;
    [SerializeField] float SlowEnemy_ChanceToSpawn = 1f;
    [SerializeField] float RangeEnemy_ChanceToSpawn = 0.5f;

    [Header("SpawnRange")]
    [SerializeField] float minSpawnPosX = -5f;
    [SerializeField] float maxSpawnPosX = 5f;
    [SerializeField] float minSpawnPosY = -5f;
    [SerializeField] float maxSpawnPosY = 5f;

    [Header("EnemyCount")]
    [SerializeField] int maxEnemyCountPerWave;
    [SerializeField] int maxEnemyCount;
    [SerializeField] int enemyCountPerWave;
    [SerializeField] int enemyCount;

    [Header("Timers")]
    [SerializeField] float enemyFrequencyTime;
    [SerializeField] float waveTime = 30f;
    [SerializeField] float waveDelay = 10f;

    Objective objective;

    bool canSpawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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

    void EnemySpawnPos(GameObject _enemyToSpawn)
    {
        Vector2 spawnPos = Vector2.zero;
        bool canSpawnHere = false;

        while (!canSpawnHere)
        {
            float spawnPosX = UnityEngine.Random.Range(minSpawnPosX, maxSpawnPosX);
            float spawnPosY = UnityEngine.Random.Range(minSpawnPosY, maxSpawnPosY);

            spawnPos = new Vector2(spawnPosX, spawnPosY);
            canSpawnHere = PrentsSpawnOverlap(spawnPos);
            canSpawn = canSpawnHere;

            if (canSpawnHere)
            {
                break;
            }
        }


        GameObject enemyToSpawn = _enemyToSpawn;
        SpawnEnemy(enemyToSpawn, spawnPos);
    }

    bool PrentsSpawnOverlap(Vector2 spawnPos)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Vector3 centerPoint = colliders[i].bounds.center;

            float width = colliders[i].bounds.extents.x;
            float height = colliders[i].bounds.extents.y;

            float leftExtent = centerPoint.x - width;
            float rightExtent = centerPoint.x + width;
            float lowerExtent = centerPoint.y - height;
            float upperExtent = centerPoint.y + height;

            if (spawnPos.x >= leftExtent && spawnPos.x <= rightExtent)
            {
                if (spawnPos.y >= lowerExtent && spawnPos.y <= upperExtent)
                {
                    return false;
                }
            }

            return true;
        }

        return true;
    }

    void SpawnEnemy(GameObject enemyToSpawn, Vector2 spawnPos)
    {
        if (canSpawn && enemyToSpawn != null)
        {
            GameObject newEnemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
            enemyCountPerWave++;
        }
    }

    public void AddEnemyCount()
    {
        enemyCount++;
    }

    public void SubEnemyCount()
    {
        enemyCount--;
    }

    public IEnumerator Waves()
    {
        while (enemyCountPerWave < maxEnemyCountPerWave && enemyCount < maxEnemyCount)
        {
            if (enemyCountPerWave >= maxEnemyCountPerWave && enemyCount >= maxEnemyCount)
            {
                break;
            }

            yield return new WaitForSeconds(enemyFrequencyTime);

            EnemySpawnPos(RandomizeEnemyToSpawn());
        }

        yield return new WaitForSeconds(waveTime);

        StartCoroutine(Waves());
        enemyCountPerWave = 0;
    }
}
