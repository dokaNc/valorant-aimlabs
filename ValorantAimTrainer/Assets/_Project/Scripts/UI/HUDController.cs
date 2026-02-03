using UnityEngine;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.Gameplay;

namespace ValorantAimTrainer.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("HUD Settings")]
        [SerializeField] private bool showHUD = true;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color shadowColor = Color.black;
        [SerializeField] private int fontSize = 24;

        [Header("References")]
        [SerializeField] private ShootingSystem shootingSystem;

        private SessionStats _stats;
        private float _displayTime;
        private bool _isSessionActive;
        private bool _isEliminationMode;
        private int _countdownValue;
        private bool _showCountdown;

        private GUIStyle _textStyle;
        private GUIStyle _shadowStyle;
        private GUIStyle _countdownStyle;
        private GUIStyle _weaponStyle;
        private GUIStyle _targetCountStyle;

        private void OnEnable()
        {
            EventBus.OnSessionStart += HandleSessionStart;
            EventBus.OnSessionEnd += HandleSessionEnd;
            EventBus.OnStatsUpdated += UpdateStats;
            EventBus.OnCountdownTick += HandleCountdownTick;
            EventBus.OnCountdownComplete += HandleCountdownComplete;
        }

        private void OnDisable()
        {
            EventBus.OnSessionStart -= HandleSessionStart;
            EventBus.OnSessionEnd -= HandleSessionEnd;
            EventBus.OnStatsUpdated -= UpdateStats;
            EventBus.OnCountdownTick -= HandleCountdownTick;
            EventBus.OnCountdownComplete -= HandleCountdownComplete;
        }

        private void Start()
        {
            InitializeStyles();

            if (shootingSystem == null)
            {
                shootingSystem = FindFirstObjectByType<ShootingSystem>();
            }
        }

        private void InitializeStyles()
        {
            _textStyle = new GUIStyle
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            _textStyle.normal.textColor = textColor;

            _shadowStyle = new GUIStyle(_textStyle);
            _shadowStyle.normal.textColor = shadowColor;

            _countdownStyle = new GUIStyle
            {
                fontSize = 72,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _countdownStyle.normal.textColor = textColor;

            _weaponStyle = new GUIStyle
            {
                fontSize = fontSize + 4,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight
            };
            _weaponStyle.normal.textColor = textColor;

            _targetCountStyle = new GUIStyle
            {
                fontSize = fontSize + 8,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight
            };
            _targetCountStyle.normal.textColor = new Color(0.92f, 0.29f, 0.24f);
        }

        private void Update()
        {
            if (_isSessionActive && SessionManager.Instance != null)
            {
                _isEliminationMode = SessionManager.Instance.IsEliminationMode;

                if (_isEliminationMode)
                {
                    // Elimination mode: show elapsed time
                    _displayTime = SessionManager.Instance.ElapsedTime;
                }
                else
                {
                    // Standard mode: show remaining time
                    _displayTime = SessionManager.Instance.RemainingTime;
                }

                _stats = SessionManager.Instance.CurrentStats;
            }
        }

        private void OnGUI()
        {
            if (!showHUD) return;

            if (_showCountdown)
            {
                DrawCountdown();
            }
            else if (_isSessionActive)
            {
                DrawSessionHUD();
            }
        }

        private void DrawCountdown()
        {
            string text = _countdownValue > 0 ? _countdownValue.ToString() : "GO!";

            Rect rect = new Rect(0, Screen.height / 2f - 50f, Screen.width, 100f);

            // Shadow
            GUI.color = shadowColor;
            Rect shadowRect = new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height);
            GUI.Label(shadowRect, text, _countdownStyle);

            // Main text
            GUI.color = textColor;
            GUI.Label(rect, text, _countdownStyle);
        }

        private void DrawSessionHUD()
        {
            float padding = 20f;
            float lineHeight = fontSize + 5f;

            // Timer (top center)
            string timerText = FormatTime(_displayTime);
            if (_isEliminationMode)
            {
                // Add stopwatch indicator for elimination mode
                timerText = "⏱ " + timerText;
            }

            Vector2 timerSize = _textStyle.CalcSize(new GUIContent(timerText));
            Rect timerRect = new Rect(
                (Screen.width - timerSize.x) / 2f,
                padding,
                timerSize.x,
                timerSize.y
            );
            DrawTextWithShadow(timerRect, timerText);

            // Targets remaining (top right, only for elimination mode)
            if (_isEliminationMode && _stats != null)
            {
                DrawTargetsRemaining(padding);
            }

            // Stats (top left)
            float y = padding;

            if (_stats != null)
            {
                DrawTextWithShadow(new Rect(padding, y, 300, lineHeight), $"Accuracy: {_stats.Accuracy:F1}%");
                y += lineHeight;

                DrawTextWithShadow(new Rect(padding, y, 300, lineHeight), $"Headshots: {_stats.HeadshotPercentage:F1}%");
                y += lineHeight;

                DrawTextWithShadow(new Rect(padding, y, 300, lineHeight), $"Hits: {_stats.Hits} / {_stats.TotalShots}");
                y += lineHeight;

                DrawTextWithShadow(new Rect(padding, y, 300, lineHeight), $"Targets: {_stats.TargetsHit}");
            }

            // Weapon info (bottom right)
            DrawWeaponInfo();
        }

        private void DrawTargetsRemaining(float padding)
        {
            int remaining = _stats.RemainingTargets;
            int total = _stats.TotalTargetsToEliminate;

            string text = $"{remaining}/{total}";
            string label = "TARGETS LEFT";

            // Main count
            Vector2 countSize = _targetCountStyle.CalcSize(new GUIContent(text));
            Rect countRect = new Rect(
                Screen.width - padding - countSize.x,
                padding,
                countSize.x,
                countSize.y
            );

            // Shadow
            GUI.color = shadowColor;
            GUI.Label(new Rect(countRect.x + 2, countRect.y + 2, countRect.width, countRect.height), text, _targetCountStyle);
            GUI.color = _targetCountStyle.normal.textColor;
            GUI.Label(countRect, text, _targetCountStyle);

            // Label below
            GUIStyle labelStyle = new GUIStyle(_textStyle)
            {
                fontSize = fontSize - 4,
                alignment = TextAnchor.UpperRight
            };
            labelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            Rect labelRect = new Rect(
                Screen.width - padding - 150,
                countRect.y + countRect.height,
                150,
                20
            );
            GUI.Label(labelRect, label, labelStyle);
        }

        private void DrawWeaponInfo()
        {
            if (shootingSystem == null || shootingSystem.CurrentWeapon == null) return;

            float padding = 20f;
            float bottomY = Screen.height - padding;
            float rightX = Screen.width - padding;

            var weapon = shootingSystem.CurrentWeapon;
            string weaponName = weapon.WeaponName.ToUpper();
            string ammoText;

            if (weapon.UnlimitedAmmo)
            {
                ammoText = "∞";
            }
            else
            {
                ammoText = $"{shootingSystem.CurrentAmmo} / {weapon.MagazineSize}";
            }

            // Weapon name
            GUIStyle nameStyle = new GUIStyle(_weaponStyle);
            Vector2 nameSize = nameStyle.CalcSize(new GUIContent(weaponName));
            Rect nameRect = new Rect(rightX - nameSize.x, bottomY - 60, nameSize.x, nameSize.y);

            // Shadow
            GUI.color = shadowColor;
            GUI.Label(new Rect(nameRect.x + 2, nameRect.y + 2, nameRect.width, nameRect.height), weaponName, nameStyle);
            GUI.color = textColor;
            GUI.Label(nameRect, weaponName, nameStyle);

            // Ammo count
            GUIStyle ammoStyle = new GUIStyle(_weaponStyle);
            ammoStyle.fontSize = fontSize + 12;
            Vector2 ammoSize = ammoStyle.CalcSize(new GUIContent(ammoText));
            Rect ammoRect = new Rect(rightX - ammoSize.x, bottomY - 30, ammoSize.x, ammoSize.y);

            // Shadow
            GUI.color = shadowColor;
            GUI.Label(new Rect(ammoRect.x + 2, ammoRect.y + 2, ammoRect.width, ammoRect.height), ammoText, ammoStyle);
            GUI.color = textColor;
            GUI.Label(ammoRect, ammoText, ammoStyle);
        }

        private void DrawTextWithShadow(Rect rect, string text)
        {
            // Shadow
            Rect shadowRect = new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height);
            GUI.Label(shadowRect, text, _shadowStyle);

            // Main text
            GUI.Label(rect, text, _textStyle);
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);

            if (_isEliminationMode)
            {
                // Show milliseconds for elimination mode
                int ms = Mathf.FloorToInt((time % 1f) * 100f);
                return $"{minutes:00}:{seconds:00}.{ms:00}";
            }
            else
            {
                return $"{minutes:00}:{seconds:00}";
            }
        }

        private void HandleSessionStart()
        {
            _isSessionActive = true;
            _showCountdown = false;
            _isEliminationMode = SessionManager.Instance?.IsEliminationMode ?? false;
        }

        private void HandleSessionEnd()
        {
            _isSessionActive = false;
        }

        private void HandleCountdownTick(int seconds)
        {
            _countdownValue = seconds;
            _showCountdown = true;
        }

        private void HandleCountdownComplete()
        {
            _countdownValue = 0;
            // Show "GO!" briefly
            Invoke(nameof(HideCountdown), 0.5f);
        }

        private void HideCountdown()
        {
            _showCountdown = false;
        }

        private void UpdateStats()
        {
            _stats = SessionManager.Instance?.CurrentStats;
        }

        public void ShowHUD()
        {
            showHUD = true;
        }

        public void HideHUD()
        {
            showHUD = false;
        }
    }
}
