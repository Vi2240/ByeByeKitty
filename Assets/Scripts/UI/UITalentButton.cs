using Unity.VisualScripting;
using _Scripts.Skill_System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System;

[System.Serializable]
public class UITalentButton
{
    private Button button;
    private ScriptableSkill skill;
    private bool _isUnlocked = false;

    public static UnityAction<ScriptableSkill> OnSkillButtonClicked;

    public UITalentButton(Button assignedButton, ScriptableSkill assignedskill)
    {
        button = assignedButton;
        button.clicked += OnClick;
        skill = assignedskill;
        if (assignedskill.SkillIcon) button.style.backgroundImage = new StyleBackground(assignedskill.SkillIcon);
    }

    private void OnClick()
    {
        OnSkillButtonClicked?.Invoke(skill);
    }
}
