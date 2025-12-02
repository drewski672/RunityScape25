using System;
using UnityEngine;

/// <summary>
/// Simple health container for tick-driven actors. Keeps combat and resource components lightweight
/// while still exposing events for UI or other gameplay hooks.
/// </summary>
public class TickHealth : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 10;

    private int _currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;

    public event Action<int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        _currentHealth = Mathf.Max(1, maxHealth);
    }

    public void HealFull()
    {
        _currentHealth = maxHealth;
        HealthChanged?.Invoke(_currentHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead)
        {
            return;
        }

        _currentHealth = Mathf.Max(0, _currentHealth - Mathf.Max(0, amount));
        HealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            Died?.Invoke();
        }
    }
}
