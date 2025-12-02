using UnityEngine;

namespace Runity.Gameplay.Player
{
    [RequireComponent(typeof(TickMover))]
    public class ClickToMove : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gridSize = 1f;

        private TickMover tickMover;
        private Vector3? destination;

        public bool HasDestination => destination.HasValue || (tickMover != null && tickMover.PendingSteps > 0);

        private void Awake()
        {
            tickMover = GetComponent<TickMover>();
        }

        public void MoveTo(Vector3 worldPosition)
        {
            destination = SnapToGrid(new Vector3(worldPosition.x, transform.position.y, worldPosition.z));
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

            if (destination.HasValue && tickMover != null && tickMover.PendingSteps == 0 && !tickMover.IsInterpolating)
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

            float stepSize = GridSize();
            Vector3 start = SnapToGrid(transform.position);
            if (transform.position != start)
            {
                transform.position = start;
            }

            Vector3 target = destination.Value;
            Vector3 delta = new Vector3(target.x - start.x, 0f, target.z - start.z);

            int stepsX = Mathf.RoundToInt(delta.x / stepSize);
            int stepsZ = Mathf.RoundToInt(delta.z / stepSize);

            if (stepsX == 0 && stepsZ == 0)
            {
                destination = null;
                return;
            }

            int diagonalSteps = Mathf.Min(Mathf.Abs(stepsX), Mathf.Abs(stepsZ));
            int remainingStepsX = Mathf.Abs(stepsX) - diagonalSteps;
            int remainingStepsZ = Mathf.Abs(stepsZ) - diagonalSteps;

            if (diagonalSteps > 0)
            {
                Vector3 diagonalDelta = new Vector3(Mathf.Sign(stepsX) * stepSize, 0f, Mathf.Sign(stepsZ) * stepSize);
                for (int i = 0; i < diagonalSteps; i++)
                {
                    tickMover.EnqueueDelta(diagonalDelta);
                }
            }

            if (remainingStepsX > 0)
            {
                Vector3 stepX = new Vector3(Mathf.Sign(stepsX) * stepSize, 0f, 0f);
                for (int i = 0; i < remainingStepsX; i++)
                {
                    tickMover.EnqueueDelta(stepX);
                }
            }

            if (remainingStepsZ > 0)
            {
                Vector3 stepZ = new Vector3(0f, 0f, Mathf.Sign(stepsZ) * stepSize);
                for (int i = 0; i < remainingStepsZ; i++)
                {
                    tickMover.EnqueueDelta(stepZ);
                }
            }
        }

        private void FaceDestination()
        {
            if (!destination.HasValue)
            {
                return;
            }

            Vector3 target = destination.Value;
            Vector3 toTarget = new Vector3(target.x - transform.position.x, 0f, target.z - transform.position.z);
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            float stepSize = GridSize();
            float snappedX = Mathf.Round(position.x / stepSize) * stepSize;
            float snappedZ = Mathf.Round(position.z / stepSize) * stepSize;

            return new Vector3(snappedX, position.y, snappedZ);
        }

        private float GridSize()
        {
            return tickMover != null ? tickMover.StepDistance : gridSize;
        }
    }
}
