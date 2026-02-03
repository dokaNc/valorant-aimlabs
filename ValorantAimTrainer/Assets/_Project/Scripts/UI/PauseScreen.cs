using UnityEngine;
using ValorantAimTrainer.Core;

namespace ValorantAimTrainer.UI
{
    public class PauseScreen : MonoBehaviour
    {
        private bool _isVisible = false;
        private GUIStyle _titleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonHoverStyle;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;
        private Texture2D _overlayTexture;

        private void Awake()
        {
            CreateTextures();
        }

        private void Start()
        {
            InitializeStyles();
        }

        private void CreateTextures()
        {
            _overlayTexture = new Texture2D(1, 1);
            _overlayTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
            _overlayTexture.Apply();

            _buttonTexture = new Texture2D(1, 1);
            _buttonTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 0.9f));
            _buttonTexture.Apply();

            _buttonHoverTexture = new Texture2D(1, 1);
            _buttonHoverTexture.SetPixel(0, 0, new Color(0.92f, 0.29f, 0.24f, 1f));
            _buttonHoverTexture.Apply();
        }

        private void InitializeStyles()
        {
            _titleStyle = new GUIStyle
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = Color.white;

            _buttonStyle = new GUIStyle
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.normal.background = _buttonTexture;

            _buttonHoverStyle = new GUIStyle(_buttonStyle);
            _buttonHoverStyle.normal.background = _buttonHoverTexture;
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            // Dark overlay
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _overlayTexture);

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;

            // Title
            Rect titleRect = new Rect(0, centerY - 150, Screen.width, 60);
            GUI.Label(titleRect, "PAUSED", _titleStyle);

            // Buttons
            float buttonWidth = 250;
            float buttonHeight = 50;
            float buttonSpacing = 15;
            float buttonX = centerX - buttonWidth / 2f;
            float buttonStartY = centerY - 30;

            // Resume Button
            Rect resumeRect = new Rect(buttonX, buttonStartY, buttonWidth, buttonHeight);
            if (DrawButton(resumeRect, "RESUME"))
            {
                OnResumeClicked();
            }

            // Restart Button
            Rect restartRect = new Rect(buttonX, buttonStartY + buttonHeight + buttonSpacing, buttonWidth, buttonHeight);
            if (DrawButton(restartRect, "RESTART"))
            {
                OnRestartClicked();
            }

            // Settings Button
            Rect settingsRect = new Rect(buttonX, buttonStartY + (buttonHeight + buttonSpacing) * 2, buttonWidth, buttonHeight);
            if (DrawButton(settingsRect, "SETTINGS"))
            {
                OnSettingsClicked();
            }

            // Quit to Menu Button
            Rect quitRect = new Rect(buttonX, buttonStartY + (buttonHeight + buttonSpacing) * 3, buttonWidth, buttonHeight);
            if (DrawButton(quitRect, "QUIT TO MENU"))
            {
                OnQuitToMenuClicked();
            }

            // Hint
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            GUIStyle hintStyle = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 30), "Press ESC to resume", hintStyle);
            GUI.color = Color.white;
        }

        private bool DrawButton(Rect rect, string text)
        {
            bool isHovered = rect.Contains(Event.current.mousePosition);
            GUIStyle style = isHovered ? _buttonHoverStyle : _buttonStyle;

            bool clicked = GUI.Button(rect, text, style);

            if (clicked)
            {
                Audio.AudioManager.Instance?.PlayUIClick();
            }

            return clicked;
        }

        private void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        private void OnRestartClicked()
        {
            // End current session and start new one
            Time.timeScale = 1f;
            GameManager.Instance?.SetState(GameState.Playing);
            SessionManager.Instance?.StartCountdown();
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance?.ShowSettings(true);
        }

        private void OnQuitToMenuClicked()
        {
            GameManager.Instance?.ReturnToMenu();
        }

        public void Show()
        {
            _isVisible = true;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        private void OnDestroy()
        {
            if (_overlayTexture != null) Destroy(_overlayTexture);
            if (_buttonTexture != null) Destroy(_buttonTexture);
            if (_buttonHoverTexture != null) Destroy(_buttonHoverTexture);
        }
    }
}
