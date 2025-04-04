using UnityEngine;

public class SheildAbility : MonoBehaviour
{
    public GameObject shieldPrefab; // Assign the Shield prefab in the inspector
    private GameObject activeShield;
    int hitPoints = 0;

    public void PlaceShield(int level, Transform spawnPosition)
    {
        switch (level)
        {            
            case 2:
                hitPoints = 35; // Level 2 shield
                break;
            case 3:
                hitPoints = 50; // Level 3 shield
                break;
            default:
                hitPoints = 20; // Default hit points
                return;
        }  

        PlaceShield(spawnPosition);
    }      

    public void PlaceShield(Transform spawnPosition)
    {
        hitPoints = (hitPoints < 1) ? 1 : hitPoints; // Ensure hitPoints is at least 1

        if (activeShield != null)
        {
            Destroy(activeShield);
        }

        // Instantiate shield at player's position
        activeShield = Instantiate(shieldPrefab, spawnPosition.position, Quaternion.identity);
        
        Shield shieldComponent = activeShield.GetComponent<Shield>();
        if (shieldComponent != null)
        {
            shieldComponent.Initialize(hitPoints);
        }
    }
}