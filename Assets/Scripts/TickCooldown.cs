using System;
using UnityEngine;

/// <summary>
/// Utility for turning frame-based cooldowns into tick-based windows. Useful for actions like
/// stepping, combat swings, harvesting trees, or crafting where you want predictable cadence.
/// </summary>
[System.Serializable]
public class TickCooldown
{
    [Tooltip("Number of ticks required before the action becomes available again.")]
    [Min(1)]
    [SerializeField]
    private long cooldownTicks = 1;

    private long _nextReadyTick;

    /// <summary>
    /// Returns true if the cooldown has completed for the provided <paramref name="currentTick"/>.
    /// </summary>
    public bool IsReady(long currentTick) => currentTick >= _nextReadyTick;

    /// <summary>
    /// Begin the cooldown window starting at <paramref name="currentTick"/>.
    /// </summary>
    public void Start(long currentTick)
    {
        _nextReadyTick = currentTick + Math.Max(1L, cooldownTicks);
    }

    /// <summary>
    /// Rewinds the cooldown so that the action is immediately available on the next tick.
    /// </summary>
    public void Reset()
    {
        _nextReadyTick = TickManager.CurrentTick;
    }

    /// <summary>
    /// How many ticks remain until the cooldown completes.
    /// </summary>
    public long TicksRemaining(long currentTick)
    {
        return Math.Max(0, _nextReadyTick - currentTick);
    }

    /// <summary>
    /// Seconds remaining before the cooldown completes, derived from the global tick duration.
    /// </summary>
    public float SecondsRemaining(long currentTick)
    {
        return TicksRemaining(currentTick) * TickManager.TickDurationSeconds;
    }
}
