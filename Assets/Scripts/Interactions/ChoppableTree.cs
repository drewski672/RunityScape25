using System.Collections;
using System.Collections.Generic;
using Runity.Gameplay.Player;
using UnityEngine;

namespace Runity.Gameplay.Interactions
{
    [RequireComponent(typeof(TickTreeResource))]
    public class ChoppableTree : MonoBehaviour, IInteractable
    {
        private const float InteractionRange = 1f;

        private TickTreeResource tree;
        private Coroutine pendingInteraction;

        public string DisplayName => "Tree";

        private void Awake()
        {
            tree = GetComponent<TickTreeResource>();
        }

        private void OnDestroy()
        {
            if (pendingInteraction != null)
            {
                StopCoroutine(pendingInteraction);
                pendingInteraction = null;
            }
        }

        public IEnumerable<ContextMenuAction> GetActions(PlayerInteractor interactor)
        {
            yield return new ContextMenuAction("Chop", () => Interact(interactor));
        }

        public void Interact(PlayerInteractor interactor)
        {
            TickWoodcutter woodcutter = interactor.GetComponent<TickWoodcutter>();
            if (woodcutter == null)
            {
                Debug.LogWarning("Player has no TickWoodcutter component to chop with.");
                return;
            }

            if (pendingInteraction != null)
            {
                StopCoroutine(pendingInteraction);
            }

            pendingInteraction = StartCoroutine(MoveIntoRangeAndChop(interactor, woodcutter));
        }

        private IEnumerator MoveIntoRangeAndChop(PlayerInteractor interactor, TickWoodcutter woodcutter)
        {
            Transform player = interactor.transform;
            ClickToMove mover = interactor.GetComponent<ClickToMove>();
            bool hasIssuedMove = false;

            if (mover == null)
            {
                Debug.LogWarning("Player has no ClickToMove component to approach the tree.");
                yield break;
            }

            while (player != null)
            {
                if (IsWithinRange(player.position))
                {
                    woodcutter.SetTarget(tree);
                    Debug.Log("You start chopping the tree on the tick cadence.");
                    break;
                }

                if (!hasIssuedMove)
                {
                    mover.MoveTo(GetAdjacentDestination(player.position));
                    hasIssuedMove = true;
                }

                yield return null;
            }

            pendingInteraction = null;
        }

        private bool IsWithinRange(Vector3 playerPosition)
        {
            Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);
            Vector2 tree2D = new Vector2(transform.position.x, transform.position.z);
            return Vector2.Distance(player2D, tree2D) <= InteractionRange;
        }

        private Vector3 GetAdjacentDestination(Vector3 playerPosition)
        {
            Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);
            Vector2 tree2D = new Vector2(transform.position.x, transform.position.z);

            Vector2 direction = player2D - tree2D;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }
            else
            {
                direction.Normalize();
            }

            Vector2 target2D = tree2D + direction * InteractionRange;
            return new Vector3(target2D.x, playerPosition.y, target2D.y);
        }
    }
}
