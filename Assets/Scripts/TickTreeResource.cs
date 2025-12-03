using System;
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

    [SerializeField]
    private int logsPerTree = 5;

    private TickHealth _health;
    private bool _isRespawning;
    private IDisposable _respawnHandle;

    public bool IsAvailable => !_isRespawning && _health != null && !_health.IsDead;

    private void Awake()
    {
        _health = GetComponent<TickHealth>();
        _health.SetMaxHealth(Mathf.Max(1, logsPerTree));
        _health.Died += HandleDepleted;
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.Died -= HandleDepleted;
        }

        _respawnHandle?.Dispose();
    }

    public void ApplyChop(int damage)
    {
        if (!IsAvailable)
        {
            return;
        }

        _health.TakeDamage(Mathf.Max(1, damage));
        Debug.Log("You gather a log.");

        if (_health.IsDead)
        {
            BeginRespawn(TickManager.CurrentTick);
        }
    }

    protected override void OnTick(long tick)
    {
        // Safety hook in case other systems reduce health outside of ApplyChop.
        if (!_isRespawning && _health != null && _health.IsDead)
        {
            BeginRespawn(tick);
        }
    }

    private void HandleDepleted()
    {
        // Safety hook in case other systems reduce health outside of ApplyChop.
        BeginRespawn(TickManager.CurrentTick);
    }

    private void BeginRespawn(long tick)
    {
        if (_isRespawning)
        {
            return;
        }

        _isRespawning = true;
        _respawnHandle?.Dispose();

        long delayTicks = Math.Max(1L, respawnTicks);

        gameObject.SetActive(false);
        _respawnHandle = TickManager.Scheduler.Schedule(_ => CompleteRespawn(), delayTicks);
    }

    private void CompleteRespawn()
    {
        _respawnHandle = null;
        _isRespawning = false;
        _health.HealFull();
        gameObject.SetActive(true);
    }
}
