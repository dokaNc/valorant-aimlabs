using UnityEngine;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Gameplay;

namespace ValorantAimTrainer.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private string gameTitle = "VALORANT AIM TRAINER";
        [SerializeField] private string gameSubtitle = "Practice your aim";

        [Header("Elimination Mode")]
        [SerializeField] private int eliminationTargetCount = 2;

        private bool _isVisible = false;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonHoverStyle;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;

        private Rect _playButtonRect;
        private Rect _eliminationButtonRect;
        private Rect _settingsButtonRect;
        private Rect _quitButtonRect;

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
            _buttonTexture = new Texture2D(1, 1);
            _buttonTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 0.9f));
            _buttonTexture.Apply();

            _buttonHoverTexture = new Texture2D(1, 1);
            _buttonHoverTexture.SetPixel(0, 0, new Color(0.92f, 0.29f, 0.24f, 1f)); // Valorant red
            _buttonHoverTexture.Apply();
        }

        private void InitializeStyles()
        {
            _titleStyle = new GUIStyle
            {
                fontSize = 64,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = Color.white;

            _subtitleStyle = new GUIStyle
            {
                fontSize = 24,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            _subtitleStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            _buttonStyle = new GUIStyle
            {
                fontSize = 28,
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
            GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;

            // Title
            Rect titleRect = new Rect(0, centerY - 200, Screen.width, 80);
            GUI.Label(titleRect, gameTitle, _titleStyle);

            // Subtitle
            Rect subtitleRect = new Rect(0, centerY - 130, Screen.width, 40);
            GUI.Label(subtitleRect, gameSubtitle, _subtitleStyle);

            // Buttons
            float buttonWidth = 300;
            float buttonHeight = 60;
            float buttonSpacing = 20;
            float buttonX = centerX - buttonWidth / 2f;
            float buttonStartY = centerY - 20;

            // Play Button (Standard mode with timer)
            _playButtonRect = new Rect(buttonX, buttonStartY, buttonWidth, buttonHeight);
            if (DrawButton(_playButtonRect, "PLAY"))
            {
                OnPlayClicked();
            }

            // Elimination Button
            _eliminationButtonRect = new Rect(buttonX, buttonStartY + buttonHeight + buttonSpacing, buttonWidth, buttonHeight);
            if (DrawButton(_eliminationButtonRect, "ELIMINATION"))
            {
                OnEliminationClicked();
            }

            // Settings Button
            _settingsButtonRect = new Rect(buttonX, buttonStartY + (buttonHeight + buttonSpacing) * 2, buttonWidth, buttonHeight);
            if (DrawButton(_settingsButtonRect, "SETTINGS"))
            {
                OnSettingsClicked();
            }

            // Quit Button
            _quitButtonRect = new Rect(buttonX, buttonStartY + (buttonHeight + buttonSpacing) * 3, buttonWidth, buttonHeight);
            if (DrawButton(_quitButtonRect, "QUIT"))
            {
                OnQuitClicked();
            }

            // Version
            GUI.color = new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(10, Screen.height - 30, 200, 20), "v0.1.0 - MVP");
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
            else if (isHovered && Event.current.type == EventType.Repaint)
            {
                // Could play hover sound here if desired
            }

            return clicked;
        }

        private void OnPlayClicked()
        {
            // Start the game session (standard mode with countdown timer)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectMode(TrainingMode.Flick);
                GameManager.Instance.SetState(GameState.Loading);

                // Reset spawner to standard mode
                TargetSpawner.Instance?.ResetToStandardMode();

                // Start standard countdown session
                SessionManager.Instance?.StartCountdown();
            }
        }

        private void OnEliminationClicked()
        {
            // Start elimination mode (kill all targets, stopwatch timer)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectMode(TrainingMode.Elimination);
                GameManager.Instance.SetState(GameState.Loading);

                // Configure spawner for elimination (fixed targets, no strafing)
                TargetSpawner.Instance?.ConfigureForElimination(eliminationTargetCount, false);

                // Start elimination session
                SessionManager.Instance?.StartEliminationCountdown(eliminationTargetCount);
            }
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance?.ShowSettings(false);
        }

        private void OnQuitClicked()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
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
            if (_buttonTexture != null) Destroy(_buttonTexture);
            if (_buttonHoverTexture != null) Destroy(_buttonHoverTexture);
        }
    }
}
