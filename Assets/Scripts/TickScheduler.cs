using System;
using System.Collections.Generic;

/// <summary>
/// Queues and runs actions on specific ticks so gameplay systems can be driven entirely by the 0.6s cadence.
/// Useful for turn progression, queued movement steps, attack timers, and resource respawns.
/// </summary>
public class TickScheduler
{
    private class ScheduledAction
    {
        public Action<long> Callback;
        public long ExecuteTick;
        public long RepeatInterval;
        public bool Cancelled;
    }

    private readonly List<ScheduledAction> _scheduled = new();

    /// <summary>
    /// Schedule an action to run after <paramref name="delayTicks"/> ticks. Optionally repeat it every
    /// <paramref name="repeatEveryTicks"/> ticks to keep systems like stamina drain or regen in sync.
    /// </summary>
    public IDisposable Schedule(Action<long> action, long delayTicks = 1, long repeatEveryTicks = 0)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var safeDelay = Math.Max(1, delayTicks);
        var startTick = TickManager.CurrentTick + safeDelay;

        var scheduled = new ScheduledAction
        {
            Callback = action,
            ExecuteTick = startTick,
            RepeatInterval = Math.Max(0, repeatEveryTicks)
        };

        _scheduled.Add(scheduled);
        return new ScheduledHandle(() => scheduled.Cancelled = true);
    }

    /// <summary>
    /// Called internally by <see cref="TickManager"/> once per global tick.
    /// </summary>
    internal void OnTick(long tick)
    {
        for (int i = _scheduled.Count - 1; i >= 0; i--)
        {
            var scheduled = _scheduled[i];

            if (scheduled.Cancelled)
            {
                _scheduled.RemoveAt(i);
                continue;
            }

            if (tick < scheduled.ExecuteTick)
            {
                continue;
            }

            scheduled.Callback?.Invoke(tick);

            if (scheduled.RepeatInterval > 0)
            {
                scheduled.ExecuteTick += scheduled.RepeatInterval;
            }
            else
            {
                _scheduled.RemoveAt(i);
            }
        }
    }

    private class ScheduledHandle : IDisposable
    {
        private Action _cancel;

        public ScheduledHandle(Action cancel)
        {
            _cancel = cancel;
        }

        public void Dispose()
        {
            _cancel?.Invoke();
            _cancel = null;
        }
    }
}
