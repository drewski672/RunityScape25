using System;
using UnityEngine;

/// <summary>
/// Handles single woodcutting attempts that are aligned to the global tick clock. Each click starts a
/// one-off swing that resolves after the configured number of ticks, requiring another interaction for
/// subsequent chops.
/// </summary>
public class TickWoodcutter : TickBehaviour
{
    [SerializeField]
    private long chopDurationTicks = 4;

    [SerializeField]
    private int chopDamage = 1;

    [SerializeField]
    private TickTreeResource targetTree;

    private bool _isChopping;
    private IDisposable _chopHandle;

    public void SetTarget(TickTreeResource tree)
    {
        if (_isChopping)
        {
            return;
        }

        targetTree = tree;
        TryStartChop();
    }

    private void TryStartChop()
    {
        if (targetTree == null)
        {
            targetTree = null;
            return;
        }

        _isChopping = true;
        Debug.Log("You swing your axe at the tree.");

        long delay = Math.Max(1, chopDurationTicks);
        _chopHandle?.Dispose();
        _chopHandle = TickManager.Scheduler.Schedule(_ => CompleteChop(), delay);
    }

    private void CancelChop()
    {
        _chopHandle?.Dispose();
        _chopHandle = null;
        _isChopping = false;
        targetTree = null;
    }

    private void CompleteChop()
    {
        _chopHandle = null;

        if (targetTree != null && targetTree.IsAvailable)
        {
            targetTree.ApplyChop(chopDamage);
        }

        _isChopping = false;
        targetTree = null;
    }

    protected override void OnTick(long tick)
    {
        if (!_isChopping)
        {
            return;
        }

        if (targetTree == null || !targetTree.IsAvailable)
        {
            CancelChop();
        }
    }

    private void OnDestroy()
    {
        _chopHandle?.Dispose();
    }
}
