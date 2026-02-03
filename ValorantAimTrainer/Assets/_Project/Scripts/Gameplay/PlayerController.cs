using UnityEngine;
using UnityEngine.InputSystem;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;

namespace ValorantAimTrainer.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private KeybindSettings keybindSettings;

        [Header("Mouse Look (Fallback if no GameSettings)")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 89f;
        [SerializeField] private bool invertY = false;

        [Header("Movement")]
        [SerializeField] private bool enableMovement = true;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Camera playerCamera;

        // Components
        private CharacterController _characterController;

        // Mouse look state
        private float _verticalRotation;
        private bool _isControlEnabled = true;

        // Movement state
        private Vector3 _velocity;
        private bool _isGrounded;
        private bool _isCrouching;
        private bool _isWalking;
        private float _currentHeight;
        private float _targetHeight;
        private float _currentCameraY;
        private float _targetCameraY;

        // Spawn position for reset
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private bool _movementFrozen; // Freeze movement during countdown

        // Debug: track total rotation for calibration
        [Header("Debug (Runtime)")]
        [SerializeField] private float _debugTotalYaw = 0f;
        [SerializeField] private bool _showDebugInfo = false;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }

            // Configure CharacterController for VALORANT feel
            _characterController.height = GameSettings.STAND_HEIGHT;
            _characterController.radius = 0.3f;
            _characterController.center = new Vector3(0, GameSettings.STAND_HEIGHT / 2f, 0);
            _characterController.skinWidth = 0.08f;
            _characterController.stepOffset = 0.4f;
            _characterController.slopeLimit = 45f;

            _currentHeight = GameSettings.STAND_HEIGHT;
            _targetHeight = GameSettings.STAND_HEIGHT;
            _currentCameraY = GameSettings.CAMERA_HEIGHT_STAND;
            _targetCameraY = GameSettings.CAMERA_HEIGHT_STAND;
        }

        private void Start()
        {
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            // Save initial spawn position
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;

            // Load keybinds from PlayerPrefs
            keybindSettings?.LoadFromPlayerPrefs();

            // Apply FOV from settings
            ApplySettings();

            // Don't lock cursor at start - let UIManager handle it based on game state
            UnlockCursor();
        }

        private void ApplySettings()
        {
            if (playerCamera != null)
            {
                // Calculate vertical FOV to match Valorant's 103° horizontal FOV
                float aspectRatio = playerCamera.aspect;
                float verticalFOV = GameSettings.CalculateVerticalFOV(aspectRatio);
                playerCamera.fieldOfView = verticalFOV;
            }
        }

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Update()
        {
            // F3 to toggle debug overlay (works anytime)
            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                _showDebugInfo = !_showDebugInfo;
            }

            if (!_isControlEnabled) return;

            HandleMouseLook();
            HandlePauseInput();

            if (enableMovement)
            {
                HandleMovement();
                HandleCrouch();
                UpdateHeight();
            }
        }

        private void HandleMouseLook()
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Calculate sensitivity based on Valorant formula
            float sens;
            bool invert;

            if (gameSettings != null)
            {
                sens = gameSettings.CalculateUnitySensitivity();
                invert = gameSettings.InvertY;
            }
            else
            {
                sens = mouseSensitivity * GameSettings.VALORANT_YAW * GameSettings.UNITY_SCALE_FACTOR;
                invert = invertY;
            }

            float mouseX = mouseDelta.x * sens;
            float mouseY = mouseDelta.y * sens;

            if (invert)
                mouseY = -mouseY;

            // Horizontal rotation - rotate the player body
            transform.Rotate(Vector3.up * mouseX);

            // Debug: track total rotation
            if (_showDebugInfo)
            {
                _debugTotalYaw += Mathf.Abs(mouseX);
            }

            // Vertical rotation - rotate only the camera
            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -verticalLookLimit, verticalLookLimit);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            // Don't process movement if frozen (countdown, etc.)
            if (_movementFrozen) return;

            // Check ground
            _isGrounded = _characterController.isGrounded;

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to keep grounded
            }

            // Get input
            Vector2 moveInput = GetMovementInput();

            // Check walk modifier (Shift = walk slow)
            _isWalking = IsKeyHeld("walk");

            // Calculate move direction in world space
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

            // Determine speed based on state
            float speed = GetCurrentMoveSpeed();

            // Apply movement
            _characterController.Move(moveDirection * speed * Time.deltaTime);

            // Handle jump
            if (IsKeyDown("jump") && _isGrounded)
            {
                _velocity.y = GameSettings.JUMP_VELOCITY;
            }

            // Apply gravity
            _velocity.y += GameSettings.GRAVITY * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private Vector2 GetMovementInput()
        {
            Vector2 input = Vector2.zero;

            // Use keybind settings if available, otherwise fallback to keyboard direct
            if (keybindSettings != null || KeybindManager.Instance != null)
            {
                if (IsKeyHeld("moveforward")) input.y += 1f;
                if (IsKeyHeld("movebackward")) input.y -= 1f;
                if (IsKeyHeld("moveright")) input.x += 1f;
                if (IsKeyHeld("moveleft")) input.x -= 1f;
            }
            else
            {
                // Fallback to ZQSD directly (AZERTY keyboard)
                if (Keyboard.current.zKey.isPressed) input.y += 1f;
                if (Keyboard.current.sKey.isPressed) input.y -= 1f;
                if (Keyboard.current.dKey.isPressed) input.x += 1f;
                if (Keyboard.current.qKey.isPressed) input.x -= 1f;
            }

            // Normalize diagonal movement
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            return input;
        }

        private float GetCurrentMoveSpeed()
        {
            if (_isCrouching)
            {
                return GameSettings.CROUCH_SPEED;
            }
            else if (_isWalking)
            {
                return GameSettings.WALK_SPEED;
            }
            else
            {
                return GameSettings.RUN_SPEED;
            }
        }

        private void HandleCrouch()
        {
            bool wantsCrouch = IsKeyHeld("crouch");

            if (wantsCrouch && !_isCrouching)
            {
                // Start crouching
                _isCrouching = true;
                _targetHeight = GameSettings.CROUCH_HEIGHT;
                _targetCameraY = GameSettings.CAMERA_HEIGHT_CROUCH;
            }
            else if (!wantsCrouch && _isCrouching)
            {
                // Try to stand up - check for ceiling
                if (CanStandUp())
                {
                    _isCrouching = false;
                    _targetHeight = GameSettings.STAND_HEIGHT;
                    _targetCameraY = GameSettings.CAMERA_HEIGHT_STAND;
                }
            }
        }

        private bool CanStandUp()
        {
            // Raycast upward to check for ceiling
            float heightDifference = GameSettings.STAND_HEIGHT - GameSettings.CROUCH_HEIGHT;
            Vector3 origin = transform.position + Vector3.up * GameSettings.CROUCH_HEIGHT;

            return !Physics.Raycast(origin, Vector3.up, heightDifference + 0.1f);
        }

        private void UpdateHeight()
        {
            // Smooth height transition
            float heightSpeed = 10f;

            if (Mathf.Abs(_currentHeight - _targetHeight) > 0.01f)
            {
                _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, Time.deltaTime * heightSpeed);
                _characterController.height = _currentHeight;
                _characterController.center = new Vector3(0, _currentHeight / 2f, 0);
            }

            if (Mathf.Abs(_currentCameraY - _targetCameraY) > 0.01f)
            {
                _currentCameraY = Mathf.Lerp(_currentCameraY, _targetCameraY, Time.deltaTime * heightSpeed);

                if (cameraTransform != null)
                {
                    Vector3 camPos = cameraTransform.localPosition;
                    camPos.y = _currentCameraY;
                    cameraTransform.localPosition = camPos;
                }
            }
        }

        private bool IsKeyHeld(string actionName)
        {
            // Try KeybindManager first
            if (KeybindManager.Instance != null)
            {
                return KeybindManager.Instance.IsKeyHeld(actionName);
            }

            // Fall back to keybind settings (using new Input System)
            if (keybindSettings != null)
            {
                KeyCode key = keybindSettings.GetKeybind(actionName);
                return IsKeyCodePressed(key);
            }

            // Ultimate fallback - hardcoded defaults (AZERTY: ZQSD)
            return actionName.ToLower() switch
            {
                "moveforward" => Keyboard.current.zKey.isPressed,
                "movebackward" => Keyboard.current.sKey.isPressed,
                "moveleft" => Keyboard.current.qKey.isPressed,
                "moveright" => Keyboard.current.dKey.isPressed,
                "walk" => Keyboard.current.leftShiftKey.isPressed,
                "crouch" => Keyboard.current.leftCtrlKey.isPressed,
                "jump" => Keyboard.current.spaceKey.isPressed,
                _ => false
            };
        }

        private bool IsKeyDown(string actionName)
        {
            // Try KeybindManager first
            if (KeybindManager.Instance != null)
            {
                return KeybindManager.Instance.IsKeyDown(actionName);
            }

            // Fall back to keybind settings (using new Input System)
            if (keybindSettings != null)
            {
                KeyCode key = keybindSettings.GetKeybind(actionName);
                return IsKeyCodePressedThisFrame(key);
            }

            // Ultimate fallback
            return actionName.ToLower() switch
            {
                "jump" => Keyboard.current.spaceKey.wasPressedThisFrame,
                _ => false
            };
        }

        private void OnGUI()
        {
            if (!_showDebugInfo || !_isControlEnabled) return;

            var boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            var labelStyle = new GUIStyle { richText = true, normal = { textColor = Color.white } };
            var headerStyle = new GUIStyle { richText = true, fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

            float panelHeight = 280f;
            GUILayout.BeginArea(new Rect(10, Screen.height - panelHeight - 10, 320, panelHeight));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label("<color=yellow><b>DEBUG [F3]</b></color>",
                new GUIStyle { richText = true, fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
            GUILayout.Space(5);

            // Movement Section
            GUILayout.Label("<color=#4ECDC4>▸ MOVEMENT</color>", headerStyle);
            string moveState = _isCrouching ? "CROUCH" : (_isWalking ? "WALK" : "RUN");
            float speed = GetCurrentMoveSpeed();
            GUILayout.Label($"  State: <b>{moveState}</b>  |  Speed: {speed:F1} m/s", labelStyle);
            GUILayout.Label($"  Grounded: {_isGrounded}  |  Velocity Y: {_velocity.y:F1}", labelStyle);
            GUILayout.Space(5);

            // Sensitivity Section
            GUILayout.Label("<color=#FF6B6B>▸ SENSITIVITY</color>", headerStyle);
            if (gameSettings != null)
            {
                GUILayout.Label($"  Valorant Sens: <b>{gameSettings.ValorantSensitivity:F2}</b>", labelStyle);
            }
            GUILayout.Space(5);

            // FOV Section
            GUILayout.Label("<color=#95E1D3>▸ FOV (Valorant: 103° H)</color>", headerStyle);
            if (playerCamera != null)
            {
                GUILayout.Label($"  Vertical: {playerCamera.fieldOfView:F1}°  |  Aspect: {playerCamera.aspect:F2}", labelStyle);
            }
            GUILayout.Space(5);

            // Calibration Section
            GUILayout.Label("<color=#DDA0DD>▸ ROTATION TEST</color>", headerStyle);
            GUILayout.Label($"  Total: <b>{_debugTotalYaw:F1}°</b>  <color=gray>[R] Reset</color>", labelStyle);

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                _debugTotalYaw = 0f;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void HandlePauseInput()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.PauseGame();
                    UnlockCursor();
                }
                else if (GameManager.Instance.CurrentState == GameState.Paused)
                {
                    GameManager.Instance.ResumeGame();
                    LockCursor();
                }
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    EnableControl();
                    _movementFrozen = false; // Can move during play
                    LockCursor();
                    break;

                case GameState.Countdown:
                    ResetToSpawnPosition(); // Reset position at start of countdown
                    EnableControl(); // Can look around
                    _movementFrozen = true; // But can't move during countdown
                    LockCursor();
                    break;

                case GameState.Paused:
                case GameState.Results:
                case GameState.MainMenu:
                    DisableControl();
                    _movementFrozen = true;
                    UnlockCursor();
                    break;
            }
        }

        // Public API
        public void SetGameSettings(GameSettings settings)
        {
            gameSettings = settings;
            ApplySettings();
        }

        public void SetKeybindSettings(KeybindSettings settings)
        {
            keybindSettings = settings;
            keybindSettings?.LoadFromPlayerPrefs();
        }

        public void SetSensitivity(float sensitivity)
        {
            if (gameSettings != null)
            {
                gameSettings.SetValorantSensitivity(sensitivity);
            }
            else
            {
                mouseSensitivity = sensitivity;
            }
        }

        public void SetInvertY(bool invert)
        {
            if (gameSettings != null)
            {
                gameSettings.SetInvertY(invert);
            }
            else
            {
                invertY = invert;
            }
        }

        public void SetMovementEnabled(bool enabled)
        {
            enableMovement = enabled;
        }

        /// <summary>
        /// Reset player to spawn position and rotation
        /// </summary>
        public void ResetToSpawnPosition()
        {
            // Disable CharacterController temporarily to allow teleport
            _characterController.enabled = false;

            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;

            // Reset velocity
            _velocity = Vector3.zero;

            // Reset vertical look
            _verticalRotation = 0f;
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.identity;
            }

            // Reset crouch state
            _isCrouching = false;
            _isWalking = false;
            _currentHeight = GameSettings.STAND_HEIGHT;
            _targetHeight = GameSettings.STAND_HEIGHT;
            _currentCameraY = GameSettings.CAMERA_HEIGHT_STAND;
            _targetCameraY = GameSettings.CAMERA_HEIGHT_STAND;

            _characterController.height = _currentHeight;
            _characterController.center = new Vector3(0, _currentHeight / 2f, 0);

            // Re-enable CharacterController
            _characterController.enabled = true;
        }

        /// <summary>
        /// Set a custom spawn position (call before session starts)
        /// </summary>
        public void SetSpawnPosition(Vector3 position, Quaternion rotation)
        {
            _spawnPosition = position;
            _spawnRotation = rotation;
        }

        public GameSettings GetGameSettings()
        {
            return gameSettings;
        }

        public void EnableControl()
        {
            _isControlEnabled = true;
        }

        public void DisableControl()
        {
            _isControlEnabled = false;
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // State getters
        public bool IsCrouching => _isCrouching;
        public bool IsWalking => _isWalking;
        public bool IsGrounded => _isGrounded;
        public float CurrentSpeed => GetCurrentMoveSpeed();

        // Input System helpers for KeyCode
        private bool IsKeyCodePressed(KeyCode keyCode)
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (keyCode == KeyCode.Mouse0) return mouse.leftButton.isPressed;
                if (keyCode == KeyCode.Mouse1) return mouse.rightButton.isPressed;
                if (keyCode == KeyCode.Mouse2) return mouse.middleButton.isPressed;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            return keyCode switch
            {
                KeyCode.Z => keyboard.zKey.isPressed,
                KeyCode.S => keyboard.sKey.isPressed,
                KeyCode.Q => keyboard.qKey.isPressed,
                KeyCode.D => keyboard.dKey.isPressed,
                KeyCode.W => keyboard.wKey.isPressed,
                KeyCode.A => keyboard.aKey.isPressed,
                KeyCode.LeftShift => keyboard.leftShiftKey.isPressed,
                KeyCode.LeftControl => keyboard.leftCtrlKey.isPressed,
                KeyCode.Space => keyboard.spaceKey.isPressed,
                KeyCode.E => keyboard.eKey.isPressed,
                KeyCode.R => keyboard.rKey.isPressed,
                _ => false
            };
        }

        private bool IsKeyCodePressedThisFrame(KeyCode keyCode)
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (keyCode == KeyCode.Mouse0) return mouse.leftButton.wasPressedThisFrame;
                if (keyCode == KeyCode.Mouse1) return mouse.rightButton.wasPressedThisFrame;
                if (keyCode == KeyCode.Mouse2) return mouse.middleButton.wasPressedThisFrame;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            return keyCode switch
            {
                KeyCode.Z => keyboard.zKey.wasPressedThisFrame,
                KeyCode.S => keyboard.sKey.wasPressedThisFrame,
                KeyCode.Q => keyboard.qKey.wasPressedThisFrame,
                KeyCode.D => keyboard.dKey.wasPressedThisFrame,
                KeyCode.W => keyboard.wKey.wasPressedThisFrame,
                KeyCode.A => keyboard.aKey.wasPressedThisFrame,
                KeyCode.LeftShift => keyboard.leftShiftKey.wasPressedThisFrame,
                KeyCode.LeftControl => keyboard.leftCtrlKey.wasPressedThisFrame,
                KeyCode.Space => keyboard.spaceKey.wasPressedThisFrame,
                KeyCode.E => keyboard.eKey.wasPressedThisFrame,
                KeyCode.R => keyboard.rKey.wasPressedThisFrame,
                _ => false
            };
        }
    }
}
