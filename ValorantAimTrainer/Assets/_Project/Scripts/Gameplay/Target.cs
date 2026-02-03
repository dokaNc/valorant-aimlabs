using UnityEngine;
using ValorantAimTrainer.Core;

namespace ValorantAimTrainer.Gameplay
{
    public class Target : MonoBehaviour, IPoolable
    {
        [Header("Settings")]
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool despawnOnHit = true;

        [Header("Health")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private int headshotDamage = 3; // Instant kill
        [SerializeField] private int bodyDamage = 1;     // 3 shots to kill

        [Header("Movement - Strafing")]
        [SerializeField] private bool enableStrafing = true;
        [SerializeField] private float strafeSpeed = 3f;
        [SerializeField] private float strafeDistance = 2f;
        [SerializeField] private float directionChangeInterval = 0.8f;
        [SerializeField] private float directionChangeRandomness = 0.3f;

        // Runtime strafing override (for Elimination mode)
        private bool _strafingOverride = false;
        private bool _strafingOverrideValue = false;

        // Infinite lifetime for elimination mode
        private bool _infiniteLifetime = false;

        private float _spawnTime;
        private float _remainingLifetime;
        private bool _isActive;
        private bool _hasBeenHit;
        private int _currentHealth;

        // Strafing state
        private Vector3 _strafeAxis; // The axis along which we strafe (horizontal, perpendicular to forward)
        private int _strafeDir; // +1 or -1
        private Vector3 _spawnPosition;
        private float _nextDirectionChangeTime;
        private float _currentStrafeOffset;
        private Bounds? _strafeBounds; // Optional bounds to constrain movement

        // Animation
        private Animator _animator;
        private static readonly int StrafeDirectionParam = Animator.StringToHash("StrafeDirection");

        public float SpawnTime => _spawnTime;
        public bool IsActive => _isActive;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();

            // DEBUG
            if (_animator == null)
            {
                Debug.LogError($"[Target] No Animator found on {gameObject.name} or children!");
            }
            else
            {
                Debug.Log($"[Target] Animator found on {_animator.gameObject.name}");
                if (_animator.runtimeAnimatorController == null)
                {
                    Debug.LogError($"[Target] Animator has NO controller assigned!");
                }
                else
                {
                    Debug.Log($"[Target] Controller: {_animator.runtimeAnimatorController.name}");
                }
                if (_animator.avatar == null)
                {
                    Debug.LogWarning($"[Target] Animator has NO avatar assigned!");
                }
                else
                {
                    Debug.Log($"[Target] Avatar: {_animator.avatar.name}, isHuman: {_animator.avatar.isHuman}");
                }
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            // Only decrease lifetime if not infinite
            if (!_infiniteLifetime)
            {
                _remainingLifetime -= Time.deltaTime;

                if (_remainingLifetime <= 0f && !_hasBeenHit)
                {
                    OnMissed();
                    return;
                }
            }

            // Check strafing: override takes priority, then serialized value
            bool shouldStrafe = _strafingOverride ? _strafingOverrideValue : enableStrafing;
            if (shouldStrafe)
            {
                UpdateStrafing();
            }
        }

        private void UpdateStrafing()
        {
            // Check if it's time to potentially change direction
            if (Time.time >= _nextDirectionChangeTime)
            {
                if (Random.value < 0.5f + directionChangeRandomness)
                {
                    _strafeDir = -_strafeDir;
                }
                _nextDirectionChangeTime = Time.time + directionChangeInterval + Random.Range(-0.2f, 0.2f);
            }

            // Update offset based on current direction
            _currentStrafeOffset += strafeSpeed * _strafeDir * Time.deltaTime;

            // Calculate potential new position
            Vector3 newPosition = _spawnPosition + _strafeAxis * _currentStrafeOffset;

            // Check against bounds if defined
            if (_strafeBounds.HasValue)
            {
                Bounds bounds = _strafeBounds.Value;

                // Check if new position is outside bounds (with small margin)
                if (newPosition.x < bounds.min.x || newPosition.x > bounds.max.x ||
                    newPosition.z < bounds.min.z || newPosition.z > bounds.max.z)
                {
                    // Reverse direction and recalculate
                    _strafeDir = -_strafeDir;
                    _currentStrafeOffset += strafeSpeed * _strafeDir * Time.deltaTime * 2f;
                    newPosition = _spawnPosition + _strafeAxis * _currentStrafeOffset;

                    // Clamp to bounds
                    newPosition.x = Mathf.Clamp(newPosition.x, bounds.min.x + 0.1f, bounds.max.x - 0.1f);
                    newPosition.z = Mathf.Clamp(newPosition.z, bounds.min.z + 0.1f, bounds.max.z - 0.1f);
                }
            }
            else
            {
                // No bounds, use strafeDistance
                if (_currentStrafeOffset > strafeDistance)
                {
                    _currentStrafeOffset = strafeDistance;
                    _strafeDir = -1;
                }
                else if (_currentStrafeOffset < -strafeDistance)
                {
                    _currentStrafeOffset = -strafeDistance;
                    _strafeDir = 1;
                }
                newPosition = _spawnPosition + _strafeAxis * _currentStrafeOffset;
            }

            newPosition.y = _spawnPosition.y;
            transform.position = newPosition;

            // Update animation
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.SetFloat(StrafeDirectionParam, _strafeDir);
                // DEBUG - log every 60 frames
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Target] StrafeDirection = {_strafeDir}, Animator state: {_animator.GetCurrentAnimatorStateInfo(0).shortNameHash}");
                }
            }
        }

        public void SetStrafeBounds(Bounds bounds)
        {
            _strafeBounds = bounds;
        }

        /// <summary>
        /// Called by spawner AFTER position is set to initialize strafe parameters
        /// </summary>
        public void InitializeStrafe(Transform playerTransform)
        {
            _spawnPosition = transform.position;
            _currentStrafeOffset = 0f;

            if (playerTransform != null)
            {
                // Direction from target to player (horizontal only)
                Vector3 toPlayer = playerTransform.position - transform.position;
                toPlayer.y = 0f;

                if (toPlayer.sqrMagnitude > 0.001f)
                {
                    toPlayer.Normalize();
                    // Strafe axis is perpendicular to player direction (left-right from player's view)
                    _strafeAxis = Vector3.Cross(Vector3.up, toPlayer).normalized;
                }
                else
                {
                    _strafeAxis = Vector3.right;
                }
            }
            else
            {
                _strafeAxis = Vector3.right;
            }

            // Random initial direction (+1 = right, -1 = left)
            _strafeDir = Random.value > 0.5f ? 1 : -1;
            _nextDirectionChangeTime = Time.time + directionChangeInterval;

            // Start animation based on initial direction
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.SetFloat(StrafeDirectionParam, _strafeDir);
            }
        }

        public void OnSpawn()
        {
            _spawnTime = Time.time;
            _remainingLifetime = lifetime;
            _isActive = true;
            _hasBeenHit = false;
            _currentHealth = maxHealth;
            _strafeBounds = null;
            _strafeAxis = Vector3.right; // Default, will be overwritten by InitializeStrafe
            _strafingOverride = false;
            _infiniteLifetime = false;

            EventBus.TriggerTargetSpawned(gameObject);
        }

        public void OnDespawn()
        {
            _isActive = false;
            EventBus.TriggerTargetDespawned(gameObject);
        }

        public void OnHit(bool isHeadshot)
        {
            if (!_isActive) return;

            // Apply damage
            int damage = isHeadshot ? headshotDamage : bodyDamage;
            _currentHealth -= damage;

            // Check if dead - register kill BEFORE triggering event
            // so that event handlers can check accurate kill count
            bool wasKilled = false;
            if (_currentHealth <= 0)
            {
                if (!_hasBeenHit) // Not already counted as killed
                {
                    _hasBeenHit = true;
                    wasKilled = true;

                    float reactionTime = Time.time - _spawnTime;
                    SessionManager.Instance?.CurrentStats.RegisterTargetHit(reactionTime);
                }
            }

            // Trigger hit event for feedback (sound, particles, stats)
            // This happens AFTER RegisterTargetHit so counters are accurate
            EventBus.TriggerTargetHit(gameObject, isHeadshot);

            // Despawn if killed
            if (wasKilled && despawnOnHit)
            {
                RequestDespawn();
            }
        }

        private void OnMissed()
        {
            if (!_isActive || _hasBeenHit) return;

            EventBus.TriggerTargetMissed(gameObject);
            RequestDespawn();
        }

        private void RequestDespawn()
        {
            _isActive = false;
            TargetSpawner.Instance?.ReturnTarget(this);
        }

        public void SetLifetime(float newLifetime)
        {
            lifetime = newLifetime;
            _remainingLifetime = newLifetime;
        }

        /// <summary>
        /// Enable or disable strafing at runtime (overrides serialized value)
        /// </summary>
        public void SetStrafingEnabled(bool enabled)
        {
            _strafingOverride = true;
            _strafingOverrideValue = enabled;
        }

        /// <summary>
        /// Clear strafing override, revert to serialized value
        /// </summary>
        public void ClearStrafingOverride()
        {
            _strafingOverride = false;
        }

        /// <summary>
        /// Set infinite lifetime (target won't despawn on its own)
        /// Used for Elimination mode where targets stay until killed
        /// </summary>
        public void SetInfiniteLifetime(bool infinite)
        {
            _infiniteLifetime = infinite;
            if (infinite)
            {
                _remainingLifetime = float.MaxValue;
            }
        }

        /// <summary>
        /// Check if this target has infinite lifetime
        /// </summary>
        public bool HasInfiniteLifetime => _infiniteLifetime;
    }
}
