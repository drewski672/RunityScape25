using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerActionController))]
[RequireComponent(typeof(SkillSet))]
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    public LayerMask groundMask;
    public LayerMask interactMask;

    private PlayerActionController _actionController;
    private SkillSet _skills;
    private Inventory _inventory;
    private PlayerMovement _movement;

    private void Awake()
    {
        _actionController = GetComponent<PlayerActionController>();
        _skills = GetComponent<SkillSet>();
        _inventory = GetComponent<Inventory>();
        _movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    // ---------------- LEFT CLICK (primary: walk / attack / chop) ----------------

    private void HandleLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundMask | interactMask;

        if (!Physics.Raycast(ray, out var hit, 200f, mask))
            return;

        GameObject target = hit.collider.gameObject;

        // First, see if it's an interactable (NPC / Tree)
        if (((1 << target.layer) & interactMask) != 0)
        {
            Vector2Int playerGrid = _movement.WorldToGrid(transform.position);
            Vector2Int targetGrid = _movement.WorldToGrid(target.transform.position);
            Vector2Int stopGrid = GetAdjacentTileNear(playerGrid, targetGrid);
            Vector3 stopWorld = _movement.GridToWorld(stopGrid);

            // NPC combat?
            if (target.TryGetComponent<Health>(out var npcHealth) &&
                target.TryGetComponent<CombatStats>(out var npcStats) &&
                target != gameObject)
            {
                _movement.SetDestination(stopWorld);

                var combatAction = new CombatAction(
                    _skills,
                    GetComponent<CombatStats>(),
                    npcHealth,
                    transform,
                    target.transform,
                    attackRange: 1.1f
                );

                _actionController.StartAction(combatAction);
                return;
            }

            // Tree (woodcut)
            if (target.TryGetComponent<TreeNode>(out var tree))
            {
                _movement.SetDestination(stopWorld);

                var wcAction = new WoodcuttingAction(
                    _skills,
                    _inventory,
                    tree
                );

                _actionController.StartAction(wcAction);
                return;
            }
        }

        // If it's not an interactable, or interact checks didn't handle it, walk to ground
        if (((1 << target.layer) & groundMask) != 0)
        {
            _movement.SetDestination(hit.point);
        }
    }

    // ---------------- RIGHT CLICK (context menu) ----------------

    private void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundMask | interactMask;

        if (!Physics.Raycast(ray, out var hit, 200f, mask))
        {
            // No hit – just hide menu if open
            ContextMenuUI.Instance?.Hide();
            return;
        }

        GameObject target = hit.collider.gameObject;
        Vector2Int playerGrid = _movement.WorldToGrid(transform.position);
        Vector2Int targetGrid = _movement.WorldToGrid(target.transform.position);
        Vector2Int stopGrid = GetAdjacentTileNear(playerGrid, targetGrid);
        Vector3 stopWorld = _movement.GridToWorld(stopGrid);

        var options = new List<(string label, Action action)>();

        // Ground-only right-click
        if (((1 << target.layer) & groundMask) != 0)
        {
            options.Add(("Walk here", () =>
            {
                _movement.SetDestination(hit.point);
            }
            ));

            options.Add(("Examine ground", () =>
            {
                Debug.Log("You see nothing special.");
            }
            ));
        }
        // Interactable right-click (NPC / Tree)
        else if (((1 << target.layer) & interactMask) != 0)
        {
            // Always have Walk here
            options.Add(("Walk here", () =>
            {
                _movement.SetDestination(stopWorld);
            }
            ));

            bool isNpc = target.TryGetComponent<Health>(out var npcHealth) &&
                         target.TryGetComponent<CombatStats>(out var npcStats) &&
                         target != gameObject;

            bool isTree = target.TryGetComponent<TreeNode>(out var tree);

            if (isNpc)
            {
                string name = target.name;
                options.Add(($"Attack {name}", () =>
                {
                    _movement.SetDestination(stopWorld);
                    var combatAction = new CombatAction(
                        _skills,
                        GetComponent<CombatStats>(),
                        npcHealth,
                        transform,
                        target.transform,
                        attackRange: 1.1f
                    );
                    _actionController.StartAction(combatAction);
                }
                ));

                options.Add(($"Examine {name}", () =>
                {
                    Debug.Log($"It's {name}. Looks aggressive.");
                }
                ));
            }
            else if (isTree)
            {
                string name = target.name;
                options.Add(($"Chop down {name}", () =>
                {
                    _movement.SetDestination(stopWorld);
                    var wcAction = new WoodcuttingAction(
                        _skills,
                        _inventory,
                        tree
                    );
                    _actionController.StartAction(wcAction);
                }
                ));

                options.Add(($"Examine {name}", () =>
                {
                    Debug.Log($"It's a {name}. It might contain some good logs.");
                }
                ));
            }
            else
            {
                // Generic examine
                string name = target.name;
                options.Add(($"Examine {name}", () =>
                {
                    Debug.Log($"You see {name}. Nothing special.");
                }
                ));
            }
        }

        if (options.Count > 0 && ContextMenuUI.Instance != null)
        {
            ContextMenuUI.Instance.Show(Input.mousePosition, options);
        }
    }

    // ---------------- Grid helper ----------------

    private Vector2Int GetAdjacentTileNear(Vector2Int playerGrid, Vector2Int targetGrid)
    {
        Vector2Int[] neighbors =
        {
            targetGrid + new Vector2Int( 1,  0),
            targetGrid + new Vector2Int(-1,  0),
            targetGrid + new Vector2Int( 0,  1),
            targetGrid + new Vector2Int( 0, -1)
        };

        Vector2Int best = neighbors[0];
        float bestDist = float.MaxValue;

        foreach (var n in neighbors)
        {
            float dist = (n - playerGrid).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = n;
            }
        }

        return best;
    }
}
