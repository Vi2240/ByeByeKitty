using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using _Scripts.Skill_System;
using UnityEngine.UIElements;

namespace _Scripts.Skill_System
{
    [CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/New Skill", order = 0)]
    public class ScriptableSkill : ScriptableObject
    {
        public List<UpgradeData> upgradeData = new List<UpgradeData>();
        public bool IsAbility;
        public string SkillName;
        public bool overWriteDescription;
        [TextArea(1, 4)] public string SkillDescription;
        public Sprite SkillIcon;
        public List<ScriptableSkill> SkillRequisit = new List<ScriptableSkill>();
        public int skillTier;
        public int cost;

        private void OnValidate()
        {
            if (SkillName == "") SkillName = name;
            if (upgradeData.Count == 0) return;
            if (overWriteDescription) return;

            GenerateDescription();
        }

        private void GenerateDescription()
        {
            if (IsAbility)
            {
                switch (upgradeData[0].statsType)
                {
                    case StatTypes.abilitys:
                        SkillDescription = $"{SkillName} grants the ... ability";
                        break;
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"{SkillName} increases ");
                for (int i = 0; i < upgradeData.Count; i++)
                {
                    sb.Append(upgradeData[i].statsType.ToString());
                    sb.Append(" by ");
                    sb.Append(upgradeData[i].skillIncreaseAmount.ToString());
                    sb.Append(upgradeData[i].isPercentage ? "%" : " points(s)");
                    if (i == upgradeData.Count - 2) sb.Append(" and ");
                    sb.Append(i < upgradeData.Count - 1 ? ", " : ".");
                }

                SkillDescription = sb.ToString();
            }
        }
    }


    [System.Serializable]
    public class UpgradeData
    {
        public StatTypes statsType;
        public int skillIncreaseAmount;
        public bool isPercentage;
    }

    public enum StatTypes
    {
        damage,
        attack_speed,
        health,
        speed,
        abilitys,
    }
}