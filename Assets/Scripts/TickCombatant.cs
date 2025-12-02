using UnityEngine;

/// <summary>
/// Simple tick-driven combat loop. Attacks only resolve on ticks using the configured cooldown so that
/// turn-based combat stays deterministic alongside other tick-bound systems.
/// </summary>
public class TickCombatant : TickBehaviour
{
    [SerializeField]
    private TickCooldown attackCooldown = new TickCooldown();

    [SerializeField]
    private int attackDamage = 5;

    [SerializeField]
    private TickHealth target;

    public void SetTarget(TickHealth newTarget)
    {
        if (target == newTarget)
        {
            return;
        }

        if (target != null)
        {
            target.Died -= HandleTargetDied;
        }

        target = newTarget;

        if (target != null)
        {
            target.Died += HandleTargetDied;
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
        }

        target = null;
    }

    protected override void OnTick(long tick)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

        if (!attackCooldown.IsReady(tick))
        {
            return;
        }

        attackCooldown.Start(tick);
        target.TakeDamage(attackDamage);
    }
}
