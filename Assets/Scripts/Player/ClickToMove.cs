using UnityEngine;

namespace Runity.Gameplay.Player
{
    [RequireComponent(typeof(TickMover))]
    public class ClickToMove : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f;

        private TickMover tickMover;
        private Vector3? destination;

        public bool HasDestination => destination.HasValue || (tickMover != null && tickMover.PendingSteps > 0);

        private void Awake()
        {
            tickMover = GetComponent<TickMover>();
        }

        public void MoveTo(Vector3 worldPosition)
        {
            destination = new Vector3(worldPosition.x, transform.position.y, worldPosition.z);
            QueueStepsToDestination();
        }

        public void ClearDestination()
        {
            destination = null;
            tickMover?.ClearSteps();
        }

        private void LateUpdate()
        {
            FaceDestination();

            if (destination.HasValue && tickMover != null && tickMover.PendingSteps == 0)
            {
                // Snap to the exact destination once all queued tick steps are completed.
                transform.position = new Vector3(destination.Value.x, transform.position.y, destination.Value.z);
                destination = null;
            }
        }

        private void QueueStepsToDestination()
        {
            if (tickMover == null || !destination.HasValue)
            {
                return;
            }

            tickMover.ClearSteps();

            Vector3 target = destination.Value;
            Vector3 start = transform.position;
            Vector3 direction = new Vector3(target.x - start.x, 0f, target.z - start.z);

            float remaining = direction.magnitude;
            if (remaining < 0.01f)
            {
                destination = null;
                return;
            }

            Vector3 stepDirection = direction.normalized;
            while (remaining > 0f)
            {
                float step = Mathf.Min(remaining, tickMover.StepDistance);
                tickMover.EnqueueDelta(stepDirection * step);
                remaining -= step;
            }
        }

        private void FaceDestination()
        {
            if (!destination.HasValue)
            {
                return;
            }

            Vector3 target = destination.Value;
            Vector3 direction = new Vector3(target.x - transform.position.x, 0f, target.z - transform.position.z);
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
