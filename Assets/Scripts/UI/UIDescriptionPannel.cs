using _Scripts.Skill_System;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class UIDescriptionPannel : MonoBehaviour
{
    private UIManager uIManager;
    private ScriptableSkill assignedSkill;

    private VisualElement skillImage;
    private Label skillName, skillDescriptionLabel, skillCost, skillRequire;
    private Button buySkillButton;

    private void Awake()
    {
        uIManager = GetComponent<UIManager>();
    }

    private void OnEnable()
    {
        UITalentButton.OnSkillButtonClicked += PopulateLabelText;
    }

    private void OnDisable()
    {
        UITalentButton.OnSkillButtonClicked -= PopulateLabelText;
        if (buySkillButton != null) buySkillButton.clicked -= PurchaseSkill;
    }

    private void Start()
    {
        GatherLabelReferences();
        StartCoroutine(DelayedPopulate());
    }

    private IEnumerator DelayedPopulate()
    {
        yield return null; // Wait 1 frame to ensure UI is fully loaded
        var skill = uIManager._SkillLibrary.GetSkillOfTier(1)[0];
        PopulateLabelText(skill);
    }

    private void GatherLabelReferences()
    {
        var root = uIManager._uIDocument?.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("Root Visual Element is null!");
            return;
        }

        skillImage = root.Q<VisualElement>("Icon");
        skillName = root.Q<Label>("SkillNameLabel");
        skillDescriptionLabel = root.Q<Label>("SkillDescriptionLabel");
        skillCost = root.Q<Label>("CostLabel");
        skillRequire = root.Q<Label>("RequireLabel");
        buySkillButton = root.Q<Button>("BuySkillButton");

        if (buySkillButton != null)
            buySkillButton.clicked += PurchaseSkill;
        else
            Debug.LogWarning("BuySkillButton not found in UI.");
    }

    private void PurchaseSkill()
    {
        if (uIManager._playerSkillManager.CanAffordSkill(assignedSkill))
        {
            uIManager._playerSkillManager.UnlockSkill(assignedSkill);
            PopulateLabelText(assignedSkill);
        }
    }

    private void PopulateLabelText(ScriptableSkill skill)
    {
        if (skill == null) return;
        assignedSkill = skill;

        if(assignedSkill.SkillIcon) skillImage.style.backgroundImage = new StyleBackground(assignedSkill.SkillIcon);
        skillName.text = assignedSkill.SkillName;
        skillDescriptionLabel.text = assignedSkill.SkillDescription;
        skillCost.text = $"Cost: {assignedSkill.cost}";

        if (assignedSkill.SkillRequisit.Count > 0)
        {
            StringBuilder sb = new StringBuilder("Requirements: ");
            for (int i = 0; i < assignedSkill.SkillRequisit.Count; i++)
            {
                sb.Append(assignedSkill.SkillRequisit[i].SkillName);
                if (i == assignedSkill.SkillRequisit.Count - 2)
                    sb.Append(" and ");
                else if (i < assignedSkill.SkillRequisit.Count - 1)
                    sb.Append(", ");
            }
            sb.Append(".");
            skillRequire.text = sb.ToString();
        }
        else
        {
            skillRequire.text = "";
        }

        if (uIManager._playerSkillManager.IsSkillUnlocked(assignedSkill))
        {
            buySkillButton.text = "Bought";
            buySkillButton.SetEnabled(false);
        }
        else if (!uIManager._playerSkillManager.ReqstMet(assignedSkill))
        {
            buySkillButton.text = "Requist not met";
            buySkillButton.SetEnabled(false);
        }
        else if (!uIManager._playerSkillManager.CanAffordSkill(assignedSkill))
        {
            buySkillButton.text = "Can't afford";
            buySkillButton.SetEnabled(false);
        }
        else
        {
            buySkillButton.text = "";
            buySkillButton.SetEnabled(true);
        }
    }
}