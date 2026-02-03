using System;
using UnityEngine;

namespace ValorantAimTrainer.Data
{
    /// <summary>
    /// Serializable keybind settings with PlayerPrefs persistence.
    /// Stores key codes for all rebindable actions.
    /// </summary>
    [CreateAssetMenu(fileName = "KeybindSettings", menuName = "ValorantAimTrainer/Keybind Settings")]
    public class KeybindSettings : ScriptableObject
    {
        // ============================================
        // DEFAULT KEYBINDS (VALORANT LAYOUT)
        // ============================================

        [Header("Movement (AZERTY: ZQSD)")]
        [SerializeField] private KeyCode moveForward = KeyCode.Z;
        [SerializeField] private KeyCode moveBackward = KeyCode.S;
        [SerializeField] private KeyCode moveLeft = KeyCode.Q;
        [SerializeField] private KeyCode moveRight = KeyCode.D;

        [Header("Movement Modifiers")]
        [SerializeField] private KeyCode walk = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouch = KeyCode.LeftControl;
        [SerializeField] private KeyCode jump = KeyCode.Space;

        [Header("Actions")]
        [SerializeField] private KeyCode shoot = KeyCode.Mouse0;
        [SerializeField] private KeyCode reload = KeyCode.R;
        [SerializeField] private KeyCode interact = KeyCode.E;

        [Header("UI")]
        [SerializeField] private KeyCode pause = KeyCode.Escape;

        // Properties
        public KeyCode MoveForward => moveForward;
        public KeyCode MoveBackward => moveBackward;
        public KeyCode MoveLeft => moveLeft;
        public KeyCode MoveRight => moveRight;
        public KeyCode Walk => walk;
        public KeyCode Crouch => crouch;
        public KeyCode Jump => jump;
        public KeyCode Shoot => shoot;
        public KeyCode Reload => reload;
        public KeyCode Interact => interact;
        public KeyCode Pause => pause;

        // PlayerPrefs keys
        private const string PREFS_PREFIX = "Keybind_";

        /// <summary>
        /// Load keybinds from PlayerPrefs
        /// </summary>
        public void LoadFromPlayerPrefs()
        {
            moveForward = LoadKey(nameof(moveForward), moveForward);
            moveBackward = LoadKey(nameof(moveBackward), moveBackward);
            moveLeft = LoadKey(nameof(moveLeft), moveLeft);
            moveRight = LoadKey(nameof(moveRight), moveRight);
            walk = LoadKey(nameof(walk), walk);
            crouch = LoadKey(nameof(crouch), crouch);
            jump = LoadKey(nameof(jump), jump);
            shoot = LoadKey(nameof(shoot), shoot);
            reload = LoadKey(nameof(reload), reload);
            interact = LoadKey(nameof(interact), interact);
            pause = LoadKey(nameof(pause), pause);
        }

        /// <summary>
        /// Save keybinds to PlayerPrefs
        /// </summary>
        public void SaveToPlayerPrefs()
        {
            SaveKey(nameof(moveForward), moveForward);
            SaveKey(nameof(moveBackward), moveBackward);
            SaveKey(nameof(moveLeft), moveLeft);
            SaveKey(nameof(moveRight), moveRight);
            SaveKey(nameof(walk), walk);
            SaveKey(nameof(crouch), crouch);
            SaveKey(nameof(jump), jump);
            SaveKey(nameof(shoot), shoot);
            SaveKey(nameof(reload), reload);
            SaveKey(nameof(interact), interact);
            SaveKey(nameof(pause), pause);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Reset all keybinds to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            // AZERTY layout (French keyboard)
            moveForward = KeyCode.Z;
            moveBackward = KeyCode.S;
            moveLeft = KeyCode.Q;
            moveRight = KeyCode.D;
            walk = KeyCode.LeftShift;
            crouch = KeyCode.LeftControl;
            jump = KeyCode.Space;
            shoot = KeyCode.Mouse0;
            reload = KeyCode.R;
            interact = KeyCode.E;
            pause = KeyCode.Escape;

            SaveToPlayerPrefs();
        }

        // Setters for rebinding
        public void SetMoveForward(KeyCode key) { moveForward = key; SaveToPlayerPrefs(); }
        public void SetMoveBackward(KeyCode key) { moveBackward = key; SaveToPlayerPrefs(); }
        public void SetMoveLeft(KeyCode key) { moveLeft = key; SaveToPlayerPrefs(); }
        public void SetMoveRight(KeyCode key) { moveRight = key; SaveToPlayerPrefs(); }
        public void SetWalk(KeyCode key) { walk = key; SaveToPlayerPrefs(); }
        public void SetCrouch(KeyCode key) { crouch = key; SaveToPlayerPrefs(); }
        public void SetJump(KeyCode key) { jump = key; SaveToPlayerPrefs(); }
        public void SetShoot(KeyCode key) { shoot = key; SaveToPlayerPrefs(); }
        public void SetReload(KeyCode key) { reload = key; SaveToPlayerPrefs(); }
        public void SetInteract(KeyCode key) { interact = key; SaveToPlayerPrefs(); }
        public void SetPause(KeyCode key) { pause = key; SaveToPlayerPrefs(); }

        /// <summary>
        /// Set a keybind by action name
        /// </summary>
        public void SetKeybind(string actionName, KeyCode key)
        {
            switch (actionName.ToLower())
            {
                case "moveforward": SetMoveForward(key); break;
                case "movebackward": SetMoveBackward(key); break;
                case "moveleft": SetMoveLeft(key); break;
                case "moveright": SetMoveRight(key); break;
                case "walk": SetWalk(key); break;
                case "crouch": SetCrouch(key); break;
                case "jump": SetJump(key); break;
                case "shoot": SetShoot(key); break;
                case "reload": SetReload(key); break;
                case "interact": SetInteract(key); break;
                case "pause": SetPause(key); break;
            }
        }

        /// <summary>
        /// Get a keybind by action name
        /// </summary>
        public KeyCode GetKeybind(string actionName)
        {
            return actionName.ToLower() switch
            {
                "moveforward" => moveForward,
                "movebackward" => moveBackward,
                "moveleft" => moveLeft,
                "moveright" => moveRight,
                "walk" => walk,
                "crouch" => crouch,
                "jump" => jump,
                "shoot" => shoot,
                "reload" => reload,
                "interact" => interact,
                "pause" => pause,
                _ => KeyCode.None
            };
        }

        private KeyCode LoadKey(string keyName, KeyCode defaultValue)
        {
            string key = PREFS_PREFIX + keyName;
            if (PlayerPrefs.HasKey(key))
            {
                return (KeyCode)PlayerPrefs.GetInt(key);
            }
            return defaultValue;
        }

        private void SaveKey(string keyName, KeyCode value)
        {
            PlayerPrefs.SetInt(PREFS_PREFIX + keyName, (int)value);
        }

        /// <summary>
        /// Get display name for a KeyCode
        /// </summary>
        public static string GetKeyDisplayName(KeyCode key)
        {
            return key switch
            {
                KeyCode.Mouse0 => "LMB",
                KeyCode.Mouse1 => "RMB",
                KeyCode.Mouse2 => "MMB",
                KeyCode.LeftShift => "L-Shift",
                KeyCode.RightShift => "R-Shift",
                KeyCode.LeftControl => "L-Ctrl",
                KeyCode.RightControl => "R-Ctrl",
                KeyCode.LeftAlt => "L-Alt",
                KeyCode.RightAlt => "R-Alt",
                KeyCode.Space => "Space",
                KeyCode.Return => "Enter",
                KeyCode.Escape => "Esc",
                KeyCode.Tab => "Tab",
                KeyCode.Backspace => "Backspace",
                KeyCode.Delete => "Del",
                KeyCode.UpArrow => "Up",
                KeyCode.DownArrow => "Down",
                KeyCode.LeftArrow => "Left",
                KeyCode.RightArrow => "Right",
                _ => key.ToString()
            };
        }
    }
}
