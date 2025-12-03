using System.Collections;
using System.Collections.Generic;
using Runity.Gameplay.Player;
using UnityEngine;

namespace Runity.Gameplay.Interactions
{
    [RequireComponent(typeof(TickHealth))]
    [RequireComponent(typeof(TickCombatant))]
    public class NeutralEnemy : MonoBehaviour, IInteractable
    {
        private const float InteractionRange = 1f;

        [SerializeField] private string displayName = "Neutral Opponent";

        private TickHealth health;
        private TickCombatant combatant;
        private Coroutine pendingInteraction;

        public string DisplayName => displayName;

        private void Awake()
        {
            health = GetComponent<TickHealth>();
            combatant = GetComponent<TickCombatant>();
            health.Damaged += HandleDamaged;
            health.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            health.Damaged -= HandleDamaged;
            health.Died -= HandleDeath;
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
                Debug.Log($"The {DisplayName.ToLower()} has already been defeated.");
                return;
            }

            TickCombatant playerCombatant = interactor.GetComponent<TickCombatant>();
            if (playerCombatant == null)
            {
                Debug.LogWarning("Player has no TickCombatant to attack with.");
                return;
            }

            if (pendingInteraction != null)
            {
                StopCoroutine(pendingInteraction);
            }

            pendingInteraction = StartCoroutine(MoveIntoRangeAndAttack(interactor, playerCombatant));
        }

        private IEnumerator MoveIntoRangeAndAttack(PlayerInteractor interactor, TickCombatant playerCombatant)
        {
            Transform player = interactor.transform;
            ClickToMove mover = interactor.GetComponent<ClickToMove>();
            bool hasIssuedMove = false;

            if (mover == null)
            {
                Debug.LogWarning("Player has no ClickToMove component to approach the neutral opponent.");
                yield break;
            }

            while (player != null)
            {
                if (health.IsDead)
                {
                    Debug.Log($"The {DisplayName.ToLower()} has already been defeated.");
                    break;
                }

                if (IsWithinRange(player.position))
                {
                    playerCombatant.SetTarget(health);
                    Debug.Log($"You provoke the {DisplayName.ToLower()}.");
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
            Vector2 enemy2D = new Vector2(transform.position.x, transform.position.z);
            return Vector2.Distance(player2D, enemy2D) <= InteractionRange;
        }

        private Vector3 GetAdjacentDestination(Vector3 playerPosition)
        {
            Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);
            Vector2 enemy2D = new Vector2(transform.position.x, transform.position.z);

            Vector2 direction = player2D - enemy2D;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }
            else
            {
                direction.Normalize();
            }

            Vector2 target2D = enemy2D + direction * InteractionRange;
            return new Vector3(target2D.x, playerPosition.y, target2D.y);
        }

        private void HandleDamaged(int damage, TickCombatant attacker)
        {
            if (attacker == null)
            {
                return;
            }

            TickHealth attackerHealth = attacker.GetComponent<TickHealth>();
            if (attackerHealth == null || attackerHealth.IsDead)
            {
                return;
            }

            combatant.SetTarget(attackerHealth);
            Debug.Log($"The {DisplayName.ToLower()} retaliates!");
        }

        private void HandleDeath()
        {
            combatant.SetTarget(null);
            Debug.Log($"The {DisplayName.ToLower()} is defeated.");
        }
    }
}
