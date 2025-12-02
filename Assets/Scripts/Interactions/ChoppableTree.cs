using System.Collections.Generic;
using Runity.Gameplay.Player;
using UnityEngine;

namespace Runity.Gameplay.Interactions
{
    [RequireComponent(typeof(TickTreeResource))]
    public class ChoppableTree : MonoBehaviour, IInteractable
    {
        private TickTreeResource tree;

        public string DisplayName => "Tree";

        private void Awake()
        {
            tree = GetComponent<TickTreeResource>();
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

            woodcutter.SetTarget(tree);
            Debug.Log("You start chopping the tree on the tick cadence.");
        }
    }
}
