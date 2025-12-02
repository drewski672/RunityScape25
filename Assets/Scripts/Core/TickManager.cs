using UnityEngine;
using System;

public class TickManager : MonoBehaviour
{
    public static TickManager Instance { get; private set; }

    [Tooltip("Length of a game tick in seconds. RuneScape uses ~0.6s.")]
    public float tickLength = 0.6f;

    private float _timeAccumulator;
    public long CurrentTick { get; private set; }

    public event Action<long> OnTick;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        _timeAccumulator += Time.deltaTime;

        while (_timeAccumulator >= tickLength)
        {
            _timeAccumulator -= tickLength;
            CurrentTick++;
            OnTick?.Invoke(CurrentTick);
        }
    }
}
