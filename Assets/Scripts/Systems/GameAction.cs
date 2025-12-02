public abstract class GameAction
{
    public bool IsComplete { get; protected set; }

    public virtual void Begin(long currentTick) { }
    public virtual void OnTick(long currentTick) { }
}
