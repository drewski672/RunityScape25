using UnityEngine;

/// <summary>
/// Simple turn-based combat loop. Each combatant swings once per turn window and unlocks their target's next
/// attack window so turns alternate cleanly after the initiator strikes first.
/// </summary>
public class TickCombatant : TickBehaviour
{
    [SerializeField, Min(1)]
    private int turnLengthTicks = 1;

    [SerializeField]
    private TickCooldown _attackCooldown = new TickCooldown();

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

    protected override void OnDisable()
    {
        base.OnDisable();

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
        if ((tick % turnLengthTicks) != 0)
        {
            return;
        }

        if (!CanAct())
        {
            return;
        }

        if (!_attackCooldown.IsReady(tick))
        {
            return;
        }

        _attackCooldown.Start(tick);
        target.TakeDamage(attackDamage, this);
    }

    private bool CanAct()
    {
        return target != null && !target.IsDead;
    }
}
