using UnityEngine;

public class PlayerAbilitiyManager : MonoBehaviour
{
    [Header("Abilities")]
    public SheildAbility shieldAbility;    

    [Header("Other Variables")]
    public int selectedAbility = 1; // 1 for shield, 2 for other abilities

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.G)) return;

        Debug.Log("ability");
        switch (selectedAbility)
        {
            case 1: 
                shieldAbility.PlaceShield(gameObject.transform);
                break;
            case 2:                    
                break;
            default:
                break;
        }
    }
}