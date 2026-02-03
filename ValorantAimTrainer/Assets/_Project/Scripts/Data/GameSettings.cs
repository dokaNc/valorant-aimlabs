using UnityEngine;

namespace ValorantAimTrainer.Data
{
    /// <summary>
    /// Game settings calibrated to match Valorant's exact feel.
    ///
    /// VALORANT SPECIFICATIONS (verified sources: mouse-sensitivity.com, kovaak.com):
    /// - Horizontal FOV: 103° (fixed)
    /// - Vertical FOV: ~70.53° at 16:9
    /// - Yaw: 0.07 degrees per mouse count at sensitivity 1.0
    /// - Raw Input: Enabled by default
    ///
    /// SENSITIVITY FORMULA:
    /// Valorant: rotation = mouse_counts × sensitivity × 0.07
    /// Unity:    rotation = mouse_delta × sensitivity × 0.07 × UNITY_SCALE_FACTOR
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "ValorantAimTrainer/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        // ============================================
        // VALORANT CONSTANTS (VERIFIED)
        // ============================================

        /// <summary>
        /// Valorant's yaw: 0.07 degrees per mouse count at sensitivity 1.0
        /// Comparison: CS2 = 0.022, Overwatch = 0.0066
        /// </summary>
        public const float VALORANT_YAW = 0.07f;

        /// <summary>Valorant's horizontal FOV (fixed at 103°)</summary>
        public const float VALORANT_HORIZONTAL_FOV = 103f;

        /// <summary>Valorant's vertical FOV at 16:9</summary>
        public const float VALORANT_VERTICAL_FOV_16_9 = 70.53f;

        /// <summary>
        /// Calibration factor for Unity's mouse delta.
        /// Adjust if sensitivity doesn't match Valorant:
        /// - Too slow? Increase
        /// - Too fast? Decrease
        /// </summary>
        public const float UNITY_SCALE_FACTOR = 1.029f;

        // ============================================
        // VALORANT MOVEMENT SPEEDS (VERIFIED)
        // ============================================
        // Source: In-game testing, community wikis
        // All speeds in Unity units/second (1 unit ≈ 1 meter)

        /// <summary>Running speed (default movement) - 5.4 m/s in VALORANT</summary>
        public const float RUN_SPEED = 5.4f;

        /// <summary>Walking speed (Shift held) - 2.7 m/s in VALORANT (50% of run)</summary>
        public const float WALK_SPEED = 2.7f;

        /// <summary>Crouching speed - 2.16 m/s in VALORANT (40% of run)</summary>
        public const float CROUCH_SPEED = 2.16f;

        /// <summary>Jump velocity - tuned for VALORANT feel</summary>
        public const float JUMP_VELOCITY = 5.5f;

        /// <summary>Gravity multiplier for realistic fall</summary>
        public const float GRAVITY = -15f;

        /// <summary>Player height when standing</summary>
        public const float STAND_HEIGHT = 1.8f;

        /// <summary>Player height when crouching</summary>
        public const float CROUCH_HEIGHT = 1.2f;

        /// <summary>Camera Y offset from player pivot when standing</summary>
        public const float CAMERA_HEIGHT_STAND = 1.6f;

        /// <summary>Camera Y offset from player pivot when crouching</summary>
        public const float CAMERA_HEIGHT_CROUCH = 1.0f;

        // ============================================
        // USER SETTINGS
        // ============================================

        [Header("Sensitivity")]
        [Tooltip("Your Valorant in-game sensitivity")]
        [Range(0.1f, 3f)]
        [SerializeField] private float valorantSensitivity = 0.5f;

        [Tooltip("Invert Y axis")]
        [SerializeField] private bool invertY = false;

        [Header("Session")]
        [SerializeField] private int sessionDuration = 60;

        // Properties
        public float ValorantSensitivity => valorantSensitivity;
        public bool InvertY => invertY;
        public int SessionDuration => sessionDuration;

        /// <summary>
        /// Returns the sensitivity multiplier for Unity's mouse delta.
        /// </summary>
        public float CalculateUnitySensitivity()
        {
            return valorantSensitivity * VALORANT_YAW * UNITY_SCALE_FACTOR;
        }

        /// <summary>
        /// Calculate vertical FOV for Unity based on aspect ratio.
        /// Ensures horizontal FOV matches Valorant's 103°.
        /// </summary>
        public static float CalculateVerticalFOV(float aspectRatio)
        {
            float hFovRad = VALORANT_HORIZONTAL_FOV * Mathf.Deg2Rad;
            float vFovRad = 2f * Mathf.Atan(Mathf.Tan(hFovRad / 2f) / aspectRatio);
            return vFovRad * Mathf.Rad2Deg;
        }

        // Setters
        public void SetValorantSensitivity(float sens)
        {
            valorantSensitivity = Mathf.Clamp(sens, 0.01f, 10f);
        }

        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }

        public void SetSessionDuration(int duration)
        {
            sessionDuration = Mathf.Clamp(duration, 30, 300);
        }
    }
}
