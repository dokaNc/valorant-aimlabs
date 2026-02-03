using UnityEngine;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.Gameplay;
using ValorantAimTrainer.Audio;

namespace ValorantAimTrainer.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameSettings gameSettings;

        private bool _isVisible = false;
        private bool _openedFromPause = false;

        // Temp values (before apply)
        private float _tempValorantSens;
        private bool _tempInvertY;
        private int _tempSessionDuration;
        private float _tempMasterVolume;
        private float _tempSFXVolume;
        private float _tempMusicVolume;

        // UI
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonHoverStyle;
        private Texture2D _panelTexture;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;

        // Session Duration Presets
        private readonly int[] _durationPresets = { 30, 60, 90, 120 };
        private int _selectedDurationIndex = 1; // Default 60

        private void Awake()
        {
            CreateTextures();
        }

        private void Start()
        {
            InitializeStyles();
            LoadCurrentSettings();
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

            _labelStyle = new GUIStyle
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft
            };
            _labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            _valueStyle = new GUIStyle
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            _valueStyle.normal.textColor = Color.white;

            _buttonStyle = new GUIStyle
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.normal.background = _buttonTexture;

            _buttonHoverStyle = new GUIStyle(_buttonStyle);
            _buttonHoverStyle.normal.background = _buttonHoverTexture;

        }

        private void LoadCurrentSettings()
        {
            if (gameSettings != null)
            {
                _tempValorantSens = gameSettings.ValorantSensitivity;
                _tempInvertY = gameSettings.InvertY;
                _tempSessionDuration = gameSettings.SessionDuration;

                // Find matching duration preset
                for (int i = 0; i < _durationPresets.Length; i++)
                {
                    if (_durationPresets[i] == _tempSessionDuration)
                    {
                        _selectedDurationIndex = i;
                        break;
                    }
                }
            }

            if (AudioManager.Instance != null)
            {
                _tempMasterVolume = AudioManager.Instance.MasterVolume;
                _tempSFXVolume = AudioManager.Instance.SFXVolume;
                _tempMusicVolume = AudioManager.Instance.MusicVolume;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            // Background
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _panelTexture);

            float centerX = Screen.width / 2f;
            float panelWidth = 600f;
            float panelX = centerX - panelWidth / 2f;
            float y = 40f;

            // Title
            GUI.Label(new Rect(0, y, Screen.width, 50), "SETTINGS", _titleStyle);
            y += 70;

            // === SENSITIVITY (MAIN SETTING) ===
            GUI.Label(new Rect(panelX, y, panelWidth, 30), "─── SENSITIVITY ───", _titleStyle);
            y += 40;

            // Valorant Sensitivity - PRIMARY SETTING
            GUI.Label(new Rect(panelX, y, 200, 25), "Valorant Sensitivity", _labelStyle);
            _tempValorantSens = GUI.HorizontalSlider(
                new Rect(panelX + 220, y + 3, 250, 20),
                _tempValorantSens, 0.1f, 2f
            );
            GUI.Label(new Rect(panelX + 480, y, 80, 25), _tempValorantSens.ToString("F2"), _valueStyle);
            y += 35;

            // Invert Y
            GUI.Label(new Rect(panelX, y, 200, 25), "Invert Y Axis", _labelStyle);
            if (GUI.Button(new Rect(panelX + 220, y, 100, 25), _tempInvertY ? "ON" : "OFF",
                _tempInvertY ? _buttonHoverStyle : _buttonStyle))
            {
                _tempInvertY = !_tempInvertY;
                AudioManager.Instance?.PlayUIClick();
            }
            y += 50;

            // === AUDIO SETTINGS ===
            GUI.Label(new Rect(panelX, y, panelWidth, 30), "─── AUDIO ───", _titleStyle);
            y += 40;

            // Master Volume
            GUI.Label(new Rect(panelX, y, 200, 25), "Master Volume", _labelStyle);
            _tempMasterVolume = GUI.HorizontalSlider(
                new Rect(panelX + 220, y + 3, 250, 20),
                _tempMasterVolume, 0f, 1f
            );
            GUI.Label(new Rect(panelX + 480, y, 80, 25), Mathf.RoundToInt(_tempMasterVolume * 100) + "%", _valueStyle);
            y += 35;

            // SFX Volume
            GUI.Label(new Rect(panelX, y, 200, 25), "SFX Volume", _labelStyle);
            _tempSFXVolume = GUI.HorizontalSlider(
                new Rect(panelX + 220, y + 3, 250, 20),
                _tempSFXVolume, 0f, 1f
            );
            GUI.Label(new Rect(panelX + 480, y, 80, 25), Mathf.RoundToInt(_tempSFXVolume * 100) + "%", _valueStyle);
            y += 35;

            // Music Volume
            GUI.Label(new Rect(panelX, y, 200, 25), "Music Volume", _labelStyle);
            _tempMusicVolume = GUI.HorizontalSlider(
                new Rect(panelX + 220, y + 3, 250, 20),
                _tempMusicVolume, 0f, 1f
            );
            GUI.Label(new Rect(panelX + 480, y, 80, 25), Mathf.RoundToInt(_tempMusicVolume * 100) + "%", _valueStyle);
            y += 50;

            // === CROSSHAIR ===
            GUI.Label(new Rect(panelX, y, panelWidth, 30), "─── CROSSHAIR ───", _titleStyle);
            y += 40;

            // Crosshair Settings Button
            Rect crosshairBtnRect = new Rect(panelX + 220, y, 200, 30);
            if (DrawButton(crosshairBtnRect, "CROSSHAIR SETTINGS"))
            {
                OnCrosshairSettingsClicked();
            }
            y += 50;

            // === CONTROLS ===
            GUI.Label(new Rect(panelX, y, panelWidth, 30), "─── CONTROLS ───", _titleStyle);
            y += 40;

            // Keybind Settings Button
            Rect keybindBtnRect = new Rect(panelX + 220, y, 200, 30);
            if (DrawButton(keybindBtnRect, "KEYBINDS"))
            {
                OnKeybindSettingsClicked();
            }
            y += 50;

            // === SESSION SETTINGS ===
            GUI.Label(new Rect(panelX, y, panelWidth, 30), "─── SESSION ───", _titleStyle);
            y += 40;

            // Session Duration
            GUI.Label(new Rect(panelX, y, 200, 25), "Session Duration", _labelStyle);
            for (int i = 0; i < _durationPresets.Length; i++)
            {
                Rect btnRect = new Rect(panelX + 220 + (i * 80), y, 75, 25);
                bool isSelected = (_selectedDurationIndex == i);
                GUIStyle style = isSelected ? _buttonHoverStyle : _buttonStyle;

                if (GUI.Button(btnRect, _durationPresets[i] + "s", style))
                {
                    _selectedDurationIndex = i;
                    _tempSessionDuration = _durationPresets[i];
                    AudioManager.Instance?.PlayUIClick();
                }
            }
            y += 60;

            // === BUTTONS ===
            float buttonWidth = 150;
            float buttonHeight = 45;
            float buttonY = Screen.height - 100;

            // Apply Button
            Rect applyRect = new Rect(centerX - buttonWidth - 20, buttonY, buttonWidth, buttonHeight);
            if (DrawButton(applyRect, "APPLY"))
            {
                ApplySettings();
            }

            // Back Button
            Rect backRect = new Rect(centerX + 20, buttonY, buttonWidth, buttonHeight);
            if (DrawButton(backRect, "BACK"))
            {
                OnBackClicked();
            }
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

        private void ApplySettings()
        {
            // Apply game settings
            if (gameSettings != null)
            {
                gameSettings.SetValorantSensitivity(_tempValorantSens);
                gameSettings.SetInvertY(_tempInvertY);
                gameSettings.SetSessionDuration(_tempSessionDuration);

                // Update session manager
                SessionManager.Instance?.SetSessionDuration(_tempSessionDuration);

                // Update player controller
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.SetGameSettings(gameSettings);
                }
            }

            // Apply audio settings
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(_tempMasterVolume);
                AudioManager.Instance.SetSFXVolume(_tempSFXVolume);
                AudioManager.Instance.SetMusicVolume(_tempMusicVolume);
            }
        }

        private void OnBackClicked()
        {
            if (_openedFromPause)
            {
                // Return to pause screen
                UIManager.Instance?.ShowPauseFromSettings();
            }
            else
            {
                // Return to main menu - UIManager will handle showing the menu
                Hide();
                UIManager.Instance?.ShowMainMenuFromSettings();
            }
        }

        private void OnCrosshairSettingsClicked()
        {
            Hide();
            UIManager.Instance?.ShowCrosshairSettings(_openedFromPause);
        }

        private void OnKeybindSettingsClicked()
        {
            Hide();
            UIManager.Instance?.ShowKeybindSettings(_openedFromPause);
        }

        public void Show(bool fromPause = false)
        {
            _isVisible = true;
            _openedFromPause = fromPause;
            LoadCurrentSettings();
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
        }
    }
}
