using System.Collections.Generic;
using Runity.Gameplay.Player;
using UnityEngine;

namespace Runity.Gameplay.Interactions
{
    public class ChoppableTree : MonoBehaviour, IInteractable
    {
        [SerializeField] private int chopCount = 5;

        public string DisplayName => "Tree";

        public IEnumerable<ContextMenuAction> GetActions(PlayerInteractor interactor)
        {
            yield return new ContextMenuAction("Chop", () => Interact(interactor));
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (chopCount <= 0)
            {
                Debug.Log("The tree is depleted.");
                return;
            }

            chopCount--;
            Debug.Log($"Chopped the tree. {chopCount} swings remaining.");

            if (chopCount == 0)
            {
                gameObject.SetActive(false);
                Debug.Log("Tree has been chopped down.");
            }
        }
    }
}
