using System;
using UnityEngine;

/// <summary>
/// Global tick scheduler that mimics Old School RuneScape's 0.6 second tick cadence.
/// TickManager stays alive across scenes and exposes a simple event for gameplay systems
/// to subscribe to predictable, fixed-duration ticks independent of frame rate.
/// </summary>
public class TickManager : MonoBehaviour
{
    /// <summary>
    /// Length of a single tick in seconds (0.6s).
    /// </summary>
    public const float TickDurationSeconds = 0.6f;

    private static TickManager _instance;

    /// <summary>
    /// Provides global access to the singleton instance. The instance is lazily created
    /// if it does not exist yet.
    /// </summary>
    public static TickManager Instance
    {
        get
        {
            EnsureInstance();
            return _instance;
        }
    }

    /// <summary>
    /// Fired every time a new tick is processed. The long payload is the current tick count
    /// starting from 1.
    /// </summary>
    public static event Action<long> Tick;

    /// <summary>
    /// The number of ticks that have been processed since startup.
    /// </summary>
    public long TickCount { get; private set; }

    private float _accumulator;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() => EnsureInstance();

    private static void EnsureInstance()
    {
        if (_instance != null)
        {
            return;
        }

        var host = new GameObject("TickManager");
        _instance = host.AddComponent<TickManager>();
        DontDestroyOnLoad(host);
    }

    private void Update()
    {
        _accumulator += Time.deltaTime;

        while (_accumulator >= TickDurationSeconds)
        {
            _accumulator -= TickDurationSeconds;
            TickCount++;
            Tick?.Invoke(TickCount);
        }
    }

    /// <summary>
    /// Returns the amount of time remaining until the next tick fires.
    /// </summary>
    public float TimeUntilNextTick => Mathf.Max(TickDurationSeconds - _accumulator, 0f);

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
