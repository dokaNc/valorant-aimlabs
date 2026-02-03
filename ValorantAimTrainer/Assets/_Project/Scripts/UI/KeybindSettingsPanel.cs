using UnityEngine;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.Audio;

namespace ValorantAimTrainer.UI
{
    /// <summary>
    /// UI panel for keybind configuration with rebinding support.
    /// </summary>
    public class KeybindSettingsPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private KeybindSettings keybindSettings;

        private bool _isVisible = false;
        private bool _openedFromPause = false;
        private string _waitingForRebind = null;

        // UI Styles
        private GUIStyle _titleStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonHoverStyle;
        private GUIStyle _rebindingStyle;
        private Texture2D _panelTexture;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;
        private Texture2D _rebindingTexture;

        // Keybind definitions for display
        private readonly (string actionName, string displayLabel)[] _movementBinds =
        {
            ("moveforward", "Move Forward"),
            ("movebackward", "Move Backward"),
            ("moveleft", "Move Left"),
            ("moveright", "Move Right"),
            ("walk", "Walk (Slow)"),
            ("crouch", "Crouch"),
            ("jump", "Jump")
        };

        private readonly (string actionName, string displayLabel)[] _actionBinds =
        {
            ("shoot", "Shoot"),
            ("reload", "Reload"),
            ("interact", "Interact")
        };

        private void Awake()
        {
            CreateTextures();
        }

        private void Start()
        {
            InitializeStyles();
        }

        private void OnEnable()
        {
            if (KeybindManager.Instance != null)
            {
                KeybindManager.Instance.OnRebindStarted += HandleRebindStarted;
                KeybindManager.Instance.OnRebindCompleted += HandleRebindCompleted;
                KeybindManager.Instance.OnRebindCancelled += HandleRebindCancelled;
            }
        }

        private void OnDisable()
        {
            if (KeybindManager.Instance != null)
            {
                KeybindManager.Instance.OnRebindStarted -= HandleRebindStarted;
                KeybindManager.Instance.OnRebindCompleted -= HandleRebindCompleted;
                KeybindManager.Instance.OnRebindCancelled -= HandleRebindCancelled;
            }
        }

        private void CreateTextures()
        {
            _panelTexture = new Texture2D(1, 1);
            _panelTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.98f));
            _panelTexture.Apply();

            _buttonTexture = new Texture2D(1, 1);
            _buttonTexture.SetPixel(0, 0, new Color(0.25f, 0.25f, 0.3f, 1f));
            _buttonTexture.Apply();

            _buttonHoverTexture = new Texture2D(1, 1);
            _buttonHoverTexture.SetPixel(0, 0, new Color(0.92f, 0.29f, 0.24f, 1f));
            _buttonHoverTexture.Apply();

            _rebindingTexture = new Texture2D(1, 1);
            _rebindingTexture.SetPixel(0, 0, new Color(0.8f, 0.6f, 0.1f, 1f));
            _rebindingTexture.Apply();
        }

        private void InitializeStyles()
        {
            _titleStyle = new GUIStyle
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = Color.white;

            _sectionStyle = new GUIStyle
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _sectionStyle.normal.textColor = new Color(0.92f, 0.29f, 0.24f);

            _labelStyle = new GUIStyle
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft
            };
            _labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            _buttonStyle = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.normal.background = _buttonTexture;

            _buttonHoverStyle = new GUIStyle(_buttonStyle);
            _buttonHoverStyle.normal.background = _buttonHoverTexture;

            _rebindingStyle = new GUIStyle(_buttonStyle);
            _rebindingStyle.normal.background = _rebindingTexture;
            _rebindingStyle.normal.textColor = Color.black;
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            // Background
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _panelTexture);

            float centerX = Screen.width / 2f;
            float panelWidth = 550f;
            float panelX = centerX - panelWidth / 2f;
            float y = 30f;

            // Title
            GUI.Label(new Rect(0, y, Screen.width, 50), "CONTROLS", _titleStyle);
            y += 60;

            // Movement Section
            GUI.Label(new Rect(panelX, y, panelWidth, 30), "MOVEMENT", _sectionStyle);
            y += 35;

            foreach (var bind in _movementBinds)
            {
                DrawKeybindRow(panelX, ref y, bind.displayLabel, bind.actionName);
            }

            y += 15;

            // Actions Section
            GUI.Label(new Rect(panelX, y, panelWidth, 30), "ACTIONS", _sectionStyle);
            y += 35;

            foreach (var bind in _actionBinds)
            {
                DrawKeybindRow(panelX, ref y, bind.displayLabel, bind.actionName);
            }

            y += 30;

            // Bottom Buttons
            float buttonWidth = 150;
            float buttonHeight = 40;
            float buttonY = Screen.height - 80;

            // Reset Defaults Button
            Rect resetRect = new Rect(centerX - buttonWidth * 1.5f - 20, buttonY, buttonWidth, buttonHeight);
            if (DrawButton(resetRect, "RESET DEFAULTS"))
            {
                OnResetDefaultsClicked();
            }

            // Apply Button
            Rect applyRect = new Rect(centerX - buttonWidth / 2f, buttonY, buttonWidth, buttonHeight);
            if (DrawButton(applyRect, "APPLY"))
            {
                OnApplyClicked();
            }

            // Back Button
            Rect backRect = new Rect(centerX + buttonWidth / 2f + 20, buttonY, buttonWidth, buttonHeight);
            if (DrawButton(backRect, "BACK"))
            {
                OnBackClicked();
            }

            // Help text
            string helpText = _waitingForRebind != null
                ? "Press any key to bind, ESC to cancel"
                : "Click a key to rebind";

            GUIStyle helpStyle = new GUIStyle
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            helpStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

            GUI.Label(new Rect(0, buttonY - 35, Screen.width, 25), helpText, helpStyle);
        }

        private void DrawKeybindRow(float x, ref float y, string label, string actionName)
        {
            float rowHeight = 30;
            float buttonWidth = 120;

            // Label
            GUI.Label(new Rect(x, y, 200, rowHeight), label, _labelStyle);

            // Key button
            Rect buttonRect = new Rect(x + 250, y, buttonWidth, rowHeight - 2);

            bool isRebinding = _waitingForRebind == actionName;
            string buttonText = isRebinding ? "..." : GetKeyDisplayName(actionName);

            GUIStyle style;
            if (isRebinding)
            {
                style = _rebindingStyle;
            }
            else
            {
                bool isHovered = buttonRect.Contains(Event.current.mousePosition);
                style = isHovered ? _buttonHoverStyle : _buttonStyle;
            }

            if (GUI.Button(buttonRect, buttonText, style))
            {
                if (!isRebinding && _waitingForRebind == null)
                {
                    StartRebind(actionName);
                }
            }

            y += rowHeight + 5;
        }

        private string GetKeyDisplayName(string actionName)
        {
            if (KeybindManager.Instance != null)
            {
                return KeybindManager.Instance.GetKeyDisplayName(actionName);
            }

            if (keybindSettings != null)
            {
                KeyCode key = keybindSettings.GetKeybind(actionName);
                return KeybindSettings.GetKeyDisplayName(key);
            }

            return "???";
        }

        private void StartRebind(string actionName)
        {
            if (KeybindManager.Instance != null)
            {
                KeybindManager.Instance.StartRebind(actionName);
            }
            else
            {
                _waitingForRebind = actionName;
            }

            AudioManager.Instance?.PlayUIClick();
        }

        private void HandleRebindStarted(string actionName)
        {
            _waitingForRebind = actionName;
        }

        private void HandleRebindCompleted(string actionName, KeyCode newKey)
        {
            _waitingForRebind = null;
            AudioManager.Instance?.PlayUIClick();
        }

        private void HandleRebindCancelled()
        {
            _waitingForRebind = null;
        }

        private bool DrawButton(Rect rect, string text)
        {
            bool isHovered = rect.Contains(Event.current.mousePosition);
            GUIStyle style = isHovered ? _buttonHoverStyle : _buttonStyle;

            bool clicked = GUI.Button(rect, text, style);

            if (clicked)
            {
                AudioManager.Instance?.PlayUIClick();
            }

            return clicked;
        }

        private void OnResetDefaultsClicked()
        {
            if (KeybindManager.Instance != null)
            {
                KeybindManager.Instance.ResetAllToDefaults();
            }
            else if (keybindSettings != null)
            {
                keybindSettings.ResetToDefaults();
            }
        }

        private void OnApplyClicked()
        {
            keybindSettings?.SaveToPlayerPrefs();
            OnBackClicked();
        }

        private void OnBackClicked()
        {
            // Cancel any pending rebind
            if (_waitingForRebind != null)
            {
                KeybindManager.Instance?.CancelRebind();
                _waitingForRebind = null;
            }

            Hide();
            UIManager.Instance?.ShowSettingsFromKeybinds(_openedFromPause);
        }

        public void Show(bool fromPause = false)
        {
            _isVisible = true;
            _openedFromPause = fromPause;
            _waitingForRebind = null;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        private void OnDestroy()
        {
            if (_panelTexture != null) Destroy(_panelTexture);
            if (_buttonTexture != null) Destroy(_buttonTexture);
            if (_buttonHoverTexture != null) Destroy(_buttonHoverTexture);
            if (_rebindingTexture != null) Destroy(_rebindingTexture);
        }
    }
}
