using System;
using System.Collections;
using System.Collections.Generic;
using Runity.Gameplay.Player;
using UnityEngine;

namespace Runity.Gameplay.Interactions
{
    [RequireComponent(typeof(TickHealth))]
    public class DummyEnemy : MonoBehaviour, IInteractable
    {
        private const float InteractionRange = 1.5f;

        [SerializeField] private int respawnTicks = 5;

        private TickHealth health;
        private IDisposable respawnHandle;
        private Coroutine pendingInteraction;

        public string DisplayName => "Training Dummy";

        private void Awake()
        {
            health = GetComponent<TickHealth>();
            health.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            health.Died -= HandleDeath;
            respawnHandle?.Dispose();
            if (pendingInteraction != null)
            {
                StopCoroutine(pendingInteraction);
                pendingInteraction = null;
            }
        }

        public IEnumerable<ContextMenuAction> GetActions(PlayerInteractor interactor)
        {
            yield return new ContextMenuAction("Attack", () => Interact(interactor));
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (health.IsDead)
            {
                Debug.Log("The dummy has already been defeated.");
                return;
            }

            TickCombatant combatant = interactor.GetComponent<TickCombatant>();
            if (combatant == null)
            {
                Debug.LogWarning("Player has no TickCombatant to attack with.");
                return;
            }

            if (pendingInteraction != null)
            {
                StopCoroutine(pendingInteraction);
            }

            pendingInteraction = StartCoroutine(MoveIntoRangeAndAttack(interactor, combatant));
        }

        private IEnumerator MoveIntoRangeAndAttack(PlayerInteractor interactor, TickCombatant combatant)
        {
            Transform player = interactor.transform;
            ClickToMove mover = interactor.GetComponent<ClickToMove>();
            bool hasIssuedMove = false;

            if (mover == null)
            {
                Debug.LogWarning("Player has no ClickToMove component to approach the dummy.");
                yield break;
            }

            while (player != null)
            {
                if (health.IsDead)
                {
                    Debug.Log("The dummy has already been defeated.");
                    break;
                }

                if (IsWithinRange(player.position))
                {
                    combatant.SetTarget(health);
                    Debug.Log("You focus on attacking the training dummy.");
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
            Vector2 dummy2D = new Vector2(transform.position.x, transform.position.z);
            return Vector2.Distance(player2D, dummy2D) <= InteractionRange;
        }

        private Vector3 GetAdjacentDestination(Vector3 playerPosition)
        {
            Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);
            Vector2 dummy2D = new Vector2(transform.position.x, transform.position.z);

            Vector2 direction = player2D - dummy2D;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }
            else
            {
                direction.Normalize();
            }

            Vector2 target2D = dummy2D + direction * InteractionRange;
            return new Vector3(target2D.x, playerPosition.y, target2D.y);
        }

        private void Respawn()
        {
            respawnHandle = null;
            health.HealFull();
            gameObject.SetActive(true);
            Debug.Log("Dummy respawned.");
        }

        private void HandleDeath()
        {
            Debug.Log("Dummy defeated! Respawning in a moment...");
            gameObject.SetActive(false);
            respawnHandle = TickManager.Scheduler.Schedule(_ => Respawn(), respawnTicks);
        }
    }
}
