using System.Collections.Generic;
using Runity.Gameplay.Interactions;
using Runity.Gameplay.UI;
using UnityEngine;

namespace Runity.Gameplay.Player
{
    [RequireComponent(typeof(ClickToMove))]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private LayerMask groundMask = -1;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ContextMenuUI contextMenu;

        private ClickToMove movement;

        public LayerMask GroundMask
        {
            get => groundMask;
            set => groundMask = value;
        }

        public ContextMenuUI ContextMenu
        {
            get => contextMenu;
            set => contextMenu = value;
        }

        public Camera MainCamera
        {
            get => mainCamera;
            set => mainCamera = value;
        }

        private void Awake()
        {
            movement = GetComponent<ClickToMove>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Start()
        {
            if (contextMenu == null)
            {
                contextMenu = FindObjectOfType<ContextMenuUI>();
            }
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }

            if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            if (!RaycastFromMouse(out RaycastHit hit))
            {
                return;
            }

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                contextMenu?.Hide();
                interactable.Interact(this);
                return;
            }

            if (IsGround(hit.collider.gameObject.layer))
            {
                movement.MoveTo(hit.point);
                contextMenu?.Hide();
            }
        }

        private void HandleRightClick()
        {
            if (contextMenu == null)
            {
                return;
            }

            if (!RaycastFromMouse(out RaycastHit hit))
            {
                return;
            }

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                return;
            }

            List<ContextMenuAction> actions = new List<ContextMenuAction>(interactable.GetActions(this));
            if (actions.Count == 0)
            {
                actions.Add(new ContextMenuAction("Interact", () => interactable.Interact(this)));
            }

            contextMenu.Show(Input.mousePosition, interactable.DisplayName, actions);
        }

        private bool RaycastFromMouse(out RaycastHit hit)
        {
            hit = default;
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    return false;
                }
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit, 500f);
        }

        private bool IsGround(int layer)
        {
            return (groundMask.value & (1 << layer)) != 0;
        }
    }
}
