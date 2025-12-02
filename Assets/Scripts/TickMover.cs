using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves an actor strictly on the 0.6s tick cadence. Each tick will process one queued step so that
/// navigation, path following, or grid movement always aligns with the global tick timeline.
/// </summary>
public class TickMover : TickBehaviour
{
    [SerializeField]
    private float stepDistance = 1f;

    [SerializeField]
    private bool snapToGround = true;

    private readonly Queue<Vector3> _stepQueue = new();

    public int PendingSteps => _stepQueue.Count;

    public float StepDistance => stepDistance;

    public void ClearSteps() => _stepQueue.Clear();

    public void EnqueueStep(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            return;
        }

        _stepQueue.Enqueue(direction.normalized * stepDistance);
    }

    public void EnqueueDelta(Vector3 delta)
    {
        if (delta == Vector3.zero)
        {
            return;
        }

        _stepQueue.Enqueue(delta);
    }

    public void EnqueueSteps(IEnumerable<Vector3> directions)
    {
        if (directions == null)
        {
            return;
        }

        foreach (var dir in directions)
        {
            EnqueueStep(dir);
        }
    }

    protected override void OnTick(long tick)
    {
        if (_stepQueue.Count == 0)
        {
            return;
        }

        var delta = _stepQueue.Dequeue();
        var targetPosition = transform.position + delta;

        if (snapToGround && Physics.Raycast(targetPosition + Vector3.up, Vector3.down, out var hitInfo, 5f))
        {
            targetPosition.y = hitInfo.point.y;
        }

        transform.position = targetPosition;
    }
}
