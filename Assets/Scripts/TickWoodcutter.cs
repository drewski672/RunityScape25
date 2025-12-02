using UnityEngine;

/// <summary>
/// Tick-synchronized woodcutting loop. A swing is only attempted on ticks, respecting the configured
/// swing cooldown and targeting a tick-bound tree resource.
/// </summary>
public class TickWoodcutter : TickBehaviour
{
    [SerializeField]
    private TickCooldown swingCooldown = new TickCooldown();

    [SerializeField]
    private int chopDamage = 1;

    [SerializeField]
    private TickTreeResource targetTree;

    public void SetTarget(TickTreeResource tree)
    {
        targetTree = tree;
    }

    protected override void OnTick(long tick)
    {
        if (targetTree == null || !targetTree.IsAvailable)
        {
            return;
        }

        if (!swingCooldown.IsReady(tick))
        {
            return;
        }

        swingCooldown.Start(tick);
        targetTree.ApplyChop(chopDamage, tick);
    }
}
