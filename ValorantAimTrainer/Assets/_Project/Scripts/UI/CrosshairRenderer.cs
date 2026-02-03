using UnityEngine;
using ValorantAimTrainer.Data;

namespace ValorantAimTrainer.UI
{
    /// <summary>
    /// Renders the crosshair matching Valorant's exact visual style.
    /// Supports all Valorant crosshair options including opacity and outline.
    /// </summary>
    public class CrosshairRenderer : MonoBehaviour
    {
        [SerializeField] private CrosshairSettings settings;

        private Texture2D _pixelTexture;
        private bool _isVisible = true;

        // Dynamic error offset (for firing/movement error simulation)
        private float _currentErrorOffset = 0f;
        private float _targetErrorOffset = 0f;
        private const float ERROR_LERP_SPEED = 15f;

        public CrosshairSettings Settings
        {
            get => settings;
            set => settings = value;
        }

        private void Awake()
        {
            CreatePixelTexture();
        }

        private void CreatePixelTexture()
        {
            _pixelTexture = new Texture2D(1, 1);
            _pixelTexture.SetPixel(0, 0, Color.white);
            _pixelTexture.Apply();
        }

        private void Update()
        {
            // Smooth error offset transition
            _currentErrorOffset = Mathf.Lerp(_currentErrorOffset, _targetErrorOffset, Time.deltaTime * ERROR_LERP_SPEED);

            // Reset target error gradually
            _targetErrorOffset = Mathf.Lerp(_targetErrorOffset, 0f, Time.deltaTime * 8f);
        }

        private void OnGUI()
        {
            if (!_isVisible || settings == null) return;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // Draw outline first (if enabled)
            if (settings.OutlineEnabled)
            {
                DrawCrosshairOutline(center);
            }

            // Draw main crosshair
            DrawCrosshair(center);
        }

        private void DrawCrosshair(Vector2 center)
        {
            // Center Dot
            if (settings.CenterDotEnabled)
            {
                Color dotColor = settings.Color;
                dotColor.a = settings.CenterDotOpacity;
                GUI.color = dotColor;

                float dotSize = settings.CenterDotThickness;
                DrawRect(center.x - dotSize / 2f, center.y - dotSize / 2f, dotSize, dotSize);
            }

            // Inner Lines
            if (settings.InnerLinesEnabled)
            {
                Color innerColor = settings.Color;
                innerColor.a = settings.InnerLinesOpacity;
                GUI.color = innerColor;

                // Apply error offset if enabled
                float errorOffset = 0f;
                if (settings.InnerLinesFiringError && settings.FiringErrorEnabled)
                {
                    errorOffset += _currentErrorOffset * settings.InnerLinesFiringErrorMultiplier;
                }

                // Vertical lines (Top/Bottom)
                if (settings.InnerLinesShowVertical)
                {
                    float vLength = settings.InnerLinesVerticalLength;
                    float vThickness = settings.InnerLinesVerticalThickness;
                    float vOffset = settings.InnerLinesVerticalOffset + errorOffset;

                    // Top
                    DrawRect(center.x - vThickness / 2f, center.y - vOffset - vLength, vThickness, vLength);
                    // Bottom
                    DrawRect(center.x - vThickness / 2f, center.y + vOffset, vThickness, vLength);
                }

                // Horizontal lines (Left/Right)
                if (settings.InnerLinesShowHorizontal)
                {
                    float hLength = settings.InnerLinesHorizontalLength;
                    float hThickness = settings.InnerLinesHorizontalThickness;
                    float hOffset = settings.InnerLinesHorizontalOffset + errorOffset;

                    // Left
                    DrawRect(center.x - hOffset - hLength, center.y - hThickness / 2f, hLength, hThickness);
                    // Right
                    DrawRect(center.x + hOffset, center.y - hThickness / 2f, hLength, hThickness);
                }
            }

            // Outer Lines
            if (settings.OuterLinesEnabled)
            {
                Color outerColor = settings.Color;
                outerColor.a = settings.OuterLinesOpacity;
                GUI.color = outerColor;

                // Apply error offset if enabled
                float errorOffset = 0f;
                if (settings.OuterLinesFiringError && settings.FiringErrorEnabled)
                {
                    errorOffset += _currentErrorOffset * settings.OuterLinesFiringErrorMultiplier;
                }

                // Vertical lines (Top/Bottom)
                if (settings.OuterLinesShowVertical)
                {
                    float vLength = settings.OuterLinesVerticalLength;
                    float vThickness = settings.OuterLinesVerticalThickness;
                    float vOffset = settings.OuterLinesVerticalOffset + errorOffset;

                    // Top
                    DrawRect(center.x - vThickness / 2f, center.y - vOffset - vLength, vThickness, vLength);
                    // Bottom
                    DrawRect(center.x - vThickness / 2f, center.y + vOffset, vThickness, vLength);
                }

                // Horizontal lines (Left/Right)
                if (settings.OuterLinesShowHorizontal)
                {
                    float hLength = settings.OuterLinesHorizontalLength;
                    float hThickness = settings.OuterLinesHorizontalThickness;
                    float hOffset = settings.OuterLinesHorizontalOffset + errorOffset;

                    // Left
                    DrawRect(center.x - hOffset - hLength, center.y - hThickness / 2f, hLength, hThickness);
                    // Right
                    DrawRect(center.x + hOffset, center.y - hThickness / 2f, hLength, hThickness);
                }
            }

            GUI.color = Color.white;
        }

        private void DrawCrosshairOutline(Vector2 center)
        {
            Color outlineColor = settings.OutlineColor;
            GUI.color = outlineColor;

            float outlineThickness = settings.OutlineThickness;

            // Center Dot Outline
            if (settings.CenterDotEnabled)
            {
                float dotSize = settings.CenterDotThickness + outlineThickness * 2f;
                DrawRect(
                    center.x - dotSize / 2f,
                    center.y - dotSize / 2f,
                    dotSize,
                    dotSize
                );
            }

            // Inner Lines Outline
            if (settings.InnerLinesEnabled)
            {
                float errorOffset = 0f;
                if (settings.InnerLinesFiringError && settings.FiringErrorEnabled)
                {
                    errorOffset += _currentErrorOffset * settings.InnerLinesFiringErrorMultiplier;
                }

                // Vertical outline
                if (settings.InnerLinesShowVertical)
                {
                    float vLength = settings.InnerLinesVerticalLength + outlineThickness * 2f;
                    float vThickness = settings.InnerLinesVerticalThickness + outlineThickness * 2f;
                    float vOffset = settings.InnerLinesVerticalOffset - outlineThickness + errorOffset;

                    DrawRect(center.x - vThickness / 2f, center.y - vOffset - vLength + outlineThickness, vThickness, vLength);
                    DrawRect(center.x - vThickness / 2f, center.y + vOffset - outlineThickness, vThickness, vLength);
                }

                // Horizontal outline
                if (settings.InnerLinesShowHorizontal)
                {
                    float hLength = settings.InnerLinesHorizontalLength + outlineThickness * 2f;
                    float hThickness = settings.InnerLinesHorizontalThickness + outlineThickness * 2f;
                    float hOffset = settings.InnerLinesHorizontalOffset - outlineThickness + errorOffset;

                    DrawRect(center.x - hOffset - hLength + outlineThickness, center.y - hThickness / 2f, hLength, hThickness);
                    DrawRect(center.x + hOffset - outlineThickness, center.y - hThickness / 2f, hLength, hThickness);
                }
            }

            // Outer Lines Outline
            if (settings.OuterLinesEnabled)
            {
                float errorOffset = 0f;
                if (settings.OuterLinesFiringError && settings.FiringErrorEnabled)
                {
                    errorOffset += _currentErrorOffset * settings.OuterLinesFiringErrorMultiplier;
                }

                // Vertical outline
                if (settings.OuterLinesShowVertical)
                {
                    float vLength = settings.OuterLinesVerticalLength + outlineThickness * 2f;
                    float vThickness = settings.OuterLinesVerticalThickness + outlineThickness * 2f;
                    float vOffset = settings.OuterLinesVerticalOffset - outlineThickness + errorOffset;

                    DrawRect(center.x - vThickness / 2f, center.y - vOffset - vLength + outlineThickness, vThickness, vLength);
                    DrawRect(center.x - vThickness / 2f, center.y + vOffset - outlineThickness, vThickness, vLength);
                }

                // Horizontal outline
                if (settings.OuterLinesShowHorizontal)
                {
                    float hLength = settings.OuterLinesHorizontalLength + outlineThickness * 2f;
                    float hThickness = settings.OuterLinesHorizontalThickness + outlineThickness * 2f;
                    float hOffset = settings.OuterLinesHorizontalOffset - outlineThickness + errorOffset;

                    DrawRect(center.x - hOffset - hLength + outlineThickness, center.y - hThickness / 2f, hLength, hThickness);
                    DrawRect(center.x + hOffset - outlineThickness, center.y - hThickness / 2f, hLength, hThickness);
                }
            }
        }

        private void DrawRect(float x, float y, float width, float height)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), _pixelTexture);
        }

        /// <summary>
        /// Trigger firing error animation (crosshair expansion).
        /// Call this when the player fires.
        /// </summary>
        public void TriggerFiringError(float intensity = 1f)
        {
            if (settings == null || !settings.FiringErrorEnabled) return;

            _targetErrorOffset = Mathf.Max(_targetErrorOffset, 5f * intensity);
        }

        /// <summary>
        /// Set movement error (crosshair expansion while moving).
        /// </summary>
        public void SetMovementError(float amount)
        {
            if (settings == null || !settings.MovementErrorEnabled) return;

            // Movement error adds a constant offset while moving
            _targetErrorOffset = Mathf.Max(_targetErrorOffset, amount);
        }

        public void Show()
        {
            _isVisible = true;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
        }

        public bool IsVisible => _isVisible;

        private void OnDestroy()
        {
            if (_pixelTexture != null)
            {
                Destroy(_pixelTexture);
            }
        }
    }
}
