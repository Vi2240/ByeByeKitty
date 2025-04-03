using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using _Scripts.Skill_System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ScritableSkillLibrary skillLibrary;
    [SerializeField] private VisualTreeAsset uiTalentButton;

    [SerializeField] private List<UITalentButton> _talentButtons = new List<UITalentButton>();

    private PlayerSkillManager playerSkillManager;
    public PlayerSkillManager _playerSkillManager => playerSkillManager;

    private UIDocument uIDocument;
    public UIDocument _uIDocument => uIDocument;

    private VisualElement skillRowOne, skillRowTwo, skillRowThree, skillRowFour, skillRowFive;

    void Start()
    {
        uIDocument = GetComponent<UIDocument>();
        playerSkillManager = FindAnyObjectByType<PlayerSkillManager>();

        UIStatsPanel uIStatsPanel = FindAnyObjectByType<UIStatsPanel>();
        if(uIStatsPanel != null)
        {
            uIStatsPanel.Initialize();
        }

        CreateSkillButtons();
    }

    private void CreateSkillButtons()
    {
        VisualElement root = _uIDocument.rootVisualElement;
        skillRowOne = root.Q<VisualElement>(name: "Skill_RowOne");
        skillRowTwo = root.Q<VisualElement>(name: "Skill_RowTwo");
        skillRowThree = root.Q<VisualElement>(name: "Skill_RowThree");
        skillRowFour = root.Q<VisualElement>(name: "Skill_RowFour");
        skillRowFive = root.Q<VisualElement>(name: "Skill_RowFive");
        /*
        SpawnButton(skillRowOne, skillLibrary.GetSkillOfTier(1));   
        SpawnButton(skillRowTwo, skillLibrary.GetSkillOfTier(2));
        SpawnButton(skillRowThree, skillLibrary.GetSkillOfTier(3));
        SpawnButton(skillRowFour, skillLibrary.GetSkillOfTier(4));
        SpawnButton(skillRowFive, skillLibrary.GetSkillOfTier(5));*/
    }

    private void SpawnButton(VisualElement parent, List<ScriptableSkill> skills)
    {
        foreach (var skill in skills)
        {
            Button clonedButton = uiTalentButton.CloneTree().Q<Button>();
            _talentButtons.Add(item: new UITalentButton(clonedButton, skill));
            parent.Add(clonedButton);
        }
    }
}