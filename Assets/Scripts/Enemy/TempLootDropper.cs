using System.Linq;
using UnityEngine;
using System.Collections;

public class TempLootDropper : MonoBehaviour
{
    [SerializeField, Tooltip("GameObjects to drop.")] GameObject[] drops;
    [SerializeField, Tooltip("RNG weight when dropping in the same order.")] int[] weights;
    bool canRun = true;

    void Update()
    {
        if (canRun)
        {
            canRun = false;
            GetRandomDrop();
            StartCoroutine(Cooldown());
        }
    }

    public GameObject GetRandomDrop()
    {
        if (drops.Length != weights.Length)
        {
            Debug.LogError("Drops and weights arrays must have the same length!");
            return null;
        }

        // Calculate total weight and create the itemsToDrop array
        int totalWeight = weights.Sum();
        GameObject[] itemsToDrop = new GameObject[totalWeight];

        int index = 0;
        for (int i = 0; i < drops.Length; i++)
        {
            for (int j = 0; j < weights[i]; j++)
            {
                itemsToDrop[index] = drops[i];
                index++;
            }
        }

        // Select a random item
        int randomIndex = Random.Range(0, totalWeight); // Random.Range is inclusive-exclusive
        print(itemsToDrop[randomIndex].name);
        return itemsToDrop[randomIndex];
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1f);
        canRun = true;
    }
}
