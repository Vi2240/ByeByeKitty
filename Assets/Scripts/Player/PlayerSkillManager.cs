using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Skill_System;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Skill_System
{

    public class PlayerSkillManager : MonoBehaviour
    {
        private int _ability1; //Abilitys
        private int _skillPoints;

        public bool Ability1 => _ability1 > 0;

        public int SkillPoints => _skillPoints;

        public event UnityAction OnSkillPointsChanged;

        private List<ScriptableSkill> _unlockedSkills = new List<ScriptableSkill>();

        private void Awake()
        {
            _skillPoints = 0;
        }

        public void GainSkillPoint()
        {
            _skillPoints++;
            OnSkillPointsChanged?.Invoke();
        }

        public bool CanAffordSkill(ScriptableSkill skill)
        {
            return _skillPoints >= skill.cost;
        }

        public void UnlockSkill(ScriptableSkill skill)
        {
            if (!CanAffordSkill(skill)) return;

            ModifyStats(skill);

            _unlockedSkills.Add(skill);
            _skillPoints -= skill.cost;

            OnSkillPointsChanged?.Invoke();
        }

        public bool IsSkillUnlocked(ScriptableSkill skill)
        {
            return _unlockedSkills.Contains(skill);
        }

        public bool ReqstMet(ScriptableSkill skill)
        {
            return skill.SkillRequisit.Count == 0 || skill.SkillRequisit.All(_unlockedSkills.Contains);
        }

        private void ModifyStats(ScriptableSkill skill)
        {
            foreach (UpgradeData data in skill.upgradeData)
            {
                bool isPrecent = data.isPercentage;
                switch (data.statsType)
                {
                    case StatTypes.damage:
                        ModifyStat(ref InventoryAndBuffs.playerDamageMultiplier, data);
                        break;
                    case StatTypes.attack_speed:
                        ModifyStat(ref InventoryAndBuffs.playerFireRateMultiplier, data);
                        break;
                    case StatTypes.health:
                        ModifyStat(ref InventoryAndBuffs.playerHealthMultiplier, data);
                        break;
                    case StatTypes.speed:
                        ModifyStat(ref InventoryAndBuffs.playerSpeedMultiplier, data);
                        break;
                        //add new skills
                }
            }
        }

        private void ModifyStat(ref float stat, UpgradeData data)
        {
            if (data.isPercentage) stat += (int)(stat * (data.skillIncreaseAmount / 100f));
            else stat += data.skillIncreaseAmount;
        }
    }
}