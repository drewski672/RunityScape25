using UnityEngine;

namespace Runity.Gameplay.Player
{
    public class CameraOrbit : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 12f;
        [SerializeField] private float minDistance = 6f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float yawSpeed = 120f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float pitch = 45f;

        private float yaw;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput()
        {
            float yawInput = 0f;
            if (Input.GetKey(KeyCode.D)) yawInput += 1f;
            if (Input.GetKey(KeyCode.A)) yawInput -= 1f;
            yaw += yawInput * yawSpeed * Time.deltaTime;

            float zoomInput = 0f;
            if (Input.GetKey(KeyCode.S)) zoomInput += 1f;
            if (Input.GetKey(KeyCode.W)) zoomInput -= 1f;
            distance = Mathf.Clamp(distance + zoomInput * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
        }

        private void UpdateCameraPosition()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 direction = rotation * Vector3.forward;
            Vector3 desiredPosition = target.position - direction * distance;

            transform.position = desiredPosition;
            transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        }
    }
}
