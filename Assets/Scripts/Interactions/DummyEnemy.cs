using System.Collections.Generic;
using Runity.Gameplay.Player;
using UnityEngine;

namespace Runity.Gameplay.Interactions
{
    public class DummyEnemy : MonoBehaviour, IInteractable
    {
        [SerializeField] private int health = 10;

        public string DisplayName => "Training Dummy";

        public IEnumerable<ContextMenuAction> GetActions(PlayerInteractor interactor)
        {
            yield return new ContextMenuAction("Attack", () => Interact(interactor));
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (health <= 0)
            {
                Debug.Log("The dummy has already been defeated.");
                return;
            }

            health -= 2;
            Debug.Log($"You strike the dummy. Remaining health: {Mathf.Max(health, 0)}");

            if (health <= 0)
            {
                Debug.Log("Dummy defeated! Respawning in a moment...");
                Invoke(nameof(Respawn), 3f);
                gameObject.SetActive(false);
            }
        }

        private void Respawn()
        {
            health = 10;
            gameObject.SetActive(true);
            Debug.Log("Dummy respawned.");
        }
    }
}
