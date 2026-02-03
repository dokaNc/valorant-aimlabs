using UnityEngine;
using UnityEngine.InputSystem;

namespace ValorantAimTrainer.Gameplay
{
    /// <summary>
    /// Adds smooth weapon sway based on mouse input for realistic FPS feel.
    /// Attach to the weapon viewmodel GameObject.
    /// </summary>
    public class WeaponSway : MonoBehaviour
    {
        [Header("Sway Settings")]
        [Tooltip("Amount of sway applied to rotation")]
        [SerializeField] private float swayAmount = 2f;

        [Tooltip("Maximum rotation angle in degrees")]
        [SerializeField] private float maxSwayAmount = 5f;

        [Tooltip("How smoothly the weapon follows mouse movement")]
        [SerializeField] private float smoothness = 6f;

        [Header("Position Sway")]
        [Tooltip("Enable position-based sway in addition to rotation")]
        [SerializeField] private bool enablePositionSway = true;

        [Tooltip("Amount of position sway")]
        [SerializeField] private float positionSwayAmount = 0.01f;

        [Tooltip("Maximum position offset")]
        [SerializeField] private float maxPositionSway = 0.03f;

        [Header("Movement Bobbing")]
        [Tooltip("Enable bobbing when player moves")]
        [SerializeField] private bool enableBobbing = false;

        [Tooltip("Speed of the bobbing motion")]
        [SerializeField] private float bobSpeed = 10f;

        [Tooltip("Amount of vertical bobbing")]
        [SerializeField] private float bobAmount = 0.01f;

        // Cached references
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        // Sway state
        private Vector3 _targetRotation;
        private Vector3 _targetPosition;
        private float _bobTimer;

        private void Awake()
        {
            _initialPosition = transform.localPosition;
            _initialRotation = transform.localRotation;
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            CalculateSway();
            ApplySway();

            if (enableBobbing)
            {
                ApplyBobbing();
            }
        }

        private void CalculateSway()
        {
            // Get mouse delta
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Calculate rotation sway (inverted for natural feel)
            float swayX = Mathf.Clamp(-mouseDelta.y * swayAmount, -maxSwayAmount, maxSwayAmount);
            float swayY = Mathf.Clamp(-mouseDelta.x * swayAmount, -maxSwayAmount, maxSwayAmount);

            _targetRotation = new Vector3(swayX, swayY, 0f);

            // Calculate position sway if enabled
            if (enablePositionSway)
            {
                float posX = Mathf.Clamp(-mouseDelta.x * positionSwayAmount, -maxPositionSway, maxPositionSway);
                float posY = Mathf.Clamp(-mouseDelta.y * positionSwayAmount, -maxPositionSway, maxPositionSway);

                _targetPosition = new Vector3(posX, posY, 0f);
            }
        }

        private void ApplySway()
        {
            // Smooth rotation sway
            Quaternion targetRotation = _initialRotation * Quaternion.Euler(_targetRotation);
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * smoothness
            );

            // Smooth position sway
            if (enablePositionSway)
            {
                Vector3 targetPos = _initialPosition + _targetPosition;
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    targetPos,
                    Time.deltaTime * smoothness
                );
            }
        }

        private void ApplyBobbing()
        {
            // Check if player is moving (WASD keys)
            bool isMoving = false;

            if (Keyboard.current != null)
            {
                isMoving = Keyboard.current.wKey.isPressed ||
                           Keyboard.current.aKey.isPressed ||
                           Keyboard.current.sKey.isPressed ||
                           Keyboard.current.dKey.isPressed;
            }

            if (isMoving)
            {
                _bobTimer += Time.deltaTime * bobSpeed;

                float bobOffsetY = Mathf.Sin(_bobTimer) * bobAmount;
                float bobOffsetX = Mathf.Cos(_bobTimer * 0.5f) * bobAmount * 0.5f;

                Vector3 bobOffset = new Vector3(bobOffsetX, bobOffsetY, 0f);
                transform.localPosition += bobOffset;
            }
            else
            {
                // Reset bob timer when not moving
                _bobTimer = 0f;
            }
        }

        /// <summary>
        /// Reset sway to initial state (useful after weapon switch)
        /// </summary>
        public void ResetSway()
        {
            transform.localPosition = _initialPosition;
            transform.localRotation = _initialRotation;
            _targetRotation = Vector3.zero;
            _targetPosition = Vector3.zero;
            _bobTimer = 0f;
        }

        /// <summary>
        /// Temporarily increase sway (e.g., after firing)
        /// </summary>
        public void AddRecoilKick(float amount)
        {
            _targetRotation += new Vector3(-amount, Random.Range(-amount * 0.5f, amount * 0.5f), 0f);
        }
    }
}
