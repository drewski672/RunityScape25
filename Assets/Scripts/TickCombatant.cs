using UnityEngine;

/// <summary>
/// Simple turn-based combat loop. Each combatant swings once per turn window and unlocks their target's next
/// attack window so turns alternate cleanly after the initiator strikes first.
/// </summary>
public class TickCombatant : TickBehaviour
{
    private const int DefaultTurnLengthTicks = 4;

    [SerializeField]
    [Min(1)]
    private int turnLengthTicks = DefaultTurnLengthTicks;

    [SerializeField]
    [Min(1)]
    private int attackDamage = 1;

    [SerializeField]
    private TickHealth target;

    public bool HasActiveTarget => target != null && !target.IsDead;
    public TickHealth CurrentTarget => target;

    private long _nextAttackTick = long.MaxValue;
    private long _lastAttackTick = -1;
    private bool _waitForInitiator;

    public void SetTarget(TickHealth newTarget, bool takeFirstTurnImmediately = true)
    {
        if (target == newTarget)
        {
            return;
        }

        if (newTarget == null)
        {
            Debug.Log($"{name} clears their target.");
        }
        else
        {
            Debug.Log($"{name} targets {newTarget.name}.");
        }

        if (target != null)
        {
            target.Died -= HandleTargetDied;
        }

        target = newTarget;

        if (target != null)
        {
            target.Died += HandleTargetDied;
            _lastAttackTick = -1;
            _waitForInitiator = !takeFirstTurnImmediately;
            long startingTick = TickManager.CurrentTick;
            _nextAttackTick = startingTick + (takeFirstTurnImmediately ? 0 : turnLengthTicks);
        }
        else
        {
            _nextAttackTick = long.MaxValue;
            _waitForInitiator = false;
        }
    }

    private void OnDisable()
    {
        if (target != null)
        {
            target.Died -= HandleTargetDied;
        }
    }

    private void HandleTargetDied()
    {
        if (target != null)
        {
            target.Died -= HandleTargetDied;
            Debug.Log($"{name} stops fighting {target.name} because they died.");
        }

        target = null;
        _nextAttackTick = long.MaxValue;
        _waitForInitiator = false;
    }

    protected override void OnTick(long tick)
    {
        if (!CanAct(tick))
        {
            return;
        }

        PerformAttack(tick);
    }

    private bool CanAct(long tick)
    {
        return target != null && !_waitForInitiator && !target.IsDead && tick >= _nextAttackTick && _lastAttackTick != tick;
    }

    private void PerformAttack(long tick)
    {
        TickHealth targetHealth = target;
        if (targetHealth == null)
        {
            return;
        }

        _lastAttackTick = tick;
        _nextAttackTick = tick + turnLengthTicks;
        _waitForInitiator = false;
        targetHealth.TakeDamage(attackDamage);

        Debug.Log($"[Tick {tick}] {name} hits {targetHealth.name} for {attackDamage} damage ({targetHealth.CurrentHealth}/{targetHealth.MaxHealth} HP left).");

        if (target == null || target.IsDead)
        {
            return;
        }

        TickCombatant targetCombatant = target.GetComponent<TickCombatant>();
        targetCombatant?.NotifyAttacked(tick);
    }

    private void NotifyAttacked(long tick)
    {
        _waitForInitiator = false;
        long desiredNextAttack = tick + turnLengthTicks;
        if (desiredNextAttack > _nextAttackTick)
        {
            _nextAttackTick = desiredNextAttack;
        }
    }
}
