using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.Skill_System
{
    [CreateAssetMenu(fileName = "New Skill Library", menuName = "Skill System/New Skill Library", order = 0)]

    public class ScritableSkillLibrary : ScriptableObject
    {
        public List<ScriptableSkill> SkillLibrary;

        public List<ScriptableSkill> GetSkillOfTier(int tier)
        {
            return SkillLibrary.Where(Skill => Skill.skillTier == tier).ToList();
        }
    }
}
