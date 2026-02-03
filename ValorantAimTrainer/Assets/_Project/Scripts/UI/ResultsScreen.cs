using UnityEngine;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.Gameplay;

namespace ValorantAimTrainer.UI
{
    public class ResultsScreen : MonoBehaviour
    {
        private bool _isVisible = false;
        private SessionStats _stats;

        private GUIStyle _titleStyle;
        private GUIStyle _statLabelStyle;
        private GUIStyle _statValueStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonHoverStyle;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;
        private Texture2D _panelTexture;

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
            EventBus.OnSessionEnd += CaptureStats;
        }

        private void OnDisable()
        {
            EventBus.OnSessionEnd -= CaptureStats;
        }

        private void CaptureStats()
        {
            _stats = SessionManager.Instance?.CurrentStats;
        }

        private void CreateTextures()
        {
            _panelTexture = new Texture2D(1, 1);
            _panelTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.95f));
            _panelTexture.Apply();

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

            _statLabelStyle = new GUIStyle
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleLeft
            };
            _statLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            _statValueStyle = new GUIStyle
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            _statValueStyle.normal.textColor = Color.white;

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

            // Background
            GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;

            // Title
            string title = _stats?.IsEliminationMode == true ? "ELIMINATION COMPLETE" : "SESSION COMPLETE";
            Rect titleRect = new Rect(0, 60, Screen.width, 60);
            GUI.Label(titleRect, title, _titleStyle);

            // Stats Panel
            float panelWidth = 500;
            float panelHeight = 350;
            float panelX = centerX - panelWidth / 2f;
            float panelY = centerY - panelHeight / 2f - 20;

            GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), _panelTexture);

            // Stats
            float statY = panelY + 30;
            float statSpacing = 45;
            float labelX = panelX + 40;
            float valueX = panelX + panelWidth - 140;

            if (_stats != null)
            {
                // Completion Time (only for elimination mode)
                if (_stats.IsEliminationMode)
                {
                    string timeStr = FormatCompletionTime(_stats.CompletionTime);
                    DrawStat(labelX, statY, valueX, "COMPLETION TIME", timeStr, GetCompletionTimeColor(_stats.CompletionTime));
                    statY += statSpacing;
                }

                // Accuracy
                DrawStat(labelX, statY, valueX, "ACCURACY", $"{_stats.Accuracy:F1}%", GetAccuracyColor(_stats.Accuracy));
                statY += statSpacing;

                // Headshot %
                DrawStat(labelX, statY, valueX, "HEADSHOT %", $"{_stats.HeadshotPercentage:F1}%", GetHeadshotColor(_stats.HeadshotPercentage));
                statY += statSpacing;

                // Hits / Shots
                DrawStat(labelX, statY, valueX, "HITS / SHOTS", $"{_stats.Hits} / {_stats.TotalShots}", Color.white);
                statY += statSpacing;

                // Targets Killed
                DrawStat(labelX, statY, valueX, "TARGETS KILLED", $"{_stats.TargetsHit}", Color.white);
                statY += statSpacing;

                // Targets Missed (skip for elimination mode where targets don't expire)
                if (!_stats.IsEliminationMode)
                {
                    DrawStat(labelX, statY, valueX, "TARGETS MISSED", $"{_stats.TargetsMissed}", Color.white);
                    statY += statSpacing;
                }

                // Average Reaction Time
                DrawStat(labelX, statY, valueX, "AVG REACTION", $"{_stats.AverageReactionTime:F0}ms", GetReactionColor(_stats.AverageReactionTime));
                statY += statSpacing;

                // Targets Per Minute
                DrawStat(labelX, statY, valueX, "TARGETS/MIN", $"{_stats.TargetsPerMinute:F1}", Color.white);
            }

            // Buttons
            float buttonWidth = 200;
            float buttonHeight = 50;
            float buttonSpacing = 20;
            float buttonY = panelY + panelHeight + 40;

            // Play Again Button
            Rect playAgainRect = new Rect(centerX - buttonWidth - buttonSpacing / 2f, buttonY, buttonWidth, buttonHeight);
            if (DrawButton(playAgainRect, "PLAY AGAIN"))
            {
                OnPlayAgainClicked();
            }

            // Main Menu Button
            Rect menuRect = new Rect(centerX + buttonSpacing / 2f, buttonY, buttonWidth, buttonHeight);
            if (DrawButton(menuRect, "MAIN MENU"))
            {
                OnMainMenuClicked();
            }
        }

        private void DrawStat(float labelX, float y, float valueX, string label, string value, Color valueColor)
        {
            GUI.Label(new Rect(labelX, y, 200, 30), label, _statLabelStyle);

            Color originalColor = _statValueStyle.normal.textColor;
            _statValueStyle.normal.textColor = valueColor;
            GUI.Label(new Rect(valueX, y, 100, 30), value, _statValueStyle);
            _statValueStyle.normal.textColor = originalColor;
        }

        private Color GetAccuracyColor(float accuracy)
        {
            if (accuracy >= 80f) return new Color(0.2f, 1f, 0.2f);      // Green
            if (accuracy >= 50f) return new Color(1f, 0.9f, 0.2f);      // Yellow
            return new Color(1f, 0.4f, 0.4f);                            // Red
        }

        private Color GetHeadshotColor(float percentage)
        {
            if (percentage >= 50f) return new Color(0.2f, 1f, 0.2f);
            if (percentage >= 25f) return new Color(1f, 0.9f, 0.2f);
            return new Color(1f, 0.4f, 0.4f);
        }

        private Color GetReactionColor(float ms)
        {
            if (ms <= 300f) return new Color(0.2f, 1f, 0.2f);
            if (ms <= 500f) return new Color(1f, 0.9f, 0.2f);
            return new Color(1f, 0.4f, 0.4f);
        }

        private Color GetCompletionTimeColor(float seconds)
        {
            // Green if under 30s, Yellow if under 60s, Red otherwise
            if (seconds <= 30f) return new Color(0.2f, 1f, 0.2f);
            if (seconds <= 60f) return new Color(1f, 0.9f, 0.2f);
            return new Color(1f, 0.4f, 0.4f);
        }

        private string FormatCompletionTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int ms = Mathf.FloorToInt((time % 1f) * 100f);
            return $"{minutes:00}:{seconds:00}.{ms:00}";
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

        private void OnPlayAgainClicked()
        {
            // Check if we were in elimination mode
            if (_stats?.IsEliminationMode == true)
            {
                // Reconfigure spawner for elimination
                int targetCount = _stats.TotalTargetsToEliminate;
                TargetSpawner.Instance?.ConfigureForElimination(targetCount, false);

                // Start elimination session
                SessionManager.Instance?.StartEliminationCountdown(targetCount);
            }
            else
            {
                // Standard mode
                TargetSpawner.Instance?.ResetToStandardMode();
                SessionManager.Instance?.StartCountdown();
            }
        }

        private void OnMainMenuClicked()
        {
            GameManager.Instance?.ReturnToMenu();
        }

        public void Show()
        {
            _isVisible = true;
            CaptureStats();
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
