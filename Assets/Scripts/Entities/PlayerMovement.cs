using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Grid Settings")]
    public float tileSize = 1f;
    public float moveHeight = 1f; // Y position to keep player at

    private CharacterController _controller;
    private Vector2Int _currentGrid;
    private Queue<Vector2Int> _path = new Queue<Vector2Int>();

    private bool _subscribedToTicks;

    // Smooth movement between tiles
    private bool _isMovingSegment;
    private Vector3 _segmentStartPos;
    private Vector3 _segmentEndPos;
    private float _segmentElapsed;
    private float _segmentDuration = 0.6f; // synced to TickManager

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        TrySubscribeToTicks();
    }

    private void Start()
    {
        _currentGrid = WorldToGrid(transform.position);
        SnapToGrid();
        TrySubscribeToTicks();

        if (TickManager.Instance != null)
        {
            _segmentDuration = TickManager.Instance.tickLength;
        }
    }

    private void OnDisable()
    {
        if (_subscribedToTicks && TickManager.Instance != null)
        {
            TickManager.Instance.OnTick -= HandleTick;
            _subscribedToTicks = false;
        }
    }

    private void TrySubscribeToTicks()
    {
        if (TickManager.Instance != null && !_subscribedToTicks)
        {
            TickManager.Instance.OnTick += HandleTick;
            _subscribedToTicks = true;
        }
    }

    private void Update()
    {
        // No input here – only smooth interpolation
        UpdateSmoothMovement();
    }

    private void HandleTick(long tick)
    {
        if (_path.Count > 0)
        {
            Vector2Int next = _path.Dequeue();
            _currentGrid = next;

            _segmentStartPos = transform.position;
            _segmentEndPos = GridToWorld(_currentGrid);
            _segmentElapsed = 0f;
            _isMovingSegment = true;
        }
    }

    private void UpdateSmoothMovement()
    {
        if (!_isMovingSegment)
            return;

        _segmentElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_segmentElapsed / _segmentDuration);
        Vector3 targetPos = Vector3.Lerp(_segmentStartPos, _segmentEndPos, t);
        targetPos.y = moveHeight;

        Vector3 delta = targetPos - transform.position;
        _controller.Move(delta);

        if (t >= 1f)
        {
            _controller.enabled = false;
            transform.position = _segmentEndPos;
            _controller.enabled = true;

            _isMovingSegment = false;
        }
    }

    // Called by PlayerInteraction – world-based destination
    public void SetDestination(Vector3 worldPos)
    {
        Vector2Int destGrid = WorldToGrid(worldPos);
        SetDestinationGrid(destGrid);
    }

    private void SetDestinationGrid(Vector2Int destGrid)
    {
        if (destGrid == _currentGrid)
            return;

        List<Vector2Int> pathList = BuildSimplePath(_currentGrid, destGrid);
        _path = new Queue<Vector2Int>(pathList);
        _isMovingSegment = false; // reset current movement segment
    }

    private void SnapToGrid()
    {
        Vector3 worldPos = GridToWorld(_currentGrid);
        _controller.enabled = false;
        transform.position = worldPos;
        _controller.enabled = true;
    }

    // Exposed for interaction code
    public Vector2Int WorldToGrid(Vector3 world)
    {
        int gx = Mathf.RoundToInt(world.x / tileSize);
        int gz = Mathf.RoundToInt(world.z / tileSize);
        return new Vector2Int(gx, gz);
    }

    public Vector3 GridToWorld(Vector2Int grid)
    {
        float x = grid.x * tileSize;
        float z = grid.y * tileSize;
        return new Vector3(x, moveHeight, z);
    }

    private List<Vector2Int> BuildSimplePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int current = start;
        int safety = 0;

        while (current != end && safety < 1000)
        {
            safety++;

            int dx = Mathf.Clamp(end.x - current.x, -1, 1);
            int dz = Mathf.Clamp(end.y - current.y, -1, 1);

            current += new Vector2Int(dx, dz);
            result.Add(current);
        }

        return result;
    }
}
