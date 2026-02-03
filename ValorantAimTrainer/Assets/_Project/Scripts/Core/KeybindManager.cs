using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ValorantAimTrainer.Data;

namespace ValorantAimTrainer.Core
{
    /// <summary>
    /// Manages keybind rebinding with runtime key detection.
    /// Uses the new Input System package.
    /// </summary>
    public class KeybindManager : MonoBehaviour
    {
        public static KeybindManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private KeybindSettings keybindSettings;

        // Rebinding state
        private bool _isRebinding;
        private string _rebindingAction;
        private Action<KeyCode> _onRebindComplete;
        private Action _onRebindCancelled;

        // Events
        public event Action<string> OnRebindStarted;
        public event Action<string, KeyCode> OnRebindCompleted;
        public event Action OnRebindCancelled;

        public KeybindSettings Settings => keybindSettings;
        public bool IsRebinding => _isRebinding;
        public string RebindingAction => _rebindingAction;

        // Keys that cannot be rebound
        private static readonly HashSet<KeyCode> ForbiddenKeys = new()
        {
            KeyCode.None,
            KeyCode.Print,
            KeyCode.SysReq,
            KeyCode.Break,
            KeyCode.Menu
        };

        // Mapping from Key (Input System) to KeyCode (legacy)
        private static readonly Dictionary<Key, KeyCode> KeyToKeyCode = new()
        {
            { Key.A, KeyCode.A }, { Key.B, KeyCode.B }, { Key.C, KeyCode.C },
            { Key.D, KeyCode.D }, { Key.E, KeyCode.E }, { Key.F, KeyCode.F },
            { Key.G, KeyCode.G }, { Key.H, KeyCode.H }, { Key.I, KeyCode.I },
            { Key.J, KeyCode.J }, { Key.K, KeyCode.K }, { Key.L, KeyCode.L },
            { Key.M, KeyCode.M }, { Key.N, KeyCode.N }, { Key.O, KeyCode.O },
            { Key.P, KeyCode.P }, { Key.Q, KeyCode.Q }, { Key.R, KeyCode.R },
            { Key.S, KeyCode.S }, { Key.T, KeyCode.T }, { Key.U, KeyCode.U },
            { Key.V, KeyCode.V }, { Key.W, KeyCode.W }, { Key.X, KeyCode.X },
            { Key.Y, KeyCode.Y }, { Key.Z, KeyCode.Z },
            { Key.Digit0, KeyCode.Alpha0 }, { Key.Digit1, KeyCode.Alpha1 },
            { Key.Digit2, KeyCode.Alpha2 }, { Key.Digit3, KeyCode.Alpha3 },
            { Key.Digit4, KeyCode.Alpha4 }, { Key.Digit5, KeyCode.Alpha5 },
            { Key.Digit6, KeyCode.Alpha6 }, { Key.Digit7, KeyCode.Alpha7 },
            { Key.Digit8, KeyCode.Alpha8 }, { Key.Digit9, KeyCode.Alpha9 },
            { Key.Space, KeyCode.Space }, { Key.Enter, KeyCode.Return },
            { Key.Tab, KeyCode.Tab }, { Key.Backspace, KeyCode.Backspace },
            { Key.Escape, KeyCode.Escape }, { Key.Delete, KeyCode.Delete },
            { Key.LeftShift, KeyCode.LeftShift }, { Key.RightShift, KeyCode.RightShift },
            { Key.LeftCtrl, KeyCode.LeftControl }, { Key.RightCtrl, KeyCode.RightControl },
            { Key.LeftAlt, KeyCode.LeftAlt }, { Key.RightAlt, KeyCode.RightAlt },
            { Key.UpArrow, KeyCode.UpArrow }, { Key.DownArrow, KeyCode.DownArrow },
            { Key.LeftArrow, KeyCode.LeftArrow }, { Key.RightArrow, KeyCode.RightArrow },
            { Key.F1, KeyCode.F1 }, { Key.F2, KeyCode.F2 }, { Key.F3, KeyCode.F3 },
            { Key.F4, KeyCode.F4 }, { Key.F5, KeyCode.F5 }, { Key.F6, KeyCode.F6 },
            { Key.F7, KeyCode.F7 }, { Key.F8, KeyCode.F8 }, { Key.F9, KeyCode.F9 },
            { Key.F10, KeyCode.F10 }, { Key.F11, KeyCode.F11 }, { Key.F12, KeyCode.F12 },
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Load saved keybinds
            keybindSettings?.LoadFromPlayerPrefs();
        }

        private void Update()
        {
            if (_isRebinding)
            {
                DetectKeyPress();
            }
        }

        /// <summary>
        /// Start listening for a new key press to rebind an action
        /// </summary>
        public void StartRebind(string actionName, Action<KeyCode> onComplete = null, Action onCancel = null)
        {
            if (_isRebinding) return;

            _isRebinding = true;
            _rebindingAction = actionName;
            _onRebindComplete = onComplete;
            _onRebindCancelled = onCancel;

            OnRebindStarted?.Invoke(actionName);
        }

        /// <summary>
        /// Cancel the current rebind operation
        /// </summary>
        public void CancelRebind()
        {
            if (!_isRebinding) return;

            _isRebinding = false;
            _rebindingAction = null;

            _onRebindCancelled?.Invoke();
            OnRebindCancelled?.Invoke();

            _onRebindComplete = null;
            _onRebindCancelled = null;
        }

        private void DetectKeyPress()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Check for cancel (Escape)
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelRebind();
                return;
            }

            // Check keyboard keys
            foreach (var kvp in KeyToKeyCode)
            {
                if (keyboard[kvp.Key].wasPressedThisFrame)
                {
                    if (!ForbiddenKeys.Contains(kvp.Value))
                    {
                        CompleteRebind(kvp.Value);
                        return;
                    }
                }
            }

            // Check mouse buttons
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    CompleteRebind(KeyCode.Mouse0);
                    return;
                }
                if (mouse.rightButton.wasPressedThisFrame)
                {
                    CompleteRebind(KeyCode.Mouse1);
                    return;
                }
                if (mouse.middleButton.wasPressedThisFrame)
                {
                    CompleteRebind(KeyCode.Mouse2);
                    return;
                }
            }
        }

        private void CompleteRebind(KeyCode newKey)
        {
            if (!_isRebinding) return;

            string action = _rebindingAction;

            // Apply the new keybind
            keybindSettings?.SetKeybind(action, newKey);

            _isRebinding = false;
            _rebindingAction = null;

            _onRebindComplete?.Invoke(newKey);
            OnRebindCompleted?.Invoke(action, newKey);

            _onRebindComplete = null;
            _onRebindCancelled = null;
        }

        /// <summary>
        /// Get the current key for an action
        /// </summary>
        public KeyCode GetKey(string actionName)
        {
            return keybindSettings?.GetKeybind(actionName) ?? KeyCode.None;
        }

        /// <summary>
        /// Check if a key is currently pressed (using new Input System)
        /// </summary>
        public bool IsKeyHeld(string actionName)
        {
            KeyCode key = GetKey(actionName);
            if (key == KeyCode.None) return false;

            return IsKeyCodePressed(key);
        }

        /// <summary>
        /// Check if a key was just pressed this frame
        /// </summary>
        public bool IsKeyDown(string actionName)
        {
            KeyCode key = GetKey(actionName);
            if (key == KeyCode.None) return false;

            return IsKeyCodePressedThisFrame(key);
        }

        /// <summary>
        /// Check if a key was just released this frame
        /// </summary>
        public bool IsKeyUp(string actionName)
        {
            KeyCode key = GetKey(actionName);
            if (key == KeyCode.None) return false;

            return IsKeyCodeReleasedThisFrame(key);
        }

        private bool IsKeyCodePressed(KeyCode keyCode)
        {
            // Handle mouse buttons
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (keyCode == KeyCode.Mouse0) return mouse.leftButton.isPressed;
                if (keyCode == KeyCode.Mouse1) return mouse.rightButton.isPressed;
                if (keyCode == KeyCode.Mouse2) return mouse.middleButton.isPressed;
            }

            // Handle keyboard
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            Key? inputKey = KeyCodeToKey(keyCode);
            if (inputKey.HasValue)
            {
                return keyboard[inputKey.Value].isPressed;
            }

            return false;
        }

        private bool IsKeyCodePressedThisFrame(KeyCode keyCode)
        {
            // Handle mouse buttons
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (keyCode == KeyCode.Mouse0) return mouse.leftButton.wasPressedThisFrame;
                if (keyCode == KeyCode.Mouse1) return mouse.rightButton.wasPressedThisFrame;
                if (keyCode == KeyCode.Mouse2) return mouse.middleButton.wasPressedThisFrame;
            }

            // Handle keyboard
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            Key? inputKey = KeyCodeToKey(keyCode);
            if (inputKey.HasValue)
            {
                return keyboard[inputKey.Value].wasPressedThisFrame;
            }

            return false;
        }

        private bool IsKeyCodeReleasedThisFrame(KeyCode keyCode)
        {
            // Handle mouse buttons
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (keyCode == KeyCode.Mouse0) return mouse.leftButton.wasReleasedThisFrame;
                if (keyCode == KeyCode.Mouse1) return mouse.rightButton.wasReleasedThisFrame;
                if (keyCode == KeyCode.Mouse2) return mouse.middleButton.wasReleasedThisFrame;
            }

            // Handle keyboard
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            Key? inputKey = KeyCodeToKey(keyCode);
            if (inputKey.HasValue)
            {
                return keyboard[inputKey.Value].wasReleasedThisFrame;
            }

            return false;
        }

        private Key? KeyCodeToKey(KeyCode keyCode)
        {
            // Reverse lookup
            foreach (var kvp in KeyToKeyCode)
            {
                if (kvp.Value == keyCode)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Reset all keybinds to defaults
        /// </summary>
        public void ResetAllToDefaults()
        {
            keybindSettings?.ResetToDefaults();
        }

        /// <summary>
        /// Get display name for an action's current key
        /// </summary>
        public string GetKeyDisplayName(string actionName)
        {
            KeyCode key = GetKey(actionName);
            return KeybindSettings.GetKeyDisplayName(key);
        }
    }
}
