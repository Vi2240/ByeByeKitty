using System.Collections;
using System.Linq;
using UnityEngine;

public class LootDropper : MonoBehaviour
{   bool canRun = true;
    int totalItems = 0;
    int item1 = 0;
    int item2 = 0;

    [SerializeField, Tooltip("GameObjects to drop.")] GameObject[] drops;
    [SerializeField, Tooltip("RNG weight when dropping in the same order.")] int[] weights;

    void Update()
    {
        if (canRun)
        {
            canRun = false;
            totalItems++;

            GameObject randomDrop = GetRandomDrop();
            if (randomDrop.name == drops[0].name)
            {
                item1++;
            }
            if (randomDrop.name == drops[1].name)
            {
                item2++;
            }

            print(randomDrop.name);
            float percent1 = (item1 / totalItems) * 100;
            float percent2 = (item2 / totalItems) * 100;
            print("Item1 drop chance: " + percent1);
            print("Item2 drop chance: " + percent2);
            print(" ");

            StartCoroutine(Cooldown());
        }
    }
    public GameObject GetRandomDrop()
    {
        if (drops.Length != weights.Length || drops.Length == 0)
        {
            Debug.LogError("Drops and weights must have the same length and not be empty.");
            return null;
        }

        // Calculate total weight
        int totalWeight = 0;
        foreach (var weight in weights)
        {
            totalWeight += weight;
        }

        // Generate a random value between 0 and total weight
        float randomValue = Random.value * totalWeight;

        // Determine which drop corresponds to the random value
        float cumulativeWeight = 0;
        for (int i = 0; i < drops.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return drops[i];
            }
        }

        // Fallback (this shouldn't happen if weights are valid)
        return null;
    }

    // Example usage
    void Start()
    {
        GameObject drop = GetRandomDrop();
        if (drop != null)
        {
            Debug.Log($"Dropped: {drop.name}");
        }
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1f);
        canRun = true;
    }
}