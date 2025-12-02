using System;

[Serializable]
public class Skill
{
    public SkillType Type;
    public int Level;
    public float CurrentXp;

    public Skill(SkillType type, int startingLevel)
    {
        Type = type;
        Level = startingLevel;
        CurrentXp = 0f;
    }
}
