using System;
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

    [SerializeField]
    private float additionalGroundOffset = 0.05f;

    private float _groundOffset;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _interpolationTime;
    private bool _isInterpolating;

    public int PendingSteps => _stepQueue.Count;

    public float StepDistance => stepDistance;

    public bool IsInterpolating => _isInterpolating;

    public void ClearSteps()
    {
        _stepQueue.Clear();
        _isInterpolating = false;
        _interpolationTime = 0f;
        _startPosition = transform.position;
        _targetPosition = transform.position;
    }

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

    private void Awake()
    {
        CacheGroundOffset();
        _startPosition = transform.position;
        _targetPosition = transform.position;
    }

    private void OnValidate()
    {
        CacheGroundOffset();
    }

    private void Update()
    {
        if (_isInterpolating)
        {
            _interpolationTime += Time.deltaTime;
            float t = Mathf.Clamp01(_interpolationTime / TickManager.TickDurationSeconds);
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);

            if (t >= 1f)
            {
                _isInterpolating = false;
            }
        }
        else if (snapToGround)
        {
            if (TryGetGroundedPosition(transform.position, out var grounded))
            {
                transform.position = grounded;
            }
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

        if (snapToGround && TryGetGroundedPosition(targetPosition, out var grounded))
        {
            targetPosition = grounded;
        }

        _startPosition = transform.position;
        _targetPosition = targetPosition;
        _interpolationTime = 0f;
        _isInterpolating = true;
    }

    private bool TryGetGroundedPosition(Vector3 position, out Vector3 groundedPosition)
    {
        // Cast from above the actor and ignore any hits on the actor itself so we snap to terrain.
        float rayHeight = Math.Max(_groundOffset + 0.5f, 1f);
        float rayLength = rayHeight + 5f;
        Vector3 origin = position + Vector3.up * rayHeight;

        var hits = Physics.RaycastAll(origin, Vector3.down, rayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.transform == transform)
                {
                    continue;
                }

                groundedPosition = new Vector3(position.x, hit.point.y + _groundOffset, position.z);
                return true;
            }
        }

        groundedPosition = position;
        return false;
    }

    private void CacheGroundOffset()
    {
        _groundOffset = additionalGroundOffset;

        if (TryGetComponent<Collider>(out var collider))
        {
            _groundOffset += collider.bounds.extents.y;
        }
    }
}
