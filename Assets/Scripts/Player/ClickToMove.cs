using UnityEngine;

namespace Runity.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class ClickToMove : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private bool useGravity = false;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController characterController;
        private Vector3? destination;
        private Vector3 velocity;

        public bool HasDestination => destination.HasValue;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            ApplyGravity();
            MoveTowardsDestination();
        }

        public void MoveTo(Vector3 worldPosition)
        {
            destination = new Vector3(worldPosition.x, transform.position.y, worldPosition.z);
        }

        public void ClearDestination()
        {
            destination = null;
        }

        private void MoveTowardsDestination()
        {
            if (!destination.HasValue)
            {
                characterController.Move(velocity * Time.deltaTime);
                return;
            }

            Vector3 target = destination.Value;
            Vector3 direction = target - transform.position;
            Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);

            if (horizontalDirection.sqrMagnitude < 0.01f)
            {
                destination = null;
                characterController.Move(velocity * Time.deltaTime);
                return;
            }

            Vector3 move = horizontalDirection.normalized * moveSpeed;
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            characterController.Move((move + velocity) * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (!useGravity)
            {
                velocity = Vector3.zero;
                return;
            }

            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
        }
    }
}
