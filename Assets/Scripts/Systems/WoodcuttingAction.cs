using UnityEngine;

public class WoodcuttingAction : GameAction
{
    private readonly SkillSet _skills;
    private readonly Inventory _inventory;
    private readonly TreeNode _tree;
    private readonly int _chopIntervalTicks;
    private readonly string _logItemId;

    private long _nextChopTick;

    public WoodcuttingAction(
        SkillSet skills,
        Inventory inventory,
        TreeNode tree,
        int chopIntervalTicks = 4,
        string logItemId = "Log")
    {
        _skills = skills;
        _inventory = inventory;
        _tree = tree;
        _chopIntervalTicks = chopIntervalTicks;
        _logItemId = logItemId;
    }

    public override void Begin(long currentTick)
    {
        base.Begin(currentTick);
        _nextChopTick = currentTick; // first chop tick
    }

    public override void OnTick(long currentTick)
    {
        if (IsComplete) return;
        if (_tree == null || _tree.IsDepleted)
        {
            IsComplete = true;
            return;
        }

        if (currentTick >= _nextChopTick)
        {
            AttemptChop();
            _nextChopTick = currentTick + _chopIntervalTicks;
        }
    }

    private void AttemptChop()
    {
        if (!_inventory.HasSpace)
        {
            Debug.Log("Inventory full, stopping woodcutting.");
            IsComplete = true;
            return;
        }

        // placeholder success
        bool success = Random.value < 0.9f;
        if (!success)
            return;

        _inventory.AddItem(_logItemId);

        if (_skills != null)
        {
            _skills.AddXp(SkillType.Woodcutting, _tree.xpPerLog);
        }

        _tree.hitsRemaining--;
        if (_tree.hitsRemaining <= 0)
        {
            _tree.Deplete();
            IsComplete = true;
        }
    }
}
