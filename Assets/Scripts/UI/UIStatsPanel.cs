using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIStatsPanel : MonoBehaviour
{
    private Label damage, health;
    private Label ability1;
    private Label skillPointsLabel;

    private UIManager uIManager;
    private UIDocument uIDocument;
    private _Scripts.Skill_System.PlayerSkillManager playerSkillManager;

    public void Initialize() //UIManagerStatsIt
    {
        uIManager = FindAnyObjectByType<UIManager>();
        playerSkillManager = uIManager._playerSkillManager;
        uIDocument = uIManager._uIDocument;

        playerSkillManager.OnSkillPointsChanged += FillLabels;
        GatherLabelReferences();
        FillLabels();
    }

    private void GatherLabelReferences()
    {
        damage = uIDocument.rootVisualElement.Q<Label>(name:"StatsLabel_Damage");
        health = uIDocument.rootVisualElement.Q<Label>(name: "StatsLabel_Health");

        ability1 = uIDocument.rootVisualElement.Q<Label>(name: "AbilitysLabel_Ability1");

        skillPointsLabel = uIDocument.rootVisualElement.Q<Label>(name: "SkillPointsLabel");
    }

    private void FillLabels()
    {
        //Stats
        damage.text = "Damage - " + playerSkillManager.Damage.ToString();
        health.text = "Health - " + playerSkillManager.Health.ToString();

        //Abilitys
        ability1.text = "Ability1 " + (playerSkillManager.Ability1 ? "Unlocked" : "Locked");

        //SkillPoint
        skillPointsLabel.text = "skill Points: " + playerSkillManager.SkillPoints.ToString();
    }
}
