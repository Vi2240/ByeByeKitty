using UnityEngine;
using UnityEngine.Rendering;

public class LvlAndXpVault : SingeltonPersistent<LvlAndXpVault>
{
    #region PlayerXp&Lvl
    [SerializeField] int playerLvl;
    [SerializeField] float maxPlayerXp;
    [SerializeField] float maxPlayerXpScale;

    float currentPlayerXp;

    void CheckIfPlayerCanLvlUp()
    {
        if (currentPlayerXp >= maxPlayerXp)
        {
            RemovePlayerXp(maxPlayerXp); // reset current xp but and add the diffrens back
            maxPlayerXp *= maxPlayerXpScale; // Make it harder to lvl up next time (by adding a higher amount of xp that you need)

            playerLvl++;
        }
    }

    public void ResetPlayerLvl()
    {
        //Check if they are sure
        //pop up button and text

        playerLvl = 0;
    }

    public void AddPlayerXp(float XpToAdd)
    {
        currentPlayerXp += XpToAdd;
        CheckIfPlayerCanLvlUp();
    }

    public void RemovePlayerXp(float XpToRemove) 
    {
        currentPlayerXp -= XpToRemove; 
    }
    #endregion

    #region WeaponXp&Lvl

    [SerializeField] ScriptableObject[] weaponsScriptableObject;

    void WeaponToLvlUp()
    {
        //CurrentWeapon(Active)
    }
    #endregion
}
