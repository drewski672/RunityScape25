using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    private GameAction _currentAction;
    private bool _subscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // In case TickManager wasn't ready in OnEnable
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (_subscribed && TickManager.Instance != null)
        {
            TickManager.Instance.OnTick -= HandleTick;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (TickManager.Instance != null && !_subscribed)
        {
            TickManager.Instance.OnTick += HandleTick;
            _subscribed = true;
            Debug.Log("PlayerActionController subscribed to TickManager.");
        }
    }

    private void HandleTick(long tick)
    {
        if (_currentAction == null) return;

        _currentAction.OnTick(tick);
        if (_currentAction.IsComplete)
        {
            Debug.Log("Action completed.");
            _currentAction = null;
        }
    }

    public void StartAction(GameAction action)
    {
        _currentAction = action;
        var currentTick = TickManager.Instance != null ? TickManager.Instance.CurrentTick : 0;
        Debug.Log($"Starting action {action.GetType().Name} at tick {currentTick}.");
        action.Begin(currentTick);
    }

    public void CancelAction()
    {
        Debug.Log("Action cancelled.");
        _currentAction = null;
    }
}
