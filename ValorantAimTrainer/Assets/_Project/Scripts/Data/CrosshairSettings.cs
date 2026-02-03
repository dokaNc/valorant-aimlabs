using System;
using System.Text;
using System.Globalization;
using UnityEngine;

namespace ValorantAimTrainer.Data
{
    /// <summary>
    /// Crosshair settings that match Valorant's exact crosshair system.
    /// Supports import/export of Valorant crosshair codes.
    ///
    /// VALORANT CROSSHAIR CODE FORMAT:
    /// 0;P;c;5;h;0;0l;4;0o;2;0a;1;0f;0;1b;0
    /// - First number = Profile (0=Primary, 1=ADS, 2=Sniper)
    /// - P = Primary crosshair marker
    /// - c = Color index (0-7)
    /// - h = Center dot on/off
    /// - 0x = Inner line params (t=thickness, l=length, o=offset, a=opacity, s=show)
    /// - 1x = Outer line params
    /// - o = Outline on/off
    /// - d = Center dot thickness
    /// </summary>
    [CreateAssetMenu(fileName = "CrosshairSettings", menuName = "ValorantAimTrainer/Crosshair Settings")]
    public class CrosshairSettings : ScriptableObject
    {
        // ============================================
        // VALORANT COLOR PRESETS
        // ============================================

        public enum CrosshairColor
        {
            White = 0,
            Green = 1,
            YellowGreen = 2,
            GreenYellow = 3,
            Yellow = 4,
            Cyan = 5,
            Pink = 6,
            Red = 7,
            Custom = 8
        }

        public static readonly Color[] ColorPresets = new Color[]
        {
            new Color(1f, 1f, 1f, 1f),           // 0: White
            new Color(0f, 1f, 0f, 1f),           // 1: Green
            new Color(0.5f, 1f, 0f, 1f),         // 2: Yellow Green
            new Color(0.75f, 1f, 0f, 1f),        // 3: Green Yellow
            new Color(1f, 1f, 0f, 1f),           // 4: Yellow
            new Color(0f, 1f, 1f, 1f),           // 5: Cyan
            new Color(1f, 0f, 1f, 1f),           // 6: Pink
            new Color(1f, 0f, 0f, 1f),           // 7: Red
        };

        // ============================================
        // GENERAL SETTINGS
        // ============================================

        [Header("Color")]
        [SerializeField] private CrosshairColor colorPreset = CrosshairColor.Cyan;
        [SerializeField] private Color customColor = Color.cyan;

        [Header("Outlines")]
        [SerializeField] private bool outlineEnabled = true;
        [Range(0f, 1f)]
        [SerializeField] private float outlineOpacity = 0.5f;
        [Range(1f, 6f)]
        [SerializeField] private float outlineThickness = 1f;

        // ============================================
        // CENTER DOT
        // ============================================

        [Header("Center Dot")]
        [SerializeField] private bool centerDotEnabled = false;
        [Range(0f, 1f)]
        [SerializeField] private float centerDotOpacity = 1f;
        [Range(1f, 6f)]
        [SerializeField] private float centerDotThickness = 2f;

        // ============================================
        // INNER LINES
        // ============================================

        [Header("Inner Lines")]
        [SerializeField] private bool innerLinesEnabled = true;
        [Range(0f, 1f)]
        [SerializeField] private float innerLinesOpacity = 1f;

        [Header("Inner Lines - Vertical (Top/Bottom)")]
        [SerializeField] private bool innerLinesShowVertical = true;
        [Range(1f, 20f)]
        [SerializeField] private float innerLinesVerticalLength = 6f;
        [Range(1f, 10f)]
        [SerializeField] private float innerLinesVerticalThickness = 2f;
        [Range(0f, 20f)]
        [SerializeField] private float innerLinesVerticalOffset = 3f;

        [Header("Inner Lines - Horizontal (Left/Right)")]
        [SerializeField] private bool innerLinesShowHorizontal = true;
        [Range(1f, 20f)]
        [SerializeField] private float innerLinesHorizontalLength = 6f;
        [Range(1f, 10f)]
        [SerializeField] private float innerLinesHorizontalThickness = 2f;
        [Range(0f, 20f)]
        [SerializeField] private float innerLinesHorizontalOffset = 3f;

        [Header("Inner Lines - Error")]
        [SerializeField] private bool innerLinesMovementError = false;
        [Range(0f, 10f)]
        [SerializeField] private float innerLinesMovementErrorMultiplier = 1f;
        [SerializeField] private bool innerLinesFiringError = true;
        [Range(0f, 10f)]
        [SerializeField] private float innerLinesFiringErrorMultiplier = 1f;

        // ============================================
        // OUTER LINES
        // ============================================

        [Header("Outer Lines")]
        [SerializeField] private bool outerLinesEnabled = false;
        [Range(0f, 1f)]
        [SerializeField] private float outerLinesOpacity = 1f;

        [Header("Outer Lines - Vertical (Top/Bottom)")]
        [SerializeField] private bool outerLinesShowVertical = true;
        [Range(1f, 20f)]
        [SerializeField] private float outerLinesVerticalLength = 2f;
        [Range(1f, 10f)]
        [SerializeField] private float outerLinesVerticalThickness = 2f;
        [Range(0f, 40f)]
        [SerializeField] private float outerLinesVerticalOffset = 10f;

        [Header("Outer Lines - Horizontal (Left/Right)")]
        [SerializeField] private bool outerLinesShowHorizontal = true;
        [Range(1f, 20f)]
        [SerializeField] private float outerLinesHorizontalLength = 2f;
        [Range(1f, 10f)]
        [SerializeField] private float outerLinesHorizontalThickness = 2f;
        [Range(0f, 40f)]
        [SerializeField] private float outerLinesHorizontalOffset = 10f;

        [Header("Outer Lines - Error")]
        [SerializeField] private bool outerLinesMovementError = true;
        [Range(0f, 10f)]
        [SerializeField] private float outerLinesMovementErrorMultiplier = 1f;
        [SerializeField] private bool outerLinesFiringError = true;
        [Range(0f, 10f)]
        [SerializeField] private float outerLinesFiringErrorMultiplier = 1f;

        // ============================================
        // GLOBAL ERROR SETTINGS
        // ============================================

        [Header("Error Settings")]
        [SerializeField] private bool firingErrorEnabled = true;
        [SerializeField] private bool movementErrorEnabled = true;

        // ============================================
        // PROPERTIES - Color
        // ============================================

        public CrosshairColor ColorPreset => colorPreset;
        public Color CustomColor => customColor;

        public Color Color
        {
            get
            {
                if (colorPreset == CrosshairColor.Custom)
                    return customColor;
                return ColorPresets[(int)colorPreset];
            }
        }

        // ============================================
        // PROPERTIES - Outline
        // ============================================

        public bool OutlineEnabled => outlineEnabled;
        public float OutlineOpacity => outlineOpacity;
        public float OutlineThickness => outlineThickness;
        public Color OutlineColor => new Color(0f, 0f, 0f, outlineOpacity);

        // ============================================
        // PROPERTIES - Center Dot
        // ============================================

        public bool CenterDotEnabled => centerDotEnabled;
        public float CenterDotOpacity => centerDotOpacity;
        public float CenterDotThickness => centerDotThickness;

        // Legacy properties for compatibility
        public bool ShowCenterDot => centerDotEnabled;
        public float CenterDotSize => centerDotThickness;

        // ============================================
        // PROPERTIES - Inner Lines
        // ============================================

        public bool InnerLinesEnabled => innerLinesEnabled;
        public float InnerLinesOpacity => innerLinesOpacity;

        // Vertical (Top/Bottom)
        public bool InnerLinesShowVertical => innerLinesShowVertical;
        public float InnerLinesVerticalLength => innerLinesVerticalLength;
        public float InnerLinesVerticalThickness => innerLinesVerticalThickness;
        public float InnerLinesVerticalOffset => innerLinesVerticalOffset;

        // Horizontal (Left/Right)
        public bool InnerLinesShowHorizontal => innerLinesShowHorizontal;
        public float InnerLinesHorizontalLength => innerLinesHorizontalLength;
        public float InnerLinesHorizontalThickness => innerLinesHorizontalThickness;
        public float InnerLinesHorizontalOffset => innerLinesHorizontalOffset;

        // Error
        public bool InnerLinesMovementError => innerLinesMovementError;
        public float InnerLinesMovementErrorMultiplier => innerLinesMovementErrorMultiplier;
        public bool InnerLinesFiringError => innerLinesFiringError;
        public float InnerLinesFiringErrorMultiplier => innerLinesFiringErrorMultiplier;

        // Legacy properties for compatibility (use vertical values as default)
        public bool ShowInnerLines => innerLinesEnabled;
        public float InnerLinesLength => innerLinesVerticalLength;
        public float InnerLinesThickness => innerLinesVerticalThickness;
        public float InnerLinesOffset => innerLinesVerticalOffset;
        public float InnerLineLength => innerLinesVerticalLength;
        public float InnerLineThickness => innerLinesVerticalThickness;
        public float InnerLineOffset => innerLinesVerticalOffset;

        // ============================================
        // PROPERTIES - Outer Lines
        // ============================================

        public bool OuterLinesEnabled => outerLinesEnabled;
        public float OuterLinesOpacity => outerLinesOpacity;

        // Vertical (Top/Bottom)
        public bool OuterLinesShowVertical => outerLinesShowVertical;
        public float OuterLinesVerticalLength => outerLinesVerticalLength;
        public float OuterLinesVerticalThickness => outerLinesVerticalThickness;
        public float OuterLinesVerticalOffset => outerLinesVerticalOffset;

        // Horizontal (Left/Right)
        public bool OuterLinesShowHorizontal => outerLinesShowHorizontal;
        public float OuterLinesHorizontalLength => outerLinesHorizontalLength;
        public float OuterLinesHorizontalThickness => outerLinesHorizontalThickness;
        public float OuterLinesHorizontalOffset => outerLinesHorizontalOffset;

        // Error
        public bool OuterLinesMovementError => outerLinesMovementError;
        public float OuterLinesMovementErrorMultiplier => outerLinesMovementErrorMultiplier;
        public bool OuterLinesFiringError => outerLinesFiringError;
        public float OuterLinesFiringErrorMultiplier => outerLinesFiringErrorMultiplier;

        // Legacy properties for compatibility (use vertical values as default)
        public bool ShowOuterLines => outerLinesEnabled;
        public float OuterLinesLength => outerLinesVerticalLength;
        public float OuterLinesThickness => outerLinesVerticalThickness;
        public float OuterLinesOffset => outerLinesVerticalOffset;
        public float OuterLineLength => outerLinesVerticalLength;
        public float OuterLineThickness => outerLinesVerticalThickness;
        public float OuterLineOffset => outerLinesVerticalOffset;
        public bool ShowOutline => outlineEnabled;

        // ============================================
        // PROPERTIES - Global Error
        // ============================================

        public bool FiringErrorEnabled => firingErrorEnabled;
        public bool MovementErrorEnabled => movementErrorEnabled;

        // ============================================
        // SETTERS
        // ============================================

        public void SetColorPreset(CrosshairColor preset)
        {
            colorPreset = preset;
        }

        public void SetCustomColor(Color color)
        {
            customColor = color;
            colorPreset = CrosshairColor.Custom;
        }

        public void SetOutlineEnabled(bool enabled) => outlineEnabled = enabled;
        public void SetOutlineOpacity(float opacity) => outlineOpacity = Mathf.Clamp01(opacity);
        public void SetOutlineThickness(float thickness) => outlineThickness = Mathf.Clamp(thickness, 1f, 6f);

        public void SetCenterDotEnabled(bool enabled) => centerDotEnabled = enabled;
        public void SetCenterDotOpacity(float opacity) => centerDotOpacity = Mathf.Clamp01(opacity);
        public void SetCenterDotThickness(float thickness) => centerDotThickness = Mathf.Clamp(thickness, 1f, 6f);

        public void SetInnerLinesEnabled(bool enabled) => innerLinesEnabled = enabled;
        public void SetInnerLinesOpacity(float opacity) => innerLinesOpacity = Mathf.Clamp01(opacity);

        // Inner Lines - Vertical
        public void SetInnerLinesShowVertical(bool show) => innerLinesShowVertical = show;
        public void SetInnerLinesVerticalLength(float length) => innerLinesVerticalLength = Mathf.Clamp(length, 1f, 20f);
        public void SetInnerLinesVerticalThickness(float thickness) => innerLinesVerticalThickness = Mathf.Clamp(thickness, 1f, 10f);
        public void SetInnerLinesVerticalOffset(float offset) => innerLinesVerticalOffset = Mathf.Clamp(offset, 0f, 20f);

        // Inner Lines - Horizontal
        public void SetInnerLinesShowHorizontal(bool show) => innerLinesShowHorizontal = show;
        public void SetInnerLinesHorizontalLength(float length) => innerLinesHorizontalLength = Mathf.Clamp(length, 1f, 20f);
        public void SetInnerLinesHorizontalThickness(float thickness) => innerLinesHorizontalThickness = Mathf.Clamp(thickness, 1f, 10f);
        public void SetInnerLinesHorizontalOffset(float offset) => innerLinesHorizontalOffset = Mathf.Clamp(offset, 0f, 20f);

        // Inner Lines - Error
        public void SetInnerLinesMovementError(bool enabled) => innerLinesMovementError = enabled;
        public void SetInnerLinesMovementErrorMultiplier(float mult) => innerLinesMovementErrorMultiplier = Mathf.Clamp(mult, 0f, 10f);
        public void SetInnerLinesFiringError(bool enabled) => innerLinesFiringError = enabled;
        public void SetInnerLinesFiringErrorMultiplier(float mult) => innerLinesFiringErrorMultiplier = Mathf.Clamp(mult, 0f, 10f);

        public void SetOuterLinesEnabled(bool enabled) => outerLinesEnabled = enabled;
        public void SetOuterLinesOpacity(float opacity) => outerLinesOpacity = Mathf.Clamp01(opacity);

        // Outer Lines - Vertical
        public void SetOuterLinesShowVertical(bool show) => outerLinesShowVertical = show;
        public void SetOuterLinesVerticalLength(float length) => outerLinesVerticalLength = Mathf.Clamp(length, 1f, 20f);
        public void SetOuterLinesVerticalThickness(float thickness) => outerLinesVerticalThickness = Mathf.Clamp(thickness, 1f, 10f);
        public void SetOuterLinesVerticalOffset(float offset) => outerLinesVerticalOffset = Mathf.Clamp(offset, 0f, 40f);

        // Outer Lines - Horizontal
        public void SetOuterLinesShowHorizontal(bool show) => outerLinesShowHorizontal = show;
        public void SetOuterLinesHorizontalLength(float length) => outerLinesHorizontalLength = Mathf.Clamp(length, 1f, 20f);
        public void SetOuterLinesHorizontalThickness(float thickness) => outerLinesHorizontalThickness = Mathf.Clamp(thickness, 1f, 10f);
        public void SetOuterLinesHorizontalOffset(float offset) => outerLinesHorizontalOffset = Mathf.Clamp(offset, 0f, 40f);

        // Outer Lines - Error
        public void SetOuterLinesMovementError(bool enabled) => outerLinesMovementError = enabled;
        public void SetOuterLinesMovementErrorMultiplier(float mult) => outerLinesMovementErrorMultiplier = Mathf.Clamp(mult, 0f, 10f);
        public void SetOuterLinesFiringError(bool enabled) => outerLinesFiringError = enabled;
        public void SetOuterLinesFiringErrorMultiplier(float mult) => outerLinesFiringErrorMultiplier = Mathf.Clamp(mult, 0f, 10f);

        // Legacy setters (set both vertical and horizontal)
        public void SetInnerLinesLength(float length)
        {
            SetInnerLinesVerticalLength(length);
            SetInnerLinesHorizontalLength(length);
        }
        public void SetInnerLinesThickness(float thickness)
        {
            SetInnerLinesVerticalThickness(thickness);
            SetInnerLinesHorizontalThickness(thickness);
        }
        public void SetInnerLinesOffset(float offset)
        {
            SetInnerLinesVerticalOffset(offset);
            SetInnerLinesHorizontalOffset(offset);
        }
        public void SetOuterLinesLength(float length)
        {
            SetOuterLinesVerticalLength(length);
            SetOuterLinesHorizontalLength(length);
        }
        public void SetOuterLinesThickness(float thickness)
        {
            SetOuterLinesVerticalThickness(thickness);
            SetOuterLinesHorizontalThickness(thickness);
        }
        public void SetOuterLinesOffset(float offset)
        {
            SetOuterLinesVerticalOffset(offset);
            SetOuterLinesHorizontalOffset(offset);
        }

        public void SetFiringErrorEnabled(bool enabled) => firingErrorEnabled = enabled;
        public void SetMovementErrorEnabled(bool enabled) => movementErrorEnabled = enabled;

        // Legacy setters for compatibility
        public void SetColor(Color newColor) => SetCustomColor(newColor);
        public void SetOutlineColor(Color newColor) { /* outline is always black in Valorant */ }
        public void SetShowOutline(bool show) => SetOutlineEnabled(show);
        public void SetShowCenterDot(bool show) => SetCenterDotEnabled(show);
        public void SetCenterDotSize(float size) => SetCenterDotThickness(size);
        public void SetShowInnerLines(bool show) => SetInnerLinesEnabled(show);
        public void SetInnerLineLength(float length) => SetInnerLinesLength(length);
        public void SetInnerLineThickness(float thickness) => SetInnerLinesThickness(thickness);
        public void SetInnerLineOffset(float offset) => SetInnerLinesOffset(offset);
        public void SetShowOuterLines(bool show) => SetOuterLinesEnabled(show);
        public void SetOuterLineLength(float length) => SetOuterLinesLength(length);
        public void SetOuterLineThickness(float thickness) => SetOuterLinesThickness(thickness);
        public void SetOuterLineOffset(float offset) => SetOuterLinesOffset(offset);

        // ============================================
        // VALORANT CODE IMPORT/EXPORT
        // ============================================

        /// <summary>
        /// Export settings to Valorant crosshair code format.
        /// Format: 0;P;c;X;h;X;...
        /// </summary>
        public string ExportToValorantCode()
        {
            var sb = new StringBuilder();

            // Profile identifier (0 = Primary)
            sb.Append("0;P;");

            // Color (c)
            if (colorPreset != CrosshairColor.Custom)
            {
                sb.Append($"c;{(int)colorPreset};");
            }

            // Outline (o) - 1=on, 0=off
            sb.Append($"o;{(outlineEnabled ? 1 : 0)};");

            // Outline opacity (p) - Valorant uses 0-1 scale
            if (outlineEnabled && Math.Abs(outlineOpacity - 0.5f) > 0.01f)
            {
                sb.Append($"p;{outlineOpacity.ToString("F1", CultureInfo.InvariantCulture)};");
            }

            // Outline thickness (t) - only if not default (1)
            if (outlineEnabled && Math.Abs(outlineThickness - 1f) > 0.01f)
            {
                sb.Append($"t;{outlineThickness:F0};");
            }

            // Center dot (h) - 1=on, 0=off (h = has dot)
            sb.Append($"h;{(centerDotEnabled ? 1 : 0)};");

            // Center dot thickness (d) - only if dot is enabled
            if (centerDotEnabled && Math.Abs(centerDotThickness - 2f) > 0.01f)
            {
                sb.Append($"d;{centerDotThickness:F0};");
            }

            // Center dot opacity (z) - only if not default (1)
            if (centerDotEnabled && Math.Abs(centerDotOpacity - 1f) > 0.01f)
            {
                sb.Append($"z;{centerDotOpacity.ToString("F1", CultureInfo.InvariantCulture)};");
            }

            // Firing error (f) - 1=on, 0=off
            sb.Append($"f;{(firingErrorEnabled ? 1 : 0)};");

            // Movement error (m) - 1=on, 0=off
            sb.Append($"m;{(movementErrorEnabled ? 1 : 0)};");

            // Inner lines (0 prefix) - use vertical values for export
            // Show inner lines (0s) - NOTE: 0=show, 1=hide (inverted!)
            sb.Append($"0s;{(innerLinesEnabled ? 0 : 1)};");
            sb.Append($"0l;{innerLinesVerticalLength:F0};");
            sb.Append($"0t;{innerLinesVerticalThickness:F0};");
            sb.Append($"0o;{innerLinesVerticalOffset:F0};");
            sb.Append($"0a;{innerLinesOpacity.ToString("F1", CultureInfo.InvariantCulture)};");
            sb.Append($"0m;{(innerLinesMovementError ? 1 : 0)};");
            sb.Append($"0f;{(innerLinesFiringError ? 1 : 0)};");

            // Outer lines (1 prefix) - use vertical values for export
            // Show outer lines (1b) - NOTE: 0=show, 1=hide (inverted!)
            sb.Append($"1b;{(outerLinesEnabled ? 0 : 1)};");
            sb.Append($"1l;{outerLinesVerticalLength:F0};");
            sb.Append($"1t;{outerLinesVerticalThickness:F0};");
            sb.Append($"1o;{outerLinesVerticalOffset:F0};");
            sb.Append($"1a;{outerLinesOpacity.ToString("F1", CultureInfo.InvariantCulture)};");
            sb.Append($"1m;{(outerLinesMovementError ? 1 : 0)};");
            sb.Append($"1f;{(outerLinesFiringError ? 1 : 0)}");

            return sb.ToString();
        }

        /// <summary>
        /// Import settings from Valorant crosshair code.
        /// Returns true if import was successful.
        /// </summary>
        public bool ImportFromValorantCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            try
            {
                // Remove whitespace and split by semicolon
                code = code.Trim();
                string[] parts = code.Split(';');

                // Parse key-value pairs
                for (int i = 0; i < parts.Length - 1; i += 2)
                {
                    string key = parts[i].ToLower();
                    string value = parts[i + 1];

                    // Skip profile identifiers
                    if (key == "0" || key == "1" || key == "2" || key == "p" || key == "s" || key == "a")
                        continue;

                    ParseParameter(key, value);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CrosshairSettings] Failed to import code: {e.Message}");
                return false;
            }
        }

        private void ParseParameter(string key, string value)
        {
            // Try parse as float
            float floatValue = 0f;
            int intValue = 0;
            bool hasFloat = float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out floatValue);
            bool hasInt = int.TryParse(value, out intValue);

            switch (key)
            {
                // Color
                case "c":
                    if (hasInt && intValue >= 0 && intValue <= 7)
                        colorPreset = (CrosshairColor)intValue;
                    break;

                // Outline
                case "o":
                    outlineEnabled = intValue == 1;
                    break;
                case "p":
                    if (hasFloat) outlineOpacity = Mathf.Clamp01(floatValue);
                    break;
                case "t":
                    if (hasFloat) outlineThickness = Mathf.Clamp(floatValue, 1f, 6f);
                    break;

                // Center dot
                case "h":
                    centerDotEnabled = intValue == 1;
                    break;
                case "d":
                    if (hasFloat) centerDotThickness = Mathf.Clamp(floatValue, 1f, 6f);
                    break;
                case "z":
                    if (hasFloat) centerDotOpacity = Mathf.Clamp01(floatValue);
                    break;

                // Global error
                case "f":
                    firingErrorEnabled = intValue == 1;
                    break;
                case "m":
                    movementErrorEnabled = intValue == 1;
                    break;

                // Inner lines (0 prefix) - apply to both vertical and horizontal
                case "0s":
                    innerLinesEnabled = intValue == 0; // Inverted!
                    break;
                case "0l":
                    if (hasFloat)
                    {
                        float val = Mathf.Clamp(floatValue, 1f, 20f);
                        innerLinesVerticalLength = val;
                        innerLinesHorizontalLength = val;
                    }
                    break;
                case "0t":
                    if (hasFloat)
                    {
                        float val = Mathf.Clamp(floatValue, 1f, 10f);
                        innerLinesVerticalThickness = val;
                        innerLinesHorizontalThickness = val;
                    }
                    break;
                case "0o":
                    if (hasFloat)
                    {
                        float val = Mathf.Clamp(floatValue, 0f, 20f);
                        innerLinesVerticalOffset = val;
                        innerLinesHorizontalOffset = val;
                    }
                    break;
                case "0a":
                    if (hasFloat) innerLinesOpacity = Mathf.Clamp01(floatValue);
                    break;
                case "0m":
                    innerLinesMovementError = intValue == 1;
                    break;
                case "0f":
                    innerLinesFiringError = intValue == 1;
                    break;

                // Outer lines (1 prefix) - apply to both vertical and horizontal
                case "1b":
                    outerLinesEnabled = intValue == 0; // Inverted!
                    break;
                case "1l":
                    if (hasFloat)
                    {
                        float val = Mathf.Clamp(floatValue, 1f, 20f);
                        outerLinesVerticalLength = val;
                        outerLinesHorizontalLength = val;
                    }
                    break;
                case "1t":
                    if (hasFloat)
                    {
                        float val = Mathf.Clamp(floatValue, 1f, 10f);
                        outerLinesVerticalThickness = val;
                        outerLinesHorizontalThickness = val;
                    }
                    break;
                case "1o":
                    if (hasFloat)
                    {
                        float val = Mathf.Clamp(floatValue, 0f, 40f);
                        outerLinesVerticalOffset = val;
                        outerLinesHorizontalOffset = val;
                    }
                    break;
                case "1a":
                    if (hasFloat) outerLinesOpacity = Mathf.Clamp01(floatValue);
                    break;
                case "1m":
                    outerLinesMovementError = intValue == 1;
                    break;
                case "1f":
                    outerLinesFiringError = intValue == 1;
                    break;
            }
        }

        /// <summary>
        /// Copy all settings from another CrosshairSettings.
        /// </summary>
        public void CopyFrom(CrosshairSettings other)
        {
            if (other == null) return;

            colorPreset = other.colorPreset;
            customColor = other.customColor;

            outlineEnabled = other.outlineEnabled;
            outlineOpacity = other.outlineOpacity;
            outlineThickness = other.outlineThickness;

            centerDotEnabled = other.centerDotEnabled;
            centerDotOpacity = other.centerDotOpacity;
            centerDotThickness = other.centerDotThickness;

            innerLinesEnabled = other.innerLinesEnabled;
            innerLinesOpacity = other.innerLinesOpacity;
            innerLinesShowVertical = other.innerLinesShowVertical;
            innerLinesVerticalLength = other.innerLinesVerticalLength;
            innerLinesVerticalThickness = other.innerLinesVerticalThickness;
            innerLinesVerticalOffset = other.innerLinesVerticalOffset;
            innerLinesShowHorizontal = other.innerLinesShowHorizontal;
            innerLinesHorizontalLength = other.innerLinesHorizontalLength;
            innerLinesHorizontalThickness = other.innerLinesHorizontalThickness;
            innerLinesHorizontalOffset = other.innerLinesHorizontalOffset;
            innerLinesMovementError = other.innerLinesMovementError;
            innerLinesMovementErrorMultiplier = other.innerLinesMovementErrorMultiplier;
            innerLinesFiringError = other.innerLinesFiringError;
            innerLinesFiringErrorMultiplier = other.innerLinesFiringErrorMultiplier;

            outerLinesEnabled = other.outerLinesEnabled;
            outerLinesOpacity = other.outerLinesOpacity;
            outerLinesShowVertical = other.outerLinesShowVertical;
            outerLinesVerticalLength = other.outerLinesVerticalLength;
            outerLinesVerticalThickness = other.outerLinesVerticalThickness;
            outerLinesVerticalOffset = other.outerLinesVerticalOffset;
            outerLinesShowHorizontal = other.outerLinesShowHorizontal;
            outerLinesHorizontalLength = other.outerLinesHorizontalLength;
            outerLinesHorizontalThickness = other.outerLinesHorizontalThickness;
            outerLinesHorizontalOffset = other.outerLinesHorizontalOffset;
            outerLinesMovementError = other.outerLinesMovementError;
            outerLinesMovementErrorMultiplier = other.outerLinesMovementErrorMultiplier;
            outerLinesFiringError = other.outerLinesFiringError;
            outerLinesFiringErrorMultiplier = other.outerLinesFiringErrorMultiplier;

            firingErrorEnabled = other.firingErrorEnabled;
            movementErrorEnabled = other.movementErrorEnabled;
        }

        /// <summary>
        /// Reset to default Valorant crosshair settings.
        /// </summary>
        public void ResetToDefault()
        {
            colorPreset = CrosshairColor.Cyan;
            customColor = Color.cyan;

            outlineEnabled = true;
            outlineOpacity = 0.5f;
            outlineThickness = 1f;

            centerDotEnabled = false;
            centerDotOpacity = 1f;
            centerDotThickness = 2f;

            innerLinesEnabled = true;
            innerLinesOpacity = 1f;
            innerLinesShowVertical = true;
            innerLinesVerticalLength = 6f;
            innerLinesVerticalThickness = 2f;
            innerLinesVerticalOffset = 3f;
            innerLinesShowHorizontal = true;
            innerLinesHorizontalLength = 6f;
            innerLinesHorizontalThickness = 2f;
            innerLinesHorizontalOffset = 3f;
            innerLinesMovementError = false;
            innerLinesMovementErrorMultiplier = 1f;
            innerLinesFiringError = true;
            innerLinesFiringErrorMultiplier = 1f;

            outerLinesEnabled = false;
            outerLinesOpacity = 1f;
            outerLinesShowVertical = true;
            outerLinesVerticalLength = 2f;
            outerLinesVerticalThickness = 2f;
            outerLinesVerticalOffset = 10f;
            outerLinesShowHorizontal = true;
            outerLinesHorizontalLength = 2f;
            outerLinesHorizontalThickness = 2f;
            outerLinesHorizontalOffset = 10f;
            outerLinesMovementError = true;
            outerLinesMovementErrorMultiplier = 1f;
            outerLinesFiringError = true;
            outerLinesFiringErrorMultiplier = 1f;

            firingErrorEnabled = true;
            movementErrorEnabled = true;
        }
    }
}
