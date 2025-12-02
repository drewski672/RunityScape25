using UnityEngine;

/// <summary>
/// Base behaviour that automatically hooks into the <see cref="TickManager"/> tick stream.
/// Inherit from this class for any gameplay component that should run logic on the 0.6s tick cadence.
/// </summary>
public abstract class TickBehaviour : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        TickManager.Tick += HandleTick;
    }

    protected virtual void OnDisable()
    {
        TickManager.Tick -= HandleTick;
    }

    private void HandleTick(long tick)
    {
        OnTick(tick);
    }

    /// <summary>
    /// Called every tick (0.6 seconds) for deterministic game play systems such as movement, combat,
    /// harvesting, crafting, and timers.
    /// </summary>
    /// <param name="tick">Current global tick count starting at 1.</param>
    protected abstract void OnTick(long tick);
}
