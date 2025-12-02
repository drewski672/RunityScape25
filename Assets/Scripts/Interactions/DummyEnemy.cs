using System;
using System.Collections.Generic;
using Runity.Gameplay.Player;
using UnityEngine;

namespace Runity.Gameplay.Interactions
{
    [RequireComponent(typeof(TickHealth))]
    public class DummyEnemy : MonoBehaviour, IInteractable
    {
        [SerializeField] private int respawnTicks = 5;

        private TickHealth health;
        private IDisposable respawnHandle;

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

            combatant.SetTarget(health);
            Debug.Log("You focus on attacking the training dummy.");
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
