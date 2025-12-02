using UnityEngine;

/// <summary>
/// Tick-bound tree resource that only changes state on ticks. When depleted it waits a configured number
/// of ticks before respawning, keeping gathering perfectly aligned with the global tick clock.
/// </summary>
[RequireComponent(typeof(TickHealth))]
public class TickTreeResource : TickBehaviour
{
    [SerializeField]
    private long respawnTicks = 10;

    private TickHealth _health;
    private bool _isRespawning;
    private long _respawnReadyTick;

    public bool IsAvailable => !_isRespawning && _health != null && !_health.IsDead;

    private void Awake()
    {
        _health = GetComponent<TickHealth>();
        _health.Died += HandleDepleted;
    }

    public void ApplyChop(int damage, long tick)
    {
        if (!IsAvailable)
        {
            return;
        }

        _health.TakeDamage(Mathf.Max(0, damage));

        if (_health.IsDead)
        {
            BeginRespawn(tick);
        }
    }

    protected override void OnTick(long tick)
    {
        if (_isRespawning && tick >= _respawnReadyTick)
        {
            _health.HealFull();
            _isRespawning = false;
        }
    }

    private void HandleDepleted()
    {
        // Safety hook in case other systems reduce health outside of ApplyChop.
        BeginRespawn(TickManager.CurrentTick);
    }

    private void BeginRespawn(long tick)
    {
        _isRespawning = true;
        _respawnReadyTick = tick + Mathf.Max(1, respawnTicks);
    }
}
