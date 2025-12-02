using System.Collections.Generic;

namespace Runity.Gameplay.Interactions
{
    public struct ContextMenuAction
    {
        public string Label;
        public System.Action Callback;

        public ContextMenuAction(string label, System.Action callback)
        {
            Label = label;
            Callback = callback;
        }
    }

    public interface IInteractable
    {
        string DisplayName { get; }
        IEnumerable<ContextMenuAction> GetActions(Player.PlayerInteractor interactor);
        void Interact(Player.PlayerInteractor interactor);
    }
}
