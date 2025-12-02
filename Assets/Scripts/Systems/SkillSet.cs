using System.Collections.Generic;
using UnityEngine;

public class SkillSet : MonoBehaviour
{
    public Dictionary<SkillType, Skill> Skills { get; private set; } =
        new Dictionary<SkillType, Skill>();

    private void Awake()
    {
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        AddSkill(SkillType.Attack, 1);
        AddSkill(SkillType.Strength, 1);
        AddSkill(SkillType.Defence, 1);
        AddSkill(SkillType.Hitpoints, 10);
        AddSkill(SkillType.Woodcutting, 1);
    }

    private void AddSkill(SkillType type, int startingLevel)
    {
        Skills[type] = new Skill(type, startingLevel);
    }

    public void AddXp(SkillType type, float amount)
    {
        if (!Skills.TryGetValue(type, out var skill))
            return;

        skill.CurrentXp += amount;
        int newLevel = XpToLevel(skill.CurrentXp);

        if (newLevel > skill.Level)
        {
            skill.Level = newLevel;
            Debug.Log($"[{type}] leveled up to {newLevel}!");
        }
    }

    // RuneScape-like XP curve
    public static int XpToLevel(float xp)
    {
        int level = 1;
        float targetXp = 0;

        for (int lvl = 2; lvl <= 99; lvl++)
        {
            targetXp += Mathf.Floor(lvl + 300f * Mathf.Pow(2f, lvl / 7f));
            float levelXp = Mathf.Floor(targetXp / 4f);

            if (xp < levelXp)
                return level;

            level = lvl;
        }

        return 99;
    }
}
