using UnityEngine;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.Audio;

namespace ValorantAimTrainer.UI
{
    /// <summary>
    /// Complete Valorant-style crosshair settings panel with import/export.
    /// Supports separate vertical and horizontal line customization.
    /// </summary>
    public class CrosshairSettingsPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CrosshairSettings crosshairSettings;
        [SerializeField] private CrosshairRenderer crosshairRenderer;

        private bool _isVisible = false;
        private Vector2 _scrollPosition;
        private string _importExportCode = "";
        private string _statusMessage = "";
        private float _statusMessageTimer = 0f;

        // Temp values for live preview
        private CrosshairSettings.CrosshairColor _tempColor;
        private bool _tempOutlineEnabled;
        private float _tempOutlineOpacity;
        private float _tempOutlineThickness;
        private bool _tempCenterDotEnabled;
        private float _tempCenterDotOpacity;
        private float _tempCenterDotThickness;

        // Inner Lines
        private bool _tempInnerLinesEnabled;
        private float _tempInnerLinesOpacity;
        private bool _tempInnerLinesShowVertical;
        private float _tempInnerLinesVerticalLength;
        private float _tempInnerLinesVerticalThickness;
        private float _tempInnerLinesVerticalOffset;
        private bool _tempInnerLinesShowHorizontal;
        private float _tempInnerLinesHorizontalLength;
        private float _tempInnerLinesHorizontalThickness;
        private float _tempInnerLinesHorizontalOffset;
        private bool _tempInnerLinesMovementError;
        private bool _tempInnerLinesFiringError;

        // Outer Lines
        private bool _tempOuterLinesEnabled;
        private float _tempOuterLinesOpacity;
        private bool _tempOuterLinesShowVertical;
        private float _tempOuterLinesVerticalLength;
        private float _tempOuterLinesVerticalThickness;
        private float _tempOuterLinesVerticalOffset;
        private bool _tempOuterLinesShowHorizontal;
        private float _tempOuterLinesHorizontalLength;
        private float _tempOuterLinesHorizontalThickness;
        private float _tempOuterLinesHorizontalOffset;
        private bool _tempOuterLinesMovementError;
        private bool _tempOuterLinesFiringError;

        private bool _tempFiringErrorEnabled;
        private bool _tempMovementErrorEnabled;

        // UI Styles
        private GUIStyle _titleStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _subSectionStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonHoverStyle;
        private GUIStyle _textFieldStyle;
        private GUIStyle _colorButtonStyle;
        private GUIStyle _statusStyle;
        private Texture2D _panelTexture;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;
        private Texture2D _textFieldTexture;
        private Texture2D[] _colorTextures;

        private readonly string[] _colorNames = { "White", "Green", "Yellow Green", "Green Yellow", "Yellow", "Cyan", "Pink", "Red" };

        private void Awake()
        {
            CreateTextures();
        }

        private void Start()
        {
            InitializeStyles();
            LoadCurrentSettings();
        }

        private void Update()
        {
            if (_statusMessageTimer > 0)
            {
                _statusMessageTimer -= Time.unscaledDeltaTime;
            }
        }

        private void CreateTextures()
        {
            _panelTexture = CreateColorTexture(new Color(0.08f, 0.08f, 0.12f, 0.98f));
            _buttonTexture = CreateColorTexture(new Color(0.2f, 0.2f, 0.25f, 1f));
            _buttonHoverTexture = CreateColorTexture(new Color(0.92f, 0.29f, 0.24f, 1f));
            _textFieldTexture = CreateColorTexture(new Color(0.15f, 0.15f, 0.2f, 1f));

            _colorTextures = new Texture2D[CrosshairSettings.ColorPresets.Length];
            for (int i = 0; i < CrosshairSettings.ColorPresets.Length; i++)
            {
                _colorTextures[i] = CreateColorTexture(CrosshairSettings.ColorPresets[i]);
            }
        }

        private Texture2D CreateColorTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void InitializeStyles()
        {
            _titleStyle = new GUIStyle
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = Color.white;

            _sectionStyle = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _sectionStyle.normal.textColor = new Color(0.92f, 0.29f, 0.24f);

            _subSectionStyle = new GUIStyle
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _subSectionStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            _labelStyle = new GUIStyle
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft
            };
            _labelStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

            _valueStyle = new GUIStyle
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            _valueStyle.normal.textColor = Color.white;

            _buttonStyle = new GUIStyle
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.normal.background = _buttonTexture;
            _buttonStyle.padding = new RectOffset(6, 6, 3, 3);

            _buttonHoverStyle = new GUIStyle(_buttonStyle);
            _buttonHoverStyle.normal.background = _buttonHoverTexture;

            _textFieldStyle = new GUIStyle
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            _textFieldStyle.normal.textColor = Color.white;
            _textFieldStyle.normal.background = _textFieldTexture;
            _textFieldStyle.padding = new RectOffset(6, 6, 3, 3);

            _colorButtonStyle = new GUIStyle
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };
            _colorButtonStyle.normal.textColor = Color.black;

            _statusStyle = new GUIStyle
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void LoadCurrentSettings()
        {
            if (crosshairSettings == null) return;

            _tempColor = crosshairSettings.ColorPreset;
            _tempOutlineEnabled = crosshairSettings.OutlineEnabled;
            _tempOutlineOpacity = crosshairSettings.OutlineOpacity;
            _tempOutlineThickness = crosshairSettings.OutlineThickness;
            _tempCenterDotEnabled = crosshairSettings.CenterDotEnabled;
            _tempCenterDotOpacity = crosshairSettings.CenterDotOpacity;
            _tempCenterDotThickness = crosshairSettings.CenterDotThickness;

            // Inner Lines
            _tempInnerLinesEnabled = crosshairSettings.InnerLinesEnabled;
            _tempInnerLinesOpacity = crosshairSettings.InnerLinesOpacity;
            _tempInnerLinesShowVertical = crosshairSettings.InnerLinesShowVertical;
            _tempInnerLinesVerticalLength = crosshairSettings.InnerLinesVerticalLength;
            _tempInnerLinesVerticalThickness = crosshairSettings.InnerLinesVerticalThickness;
            _tempInnerLinesVerticalOffset = crosshairSettings.InnerLinesVerticalOffset;
            _tempInnerLinesShowHorizontal = crosshairSettings.InnerLinesShowHorizontal;
            _tempInnerLinesHorizontalLength = crosshairSettings.InnerLinesHorizontalLength;
            _tempInnerLinesHorizontalThickness = crosshairSettings.InnerLinesHorizontalThickness;
            _tempInnerLinesHorizontalOffset = crosshairSettings.InnerLinesHorizontalOffset;
            _tempInnerLinesMovementError = crosshairSettings.InnerLinesMovementError;
            _tempInnerLinesFiringError = crosshairSettings.InnerLinesFiringError;

            // Outer Lines
            _tempOuterLinesEnabled = crosshairSettings.OuterLinesEnabled;
            _tempOuterLinesOpacity = crosshairSettings.OuterLinesOpacity;
            _tempOuterLinesShowVertical = crosshairSettings.OuterLinesShowVertical;
            _tempOuterLinesVerticalLength = crosshairSettings.OuterLinesVerticalLength;
            _tempOuterLinesVerticalThickness = crosshairSettings.OuterLinesVerticalThickness;
            _tempOuterLinesVerticalOffset = crosshairSettings.OuterLinesVerticalOffset;
            _tempOuterLinesShowHorizontal = crosshairSettings.OuterLinesShowHorizontal;
            _tempOuterLinesHorizontalLength = crosshairSettings.OuterLinesHorizontalLength;
            _tempOuterLinesHorizontalThickness = crosshairSettings.OuterLinesHorizontalThickness;
            _tempOuterLinesHorizontalOffset = crosshairSettings.OuterLinesHorizontalOffset;
            _tempOuterLinesMovementError = crosshairSettings.OuterLinesMovementError;
            _tempOuterLinesFiringError = crosshairSettings.OuterLinesFiringError;

            _tempFiringErrorEnabled = crosshairSettings.FiringErrorEnabled;
            _tempMovementErrorEnabled = crosshairSettings.MovementErrorEnabled;

            _importExportCode = crosshairSettings.ExportToValorantCode();
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _panelTexture);

            float panelWidth = 750f;
            float panelHeight = Screen.height - 100f;
            float panelX = (Screen.width - panelWidth) / 2f;
            float panelY = 50f;

            GUI.Label(new Rect(0, 10, Screen.width, 40), "CROSSHAIR SETTINGS", _titleStyle);

            Rect scrollViewRect = new Rect(panelX, panelY, panelWidth, panelHeight - 60);
            Rect contentRect = new Rect(0, 0, panelWidth - 30, 1600);

            _scrollPosition = GUI.BeginScrollView(scrollViewRect, _scrollPosition, contentRect);

            float y = 10f;
            float leftCol = 20f;
            float colWidth = panelWidth - 60f;

            // === IMPORT/EXPORT ===
            y = DrawSection(leftCol, y, colWidth, "CROSSHAIR CODE");
            GUI.Label(new Rect(leftCol, y, 120, 18), "Valorant Code:", _labelStyle);
            y += 22;
            _importExportCode = GUI.TextField(new Rect(leftCol, y, colWidth, 35), _importExportCode, _textFieldStyle);
            y += 42;

            if (DrawButton(new Rect(leftCol, y, 90, 26), "IMPORT")) ImportCode();
            if (DrawButton(new Rect(leftCol + 100, y, 90, 26), "EXPORT")) ExportCode();
            if (DrawButton(new Rect(leftCol + 200, y, 90, 26), "COPY")) CopyToClipboard();
            if (DrawButton(new Rect(leftCol + 300, y, 90, 26), "RESET")) ResetToDefault();
            y += 32;

            if (_statusMessageTimer > 0 && !string.IsNullOrEmpty(_statusMessage))
            {
                _statusStyle.normal.textColor = _statusMessage.StartsWith("Error") ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 1f, 0.4f);
                GUI.Label(new Rect(leftCol, y, colWidth, 22), _statusMessage, _statusStyle);
            }
            y += 30;

            // === COLOR ===
            y = DrawSection(leftCol, y, colWidth, "COLOR");
            for (int i = 0; i < _colorNames.Length; i++)
            {
                float btnX = leftCol + (i % 4) * 85;
                float btnY = y + (i / 4) * 30;
                Rect colorRect = new Rect(btnX, btnY, 80, 26);
                bool isSelected = ((int)_tempColor == i);
                _colorButtonStyle.normal.background = _colorTextures[i];
                if (isSelected)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(new Rect(btnX - 2, btnY - 2, 84, 30), Texture2D.whiteTexture);
                }
                GUI.color = Color.white;
                if (GUI.Button(colorRect, _colorNames[i], _colorButtonStyle))
                {
                    _tempColor = (CrosshairSettings.CrosshairColor)i;
                    ApplyPreview();
                    AudioManager.Instance?.PlayUIClick();
                }
            }
            y += 70;

            // === OUTLINES ===
            y = DrawSection(leftCol, y, colWidth, "OUTLINES");
            _tempOutlineEnabled = DrawToggle(leftCol, y, "Show Outlines", _tempOutlineEnabled);
            y += 26;
            if (_tempOutlineEnabled)
            {
                _tempOutlineOpacity = DrawSlider(leftCol, y, "Opacity", _tempOutlineOpacity, 0f, 1f, "{0:P0}");
                y += 26;
                _tempOutlineThickness = DrawSlider(leftCol, y, "Thickness", _tempOutlineThickness, 1f, 6f, "{0:F0}");
                y += 26;
            }
            y += 10;

            // === CENTER DOT ===
            y = DrawSection(leftCol, y, colWidth, "CENTER DOT");
            _tempCenterDotEnabled = DrawToggle(leftCol, y, "Show Center Dot", _tempCenterDotEnabled);
            y += 26;
            if (_tempCenterDotEnabled)
            {
                _tempCenterDotOpacity = DrawSlider(leftCol, y, "Opacity", _tempCenterDotOpacity, 0f, 1f, "{0:P0}");
                y += 26;
                _tempCenterDotThickness = DrawSlider(leftCol, y, "Thickness", _tempCenterDotThickness, 1f, 6f, "{0:F0}");
                y += 26;
            }
            y += 10;

            // === INNER LINES ===
            y = DrawSection(leftCol, y, colWidth, "INNER LINES");
            _tempInnerLinesEnabled = DrawToggle(leftCol, y, "Show Inner Lines", _tempInnerLinesEnabled);
            y += 26;

            if (_tempInnerLinesEnabled)
            {
                _tempInnerLinesOpacity = DrawSlider(leftCol, y, "Opacity", _tempInnerLinesOpacity, 0f, 1f, "{0:P0}");
                y += 30;

                // Vertical (Top/Bottom)
                y = DrawSubSection(leftCol, y, "Vertical Lines (Top/Bottom)");
                _tempInnerLinesShowVertical = DrawToggle(leftCol, y, "Show", _tempInnerLinesShowVertical);
                y += 26;
                if (_tempInnerLinesShowVertical)
                {
                    _tempInnerLinesVerticalLength = DrawSlider(leftCol, y, "Length", _tempInnerLinesVerticalLength, 1f, 20f, "{0:F0}");
                    y += 26;
                    _tempInnerLinesVerticalThickness = DrawSlider(leftCol, y, "Thickness", _tempInnerLinesVerticalThickness, 1f, 10f, "{0:F0}");
                    y += 26;
                    _tempInnerLinesVerticalOffset = DrawSlider(leftCol, y, "Offset", _tempInnerLinesVerticalOffset, 0f, 20f, "{0:F0}");
                    y += 26;
                }

                // Horizontal (Left/Right)
                y = DrawSubSection(leftCol, y, "Horizontal Lines (Left/Right)");
                _tempInnerLinesShowHorizontal = DrawToggle(leftCol, y, "Show", _tempInnerLinesShowHorizontal);
                y += 26;
                if (_tempInnerLinesShowHorizontal)
                {
                    _tempInnerLinesHorizontalLength = DrawSlider(leftCol, y, "Length", _tempInnerLinesHorizontalLength, 1f, 20f, "{0:F0}");
                    y += 26;
                    _tempInnerLinesHorizontalThickness = DrawSlider(leftCol, y, "Thickness", _tempInnerLinesHorizontalThickness, 1f, 10f, "{0:F0}");
                    y += 26;
                    _tempInnerLinesHorizontalOffset = DrawSlider(leftCol, y, "Offset", _tempInnerLinesHorizontalOffset, 0f, 20f, "{0:F0}");
                    y += 26;
                }

                // Error
                _tempInnerLinesMovementError = DrawToggle(leftCol, y, "Movement Error", _tempInnerLinesMovementError);
                _tempInnerLinesFiringError = DrawToggle(leftCol + 180, y, "Firing Error", _tempInnerLinesFiringError);
                y += 26;
            }
            y += 10;

            // === OUTER LINES ===
            y = DrawSection(leftCol, y, colWidth, "OUTER LINES");
            _tempOuterLinesEnabled = DrawToggle(leftCol, y, "Show Outer Lines", _tempOuterLinesEnabled);
            y += 26;

            if (_tempOuterLinesEnabled)
            {
                _tempOuterLinesOpacity = DrawSlider(leftCol, y, "Opacity", _tempOuterLinesOpacity, 0f, 1f, "{0:P0}");
                y += 30;

                // Vertical
                y = DrawSubSection(leftCol, y, "Vertical Lines (Top/Bottom)");
                _tempOuterLinesShowVertical = DrawToggle(leftCol, y, "Show", _tempOuterLinesShowVertical);
                y += 26;
                if (_tempOuterLinesShowVertical)
                {
                    _tempOuterLinesVerticalLength = DrawSlider(leftCol, y, "Length", _tempOuterLinesVerticalLength, 1f, 20f, "{0:F0}");
                    y += 26;
                    _tempOuterLinesVerticalThickness = DrawSlider(leftCol, y, "Thickness", _tempOuterLinesVerticalThickness, 1f, 10f, "{0:F0}");
                    y += 26;
                    _tempOuterLinesVerticalOffset = DrawSlider(leftCol, y, "Offset", _tempOuterLinesVerticalOffset, 0f, 40f, "{0:F0}");
                    y += 26;
                }

                // Horizontal
                y = DrawSubSection(leftCol, y, "Horizontal Lines (Left/Right)");
                _tempOuterLinesShowHorizontal = DrawToggle(leftCol, y, "Show", _tempOuterLinesShowHorizontal);
                y += 26;
                if (_tempOuterLinesShowHorizontal)
                {
                    _tempOuterLinesHorizontalLength = DrawSlider(leftCol, y, "Length", _tempOuterLinesHorizontalLength, 1f, 20f, "{0:F0}");
                    y += 26;
                    _tempOuterLinesHorizontalThickness = DrawSlider(leftCol, y, "Thickness", _tempOuterLinesHorizontalThickness, 1f, 10f, "{0:F0}");
                    y += 26;
                    _tempOuterLinesHorizontalOffset = DrawSlider(leftCol, y, "Offset", _tempOuterLinesHorizontalOffset, 0f, 40f, "{0:F0}");
                    y += 26;
                }

                _tempOuterLinesMovementError = DrawToggle(leftCol, y, "Movement Error", _tempOuterLinesMovementError);
                _tempOuterLinesFiringError = DrawToggle(leftCol + 180, y, "Firing Error", _tempOuterLinesFiringError);
                y += 26;
            }
            y += 10;

            // === ERROR SETTINGS ===
            y = DrawSection(leftCol, y, colWidth, "ERROR SETTINGS");
            _tempFiringErrorEnabled = DrawToggle(leftCol, y, "Firing Error", _tempFiringErrorEnabled);
            y += 26;
            _tempMovementErrorEnabled = DrawToggle(leftCol, y, "Movement Error", _tempMovementErrorEnabled);
            y += 40;

            GUI.EndScrollView();

            // Buttons
            float buttonY = Screen.height - 60;
            if (DrawButton(new Rect(panelX, buttonY, 140, 38), "APPLY")) ApplySettings();
            if (DrawButton(new Rect(panelX + panelWidth - 140, buttonY, 140, 38), "BACK")) OnBackClicked();

            DrawCrosshairPreview();
        }

        private float DrawSection(float x, float y, float width, string title)
        {
            GUI.Label(new Rect(x, y, width, 22), title, _sectionStyle);
            GUI.color = new Color(0.92f, 0.29f, 0.24f, 0.5f);
            GUI.DrawTexture(new Rect(x, y + 22, width, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;
            return y + 30;
        }

        private float DrawSubSection(float x, float y, string title)
        {
            GUI.Label(new Rect(x + 10, y, 300, 20), title, _subSectionStyle);
            return y + 24;
        }

        private float DrawSlider(float x, float y, string label, float value, float min, float max, string format)
        {
            GUI.Label(new Rect(x, y, 120, 18), label, _labelStyle);
            float newValue = GUI.HorizontalSlider(new Rect(x + 130, y + 2, 180, 18), value, min, max);
            GUI.Label(new Rect(x + 320, y, 50, 18), string.Format(format, newValue), _valueStyle);
            if (Mathf.Abs(newValue - value) > 0.001f) ApplyPreview();
            return newValue;
        }

        private bool DrawToggle(float x, float y, string label, bool value)
        {
            GUI.Label(new Rect(x, y, 120, 18), label, _labelStyle);
            Rect btnRect = new Rect(x + 130, y, 50, 20);
            bool isHovered = btnRect.Contains(Event.current.mousePosition);
            GUIStyle style = (value || isHovered) ? _buttonHoverStyle : _buttonStyle;
            if (GUI.Button(btnRect, value ? "ON" : "OFF", style))
            {
                value = !value;
                ApplyPreview();
                AudioManager.Instance?.PlayUIClick();
            }
            return value;
        }

        private bool DrawButton(Rect rect, string text)
        {
            bool isHovered = rect.Contains(Event.current.mousePosition);
            GUIStyle style = isHovered ? _buttonHoverStyle : _buttonStyle;
            bool clicked = GUI.Button(rect, text, style);
            if (clicked) AudioManager.Instance?.PlayUIClick();
            return clicked;
        }

        private void DrawCrosshairPreview()
        {
            float previewSize = 150;
            float previewX = Screen.width - previewSize - 20;
            float previewY = 60;

            GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            GUI.DrawTexture(new Rect(previewX - 5, previewY - 5, previewSize + 10, previewSize + 10), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var previewLabelStyle = new GUIStyle(_labelStyle) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(previewX, previewY - 25, previewSize, 20), "PREVIEW", previewLabelStyle);

            Vector2 center = new Vector2(previewX + previewSize / 2f, previewY + previewSize / 2f);
            DrawCrosshairInPreview(center);
        }

        private void DrawCrosshairInPreview(Vector2 center)
        {
            if (crosshairSettings == null) return;

            Color crosshairColor = _tempColor == CrosshairSettings.CrosshairColor.Custom
                ? crosshairSettings.CustomColor
                : CrosshairSettings.ColorPresets[(int)_tempColor];

            // Outline
            if (_tempOutlineEnabled)
            {
                Color outlineColor = new Color(0f, 0f, 0f, _tempOutlineOpacity);
                GUI.color = outlineColor;
                float ol = _tempOutlineThickness;

                if (_tempCenterDotEnabled)
                {
                    float ds = _tempCenterDotThickness + ol * 2f;
                    GUI.DrawTexture(new Rect(center.x - ds / 2f, center.y - ds / 2f, ds, ds), Texture2D.whiteTexture);
                }

                if (_tempInnerLinesEnabled)
                {
                    if (_tempInnerLinesShowVertical)
                    {
                        float l = _tempInnerLinesVerticalLength + ol * 2f;
                        float t = _tempInnerLinesVerticalThickness + ol * 2f;
                        float o = _tempInnerLinesVerticalOffset - ol;
                        GUI.DrawTexture(new Rect(center.x - t / 2f, center.y - o - l + ol, t, l), Texture2D.whiteTexture);
                        GUI.DrawTexture(new Rect(center.x - t / 2f, center.y + o - ol, t, l), Texture2D.whiteTexture);
                    }
                    if (_tempInnerLinesShowHorizontal)
                    {
                        float l = _tempInnerLinesHorizontalLength + ol * 2f;
                        float t = _tempInnerLinesHorizontalThickness + ol * 2f;
                        float o = _tempInnerLinesHorizontalOffset - ol;
                        GUI.DrawTexture(new Rect(center.x - o - l + ol, center.y - t / 2f, l, t), Texture2D.whiteTexture);
                        GUI.DrawTexture(new Rect(center.x + o - ol, center.y - t / 2f, l, t), Texture2D.whiteTexture);
                    }
                }
            }

            // Main crosshair
            if (_tempCenterDotEnabled)
            {
                Color dotColor = crosshairColor;
                dotColor.a = _tempCenterDotOpacity;
                GUI.color = dotColor;
                float ds = _tempCenterDotThickness;
                GUI.DrawTexture(new Rect(center.x - ds / 2f, center.y - ds / 2f, ds, ds), Texture2D.whiteTexture);
            }

            if (_tempInnerLinesEnabled)
            {
                Color c = crosshairColor;
                c.a = _tempInnerLinesOpacity;
                GUI.color = c;

                if (_tempInnerLinesShowVertical)
                {
                    float l = _tempInnerLinesVerticalLength;
                    float t = _tempInnerLinesVerticalThickness;
                    float o = _tempInnerLinesVerticalOffset;
                    GUI.DrawTexture(new Rect(center.x - t / 2f, center.y - o - l, t, l), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(center.x - t / 2f, center.y + o, t, l), Texture2D.whiteTexture);
                }
                if (_tempInnerLinesShowHorizontal)
                {
                    float l = _tempInnerLinesHorizontalLength;
                    float t = _tempInnerLinesHorizontalThickness;
                    float o = _tempInnerLinesHorizontalOffset;
                    GUI.DrawTexture(new Rect(center.x - o - l, center.y - t / 2f, l, t), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(center.x + o, center.y - t / 2f, l, t), Texture2D.whiteTexture);
                }
            }

            if (_tempOuterLinesEnabled)
            {
                Color c = crosshairColor;
                c.a = _tempOuterLinesOpacity;
                GUI.color = c;

                if (_tempOuterLinesShowVertical)
                {
                    float l = _tempOuterLinesVerticalLength;
                    float t = _tempOuterLinesVerticalThickness;
                    float o = _tempOuterLinesVerticalOffset;
                    GUI.DrawTexture(new Rect(center.x - t / 2f, center.y - o - l, t, l), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(center.x - t / 2f, center.y + o, t, l), Texture2D.whiteTexture);
                }
                if (_tempOuterLinesShowHorizontal)
                {
                    float l = _tempOuterLinesHorizontalLength;
                    float t = _tempOuterLinesHorizontalThickness;
                    float o = _tempOuterLinesHorizontalOffset;
                    GUI.DrawTexture(new Rect(center.x - o - l, center.y - t / 2f, l, t), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(center.x + o, center.y - t / 2f, l, t), Texture2D.whiteTexture);
                }
            }

            GUI.color = Color.white;
        }

        private void ApplyPreview()
        {
            if (crosshairSettings == null) return;

            crosshairSettings.SetColorPreset(_tempColor);
            crosshairSettings.SetOutlineEnabled(_tempOutlineEnabled);
            crosshairSettings.SetOutlineOpacity(_tempOutlineOpacity);
            crosshairSettings.SetOutlineThickness(_tempOutlineThickness);
            crosshairSettings.SetCenterDotEnabled(_tempCenterDotEnabled);
            crosshairSettings.SetCenterDotOpacity(_tempCenterDotOpacity);
            crosshairSettings.SetCenterDotThickness(_tempCenterDotThickness);

            crosshairSettings.SetInnerLinesEnabled(_tempInnerLinesEnabled);
            crosshairSettings.SetInnerLinesOpacity(_tempInnerLinesOpacity);
            crosshairSettings.SetInnerLinesShowVertical(_tempInnerLinesShowVertical);
            crosshairSettings.SetInnerLinesVerticalLength(_tempInnerLinesVerticalLength);
            crosshairSettings.SetInnerLinesVerticalThickness(_tempInnerLinesVerticalThickness);
            crosshairSettings.SetInnerLinesVerticalOffset(_tempInnerLinesVerticalOffset);
            crosshairSettings.SetInnerLinesShowHorizontal(_tempInnerLinesShowHorizontal);
            crosshairSettings.SetInnerLinesHorizontalLength(_tempInnerLinesHorizontalLength);
            crosshairSettings.SetInnerLinesHorizontalThickness(_tempInnerLinesHorizontalThickness);
            crosshairSettings.SetInnerLinesHorizontalOffset(_tempInnerLinesHorizontalOffset);
            crosshairSettings.SetInnerLinesMovementError(_tempInnerLinesMovementError);
            crosshairSettings.SetInnerLinesFiringError(_tempInnerLinesFiringError);

            crosshairSettings.SetOuterLinesEnabled(_tempOuterLinesEnabled);
            crosshairSettings.SetOuterLinesOpacity(_tempOuterLinesOpacity);
            crosshairSettings.SetOuterLinesShowVertical(_tempOuterLinesShowVertical);
            crosshairSettings.SetOuterLinesVerticalLength(_tempOuterLinesVerticalLength);
            crosshairSettings.SetOuterLinesVerticalThickness(_tempOuterLinesVerticalThickness);
            crosshairSettings.SetOuterLinesVerticalOffset(_tempOuterLinesVerticalOffset);
            crosshairSettings.SetOuterLinesShowHorizontal(_tempOuterLinesShowHorizontal);
            crosshairSettings.SetOuterLinesHorizontalLength(_tempOuterLinesHorizontalLength);
            crosshairSettings.SetOuterLinesHorizontalThickness(_tempOuterLinesHorizontalThickness);
            crosshairSettings.SetOuterLinesHorizontalOffset(_tempOuterLinesHorizontalOffset);
            crosshairSettings.SetOuterLinesMovementError(_tempOuterLinesMovementError);
            crosshairSettings.SetOuterLinesFiringError(_tempOuterLinesFiringError);

            crosshairSettings.SetFiringErrorEnabled(_tempFiringErrorEnabled);
            crosshairSettings.SetMovementErrorEnabled(_tempMovementErrorEnabled);

            _importExportCode = crosshairSettings.ExportToValorantCode();
        }

        private void ApplySettings()
        {
            ApplyPreview();
            ShowStatus("Settings applied!");
        }

        private void ImportCode()
        {
            if (string.IsNullOrEmpty(_importExportCode))
            {
                ShowStatus("Error: No code to import");
                return;
            }
            if (crosshairSettings.ImportFromValorantCode(_importExportCode))
            {
                LoadCurrentSettings();
                ShowStatus("Code imported successfully!");
            }
            else
            {
                ShowStatus("Error: Invalid crosshair code");
            }
        }

        private void ExportCode()
        {
            ApplyPreview();
            _importExportCode = crosshairSettings.ExportToValorantCode();
            ShowStatus("Code exported!");
        }

        private void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = _importExportCode;
            ShowStatus("Code copied to clipboard!");
        }

        private void ResetToDefault()
        {
            if (crosshairSettings != null)
            {
                crosshairSettings.ResetToDefault();
                LoadCurrentSettings();
                ShowStatus("Reset to default!");
            }
        }

        private void ShowStatus(string message)
        {
            _statusMessage = message;
            _statusMessageTimer = 3f;
        }

        private void OnBackClicked()
        {
            Hide();
            UIManager.Instance?.ShowSettingsFromCrosshair();
        }

        public void Show()
        {
            _isVisible = true;
            LoadCurrentSettings();
            crosshairRenderer?.Show();
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public bool IsVisible => _isVisible;

        private void OnDestroy()
        {
            if (_panelTexture != null) Destroy(_panelTexture);
            if (_buttonTexture != null) Destroy(_buttonTexture);
            if (_buttonHoverTexture != null) Destroy(_buttonHoverTexture);
            if (_textFieldTexture != null) Destroy(_textFieldTexture);
            if (_colorTextures != null)
            {
                foreach (var tex in _colorTextures)
                {
                    if (tex != null) Destroy(tex);
                }
            }
        }
    }
}
