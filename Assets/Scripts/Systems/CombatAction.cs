using UnityEngine;

public class CombatAction : GameAction
{
    private readonly SkillSet _attackerSkills;
    private readonly CombatStats _attackerStats;
    private readonly Health _defenderHealth;
    private readonly Transform _attackerTransform;
    private readonly Transform _defenderTransform;
    private readonly float _attackRange;
    private readonly int _attackSpeedTicks;

    private long _nextHitTick;

    public CombatAction(
        SkillSet attackerSkills,
        CombatStats attackerStats,
        Health defenderHealth,
        Transform attackerTransform,
        Transform defenderTransform,
        float attackRange = 1.8f)
    {
        _attackerSkills = attackerSkills;
        _attackerStats = attackerStats;
        _defenderHealth = defenderHealth;
        _attackerTransform = attackerTransform;
        _defenderTransform = defenderTransform;
        _attackRange = attackRange;
        _attackSpeedTicks = attackerStats.attackSpeedTicks;
    }

    public override void Begin(long currentTick)
    {
        base.Begin(currentTick);
        _nextHitTick = currentTick; // first swing immediately
    }

    public override void OnTick(long currentTick)
    {
        if (IsComplete) return;

        if (_defenderHealth == null || _defenderHealth.IsDead)
        {
            IsComplete = true;
            return;
        }

        float distance = Vector3.Distance(
            new Vector3(_attackerTransform.position.x, 0, _attackerTransform.position.z),
            new Vector3(_defenderTransform.position.x, 0, _defenderTransform.position.z));

        if (distance > _attackRange)
        {
            // out of range – for now just end combat
            IsComplete = true;
            return;
        }

        if (currentTick >= _nextHitTick)
        {
            PerformHit();
            _nextHitTick = currentTick + _attackSpeedTicks;
        }
    }

    private void PerformHit()
    {
        // placeholder hit chance + damage
        bool isHit = Random.value < 0.75f;

        if (isHit)
        {
            int damage = Random.Range(0, _attackerStats.maxHit + 1);
            _defenderHealth.TakeDamage(damage);

            if (_attackerSkills != null && damage > 0)
            {
                _attackerSkills.AddXp(SkillType.Attack, damage * 4f);
                _attackerSkills.AddXp(SkillType.Strength, damage * 1.33f);
                _attackerSkills.AddXp(SkillType.Hitpoints, damage * 1.33f);
            }

            Debug.Log($"Hit for {damage}");
        }
        else
        {
            Debug.Log("Miss!");
        }
    }
}
