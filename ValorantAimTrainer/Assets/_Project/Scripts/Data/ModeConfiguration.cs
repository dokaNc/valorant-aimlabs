using UnityEngine;
using ValorantAimTrainer.Core;

namespace ValorantAimTrainer.Data
{
    /// <summary>
    /// Configuration for different training modes.
    /// Defines behavior like timer type, target spawning, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "ModeConfig", menuName = "ValorantAimTrainer/Mode Configuration")]
    public class ModeConfiguration : ScriptableObject
    {
        [Header("Mode Identity")]
        [SerializeField] private TrainingMode mode;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("Timer Settings")]
        [Tooltip("Countdown = limited time, Countup = stopwatch until completion")]
        [SerializeField] private TimerType timerType = TimerType.Countdown;
        [SerializeField] private float defaultDuration = 60f;

        [Header("Target Spawning")]
        [Tooltip("Use fixed spawn points instead of random positions")]
        [SerializeField] private bool useFixedSpawnPoints = false;
        [SerializeField] private int targetCount = 10;
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private int maxActiveTargets = 1;

        [Header("Target Behavior")]
        [SerializeField] private bool targetsStrafe = true;
        [SerializeField] private bool targetsHaveLifetime = true;
        [SerializeField] private float targetLifetime = 3f;

        [Header("Player Movement")]
        [SerializeField] private bool allowPlayerMovement = true;

        [Header("Win Condition")]
        [Tooltip("End when all targets eliminated (Elimination mode)")]
        [SerializeField] private bool endOnAllTargetsKilled = false;

        // Properties
        public TrainingMode Mode => mode;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public TimerType TimerType => timerType;
        public float DefaultDuration => defaultDuration;
        public bool UseFixedSpawnPoints => useFixedSpawnPoints;
        public int TargetCount => targetCount;
        public float SpawnInterval => spawnInterval;
        public int MaxActiveTargets => maxActiveTargets;
        public bool TargetsStrafe => targetsStrafe;
        public bool TargetsHaveLifetime => targetsHaveLifetime;
        public float TargetLifetime => targetLifetime;
        public bool AllowPlayerMovement => allowPlayerMovement;
        public bool EndOnAllTargetsKilled => endOnAllTargetsKilled;

        /// <summary>
        /// Check if this is an elimination-style mode (count-up timer, kill all targets)
        /// </summary>
        public bool IsEliminationMode => timerType == TimerType.Countup && endOnAllTargetsKilled;
    }

    public enum TimerType
    {
        /// <summary>Time counts down from duration to 0</summary>
        Countdown,
        /// <summary>Time counts up from 0 (stopwatch style)</summary>
        Countup
    }
}
