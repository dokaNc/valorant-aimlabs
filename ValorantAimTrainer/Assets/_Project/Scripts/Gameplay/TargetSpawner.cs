using System.Collections.Generic;
using UnityEngine;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;

namespace ValorantAimTrainer.Gameplay
{
    public class TargetSpawner : MonoBehaviour
    {
        public static TargetSpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private int poolSize = 5;
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private int maxActiveTargets = 1;

        [Header("Spawn Area")]
        [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
        [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 3f, 10f);
        [SerializeField] private float minSpawnDistance = 5f;
        [SerializeField] private float maxSpawnDistance = 20f;
        [SerializeField] private float modelRotationOffset = 0f;

        [Header("Fixed Spawn Points (Elimination Mode)")]
        [Tooltip("Predefined spawn positions for Elimination mode")]
        [SerializeField] private Transform[] fixedSpawnPoints;

        [Header("References")]
        [SerializeField] private Transform playerTransform;

        private ObjectPool<Target> _targetPool;
        private GameObject _targetPrefab;
        private float _nextSpawnTime;
        private bool _isSpawning;
        private List<SpawnZone> _spawnZones = new List<SpawnZone>();

        // Elimination mode state
        private bool _isEliminationMode;
        private bool _eliminationTargetsSpawned;
        private int _eliminationTargetCount;
        private bool _eliminationStrafingEnabled;
        private ModeConfiguration _currentModeConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnSessionStart += StartSpawning;
            EventBus.OnSessionEnd += StopSpawning;
            EventBus.OnSessionPause += PauseSpawning;
            EventBus.OnSessionResume += ResumeSpawning;
        }

        private void OnDisable()
        {
            EventBus.OnSessionStart -= StartSpawning;
            EventBus.OnSessionEnd -= StopSpawning;
            EventBus.OnSessionPause -= PauseSpawning;
            EventBus.OnSessionResume -= ResumeSpawning;
        }

        private void Update()
        {
            if (!_isSpawning) return;

            if (Time.time >= _nextSpawnTime && _targetPool.ActiveCount < maxActiveTargets)
            {
                SpawnTarget();
                _nextSpawnTime = Time.time + spawnInterval;
            }
        }

        public void Initialize(AgentData agentData)
        {
            if (agentData?.ModelPrefab == null)
            {
                Debug.LogError("[TargetSpawner] Invalid agent data or missing prefab");
                return;
            }

            _targetPrefab = agentData.ModelPrefab;

            // Create pool with target prefab
            Target targetComponent = _targetPrefab.GetComponent<Target>();
            if (targetComponent == null)
            {
                Debug.LogError("[TargetSpawner] Target prefab missing Target component");
                return;
            }

            _targetPool = new ObjectPool<Target>(targetComponent, transform, poolSize);
        }

        public void RegisterSpawnZone(SpawnZone zone)
        {
            if (!_spawnZones.Contains(zone))
            {
                _spawnZones.Add(zone);
            }
        }

        public void UnregisterSpawnZone(SpawnZone zone)
        {
            _spawnZones.Remove(zone);
        }

        private void StartSpawning()
        {
            _isSpawning = true;
            _nextSpawnTime = Time.time;

            // For elimination mode, spawn all fixed targets at once
            if (_isEliminationMode && !_eliminationTargetsSpawned)
            {
                SpawnFixedTargets();
                _eliminationTargetsSpawned = true;
                _isSpawning = false; // No continuous spawning in elimination
            }
        }

        private void StopSpawning()
        {
            _isSpawning = false;
            _eliminationTargetsSpawned = false;
            _targetPool?.ReturnAll();
        }

        private void PauseSpawning()
        {
            _isSpawning = false;
        }

        private void ResumeSpawning()
        {
            _isSpawning = true;
        }

        private void SpawnTarget()
        {
            if (_targetPool == null) return;

            // Auto-find player if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("Player");
                if (player != null) playerTransform = player.transform;
            }

            Target target = _targetPool.Get();
            Vector3 spawnPos;
            SpawnZone usedZone = null;

            // Get spawn position and track which zone was used
            if (_spawnZones.Count > 0)
            {
                usedZone = GetRandomSpawnZone();
                spawnPos = usedZone.GetRandomPointInZone();
            }
            else
            {
                spawnPos = GetDefaultSpawnPosition();
            }

            // Set position and rotation FIRST
            target.transform.position = spawnPos;
            target.transform.rotation = GetSpawnRotation(spawnPos);

            // Pass zone bounds to target for strafe constraints
            if (usedZone != null)
            {
                target.SetStrafeBounds(usedZone.WorldBounds);
            }

            // Initialize strafe AFTER position is set
            target.InitializeStrafe(playerTransform);
        }

        private Vector3 GetDefaultSpawnPosition()
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                0f,
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            );

            Vector3 spawnPos = spawnAreaCenter + randomOffset;

            // Apply distance constraints from player
            if (playerTransform != null)
            {
                Vector3 directionFromPlayer = (spawnPos - playerTransform.position).normalized;
                float distanceFromPlayer = Vector3.Distance(spawnPos, playerTransform.position);

                if (distanceFromPlayer < minSpawnDistance)
                {
                    spawnPos = playerTransform.position + directionFromPlayer * minSpawnDistance;
                }
                else if (distanceFromPlayer > maxSpawnDistance)
                {
                    spawnPos = playerTransform.position + directionFromPlayer * maxSpawnDistance;
                }
            }

            return spawnPos;
        }

        private Quaternion GetSpawnRotation(Vector3 spawnPosition)
        {
            // Auto-find player if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            if (playerTransform != null)
            {
                // Use direction AWAY from player, then model's -Z faces player
                Vector3 directionAwayFromPlayer = spawnPosition - playerTransform.position;
                directionAwayFromPlayer.y = 0f;
                if (directionAwayFromPlayer != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionAwayFromPlayer);
                    return lookRotation;
                }
            }

            return Quaternion.identity;
        }

        private SpawnZone GetRandomSpawnZone()
        {
            float totalWeight = 0f;
            foreach (var zone in _spawnZones)
            {
                totalWeight += zone.Weight;
            }

            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var zone in _spawnZones)
            {
                cumulative += zone.Weight;
                if (random <= cumulative)
                {
                    return zone;
                }
            }

            return _spawnZones[0];
        }

        public void ReturnTarget(Target target)
        {
            _targetPool?.Return(target);
        }

        public void SetSpawnInterval(float interval)
        {
            spawnInterval = interval;
        }

        public void SetMaxActiveTargets(int max)
        {
            maxActiveTargets = max;
        }

        /// <summary>
        /// Configure spawner for elimination mode
        /// </summary>
        public void ConfigureForElimination(int targetCount, bool enableStrafing = false)
        {
            _isEliminationMode = true;
            _eliminationTargetCount = targetCount;
            _eliminationStrafingEnabled = enableStrafing;
            _eliminationTargetsSpawned = false;
        }

        /// <summary>
        /// Configure spawner with mode configuration
        /// </summary>
        public void ConfigureWithMode(ModeConfiguration config)
        {
            _currentModeConfig = config;

            if (config.IsEliminationMode)
            {
                ConfigureForElimination(config.TargetCount, config.TargetsStrafe);
            }
            else
            {
                _isEliminationMode = false;
                spawnInterval = config.SpawnInterval;
                maxActiveTargets = config.MaxActiveTargets;
            }
        }

        /// <summary>
        /// Reset to standard spawning mode
        /// </summary>
        public void ResetToStandardMode()
        {
            _isEliminationMode = false;
            _eliminationTargetsSpawned = false;
            _currentModeConfig = null;
        }

        /// <summary>
        /// Spawn targets at fixed spawn points for elimination mode
        /// </summary>
        private void SpawnFixedTargets()
        {
            if (_targetPool == null) return;

            // Auto-find player if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("Player");
                if (player != null) playerTransform = player.transform;
            }

            int count = Mathf.Min(_eliminationTargetCount, fixedSpawnPoints?.Length ?? 0);

            // If not enough fixed points, use what we have + random positions
            if (fixedSpawnPoints == null || fixedSpawnPoints.Length == 0)
            {
                // Fall back to random positions
                for (int i = 0; i < _eliminationTargetCount; i++)
                {
                    SpawnEliminationTarget(GetDefaultSpawnPosition());
                }
            }
            else
            {
                // Spawn at fixed points
                for (int i = 0; i < fixedSpawnPoints.Length && i < _eliminationTargetCount; i++)
                {
                    if (fixedSpawnPoints[i] != null)
                    {
                        SpawnEliminationTarget(fixedSpawnPoints[i].position);
                    }
                }

                // If we need more targets than fixed points, add random ones
                for (int i = fixedSpawnPoints.Length; i < _eliminationTargetCount; i++)
                {
                    SpawnEliminationTarget(GetDefaultSpawnPosition());
                }
            }
        }

        /// <summary>
        /// Spawn a single target configured for elimination mode
        /// </summary>
        private void SpawnEliminationTarget(Vector3 position)
        {
            Target target = _targetPool.Get();

            target.transform.position = position;
            target.transform.rotation = GetSpawnRotation(position);

            // Configure for elimination: no timeout, optional strafing
            target.SetInfiniteLifetime(true);
            target.SetStrafingEnabled(_eliminationStrafingEnabled);

            // Initialize strafe if enabled
            if (_eliminationStrafingEnabled)
            {
                target.InitializeStrafe(playerTransform);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

            if (playerTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerTransform.position, minSpawnDistance);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(playerTransform.position, maxSpawnDistance);
            }

            // Draw fixed spawn points
            if (fixedSpawnPoints != null)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < fixedSpawnPoints.Length; i++)
                {
                    if (fixedSpawnPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(fixedSpawnPoints[i].position, 0.5f);
                        Gizmos.DrawLine(fixedSpawnPoints[i].position, fixedSpawnPoints[i].position + Vector3.up * 2f);
                    }
                }
            }
        }
    }
}
