using UnityEngine;
using ValorantAimTrainer.Data;

namespace ValorantAimTrainer.Core
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        [Header("Session Settings")]
        [SerializeField] private float defaultSessionDuration = 60f;
        [SerializeField] private int countdownSeconds = 3;

        [Header("Elimination Mode Settings")]
        [SerializeField] private int eliminationTargetCount = 20;

        private SessionStats _currentStats;
        private float _sessionTimer;
        private float _countdownTimer;
        private bool _isSessionActive;
        private bool _isCountingDown;
        private bool _isEliminationMode;
        private ModeConfiguration _currentModeConfig;

        public SessionStats CurrentStats => _currentStats;
        public float RemainingTime => _sessionTimer;
        public float ElapsedTime => _currentStats?.SessionDuration ?? 0f;
        public float SessionDuration => defaultSessionDuration;
        public bool IsSessionActive => _isSessionActive;
        public bool IsEliminationMode => _isEliminationMode;
        public int EliminationTargetCount => eliminationTargetCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _currentStats = new SessionStats();
        }

        private void OnEnable()
        {
            EventBus.OnTargetHit += HandleTargetHit;
            EventBus.OnTargetMissed += HandleTargetMissed;
            EventBus.OnTargetSpawned += HandleTargetSpawned;
            EventBus.OnShoot += HandleShoot;
            EventBus.OnHit += HandleHit;
            EventBus.OnMiss += HandleMiss;
        }

        private void OnDisable()
        {
            EventBus.OnTargetHit -= HandleTargetHit;
            EventBus.OnTargetMissed -= HandleTargetMissed;
            EventBus.OnTargetSpawned -= HandleTargetSpawned;
            EventBus.OnShoot -= HandleShoot;
            EventBus.OnHit -= HandleHit;
            EventBus.OnMiss -= HandleMiss;
        }

        private void Update()
        {
            if (_isCountingDown)
            {
                UpdateCountdown();
            }
            else if (_isSessionActive)
            {
                UpdateSession();
            }
        }

        private void UpdateCountdown()
        {
            _countdownTimer -= Time.deltaTime;

            int currentSecond = Mathf.CeilToInt(_countdownTimer);
            int previousSecond = Mathf.CeilToInt(_countdownTimer + Time.deltaTime);

            if (currentSecond != previousSecond && currentSecond > 0)
            {
                EventBus.TriggerCountdownTick(currentSecond);
            }

            if (_countdownTimer <= 0f)
            {
                _isCountingDown = false;
                StartSessionInternal();
                EventBus.TriggerCountdownComplete();
            }
        }

        private void UpdateSession()
        {
            // Always track session duration
            _currentStats.SessionDuration += Time.deltaTime;

            if (_isEliminationMode)
            {
                // Elimination mode: count UP (stopwatch)
                _sessionTimer += Time.deltaTime;
                // Session ends when all targets are eliminated (handled in HandleTargetHit)
            }
            else
            {
                // Standard mode: count DOWN
                _sessionTimer -= Time.deltaTime;

                if (_sessionTimer <= 0f)
                {
                    EndSession();
                }
            }
        }

        /// <summary>
        /// Start a standard countdown session with time limit
        /// </summary>
        public void StartCountdown()
        {
            _isEliminationMode = false;
            _currentModeConfig = null;
            _currentStats.Reset();
            _currentStats.IsEliminationMode = false;
            _countdownTimer = countdownSeconds;
            _isCountingDown = true;

            GameManager.Instance.SetState(GameState.Countdown);
            EventBus.TriggerCountdownTick(countdownSeconds);
        }

        /// <summary>
        /// Start an elimination mode session (count-up timer, kill all targets)
        /// </summary>
        public void StartEliminationCountdown(int targetCount = -1)
        {
            _isEliminationMode = true;
            _currentStats.Reset();
            _currentStats.IsEliminationMode = true;
            _currentStats.TotalTargetsToEliminate = targetCount > 0 ? targetCount : eliminationTargetCount;
            _countdownTimer = countdownSeconds;
            _isCountingDown = true;

            GameManager.Instance.SetState(GameState.Countdown);
            EventBus.TriggerCountdownTick(countdownSeconds);
        }

        /// <summary>
        /// Start session with a mode configuration
        /// </summary>
        public void StartWithConfiguration(ModeConfiguration config)
        {
            _currentModeConfig = config;

            if (config.IsEliminationMode)
            {
                StartEliminationCountdown(config.TargetCount);
            }
            else
            {
                defaultSessionDuration = config.DefaultDuration;
                StartCountdown();
            }
        }

        private void StartSessionInternal()
        {
            if (_isEliminationMode)
            {
                // Elimination: start at 0, count up
                _sessionTimer = 0f;
            }
            else
            {
                // Standard: start at duration, count down
                _sessionTimer = defaultSessionDuration;
            }

            _isSessionActive = true;

            GameManager.Instance.SetState(GameState.Playing);
            EventBus.TriggerSessionStart();
        }

        public void EndSession()
        {
            if (!_isSessionActive) return;

            _isSessionActive = false;
            _isCountingDown = false;

            // Record completion time for elimination mode
            if (_isEliminationMode)
            {
                _currentStats.CompletionTime = _sessionTimer;
            }

            GameManager.Instance.EndGame();
        }

        public void SetSessionDuration(float duration)
        {
            defaultSessionDuration = duration;
        }

        public void SetEliminationTargetCount(int count)
        {
            eliminationTargetCount = Mathf.Max(1, count);
        }

        private void HandleTargetHit(GameObject target, bool isHeadshot)
        {
            // Check if all targets eliminated in elimination mode
            if (_isEliminationMode && _isSessionActive)
            {
                // TargetsHit is updated in Target.OnHit -> SessionStats.RegisterTargetHit
                // We need to check after the stat is updated, so we do it here
                // Note: The actual kill registration happens before this event
                // So we check >= instead of >
                if (_currentStats.TargetsHit >= _currentStats.TotalTargetsToEliminate)
                {
                    _currentStats.CompletionTime = _sessionTimer;
                    EventBus.TriggerAllTargetsEliminated();
                    EndSession();
                }
            }
        }

        private void HandleTargetMissed(GameObject target)
        {
            _currentStats.RegisterTargetMissed();
            EventBus.TriggerStatsUpdated();
        }

        private void HandleTargetSpawned(GameObject target)
        {
            _currentStats.RegisterTargetSpawned();
            EventBus.TriggerStatsUpdated();
        }

        private void HandleShoot()
        {
            // Shot registered, waiting for hit/miss
        }

        private void HandleHit(Vector3 position, bool isHeadshot)
        {
            _currentStats.RegisterShot(true, isHeadshot);
            EventBus.TriggerStatsUpdated();
        }

        private void HandleMiss()
        {
            _currentStats.RegisterShot(false, false);
            EventBus.TriggerStatsUpdated();
        }
    }
}
