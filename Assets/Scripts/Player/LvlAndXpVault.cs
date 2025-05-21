using _Scripts.Skill_System;
using UnityEngine;
using UnityEngine.Rendering;

public class LvlAndXpVault : MonoBehaviour
{
    #region PlayerXp&Lvl
    [SerializeField] int playerLvl;
    [SerializeField] float maxPlayerXp;
    [SerializeField] float maxPlayerXpScale;

    float currentPlayerXp;

    PlayerSkillManager skillManager;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        skillManager = FindAnyObjectByType<PlayerSkillManager>();
    }

    void CheckIfPlayerCanLvlUp()
    {
        if (currentPlayerXp >= maxPlayerXp)
        {
            RemovePlayerXp(maxPlayerXp); // reset current xp but and add the difference back
            maxPlayerXp *= maxPlayerXpScale; // Make it harder to lvl up next time (increase xp needed)

            playerLvl++;
            skillManager.GainSkillPoint();
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
