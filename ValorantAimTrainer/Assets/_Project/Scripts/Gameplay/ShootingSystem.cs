using UnityEngine;
using UnityEngine.InputSystem;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.UI;

namespace ValorantAimTrainer.Gameplay
{
    /// <summary>
    /// Shooting system that matches Valorant's weapon mechanics.
    /// Handles fire rate, recoil feedback, and hit detection.
    /// </summary>
    public class ShootingSystem : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private WeaponData currentWeapon;

        [Header("Raycast Settings")]
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask hitLayers;

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CrosshairRenderer crosshairRenderer;
        [SerializeField] private WeaponViewmodel weaponViewmodel;

        // State
        private bool _canShoot = true;
        private float _nextFireTime = 0f;
        private int _currentAmmo;
        private bool _isReloading = false;

        // Spread tracking (for visual feedback)
        private float _currentSpread = 0f;
        private float _lastShotTime = 0f;

        // Properties
        public WeaponData CurrentWeapon => currentWeapon;
        public int CurrentAmmo => _currentAmmo;
        public bool IsReloading => _isReloading;

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (hitLayers == 0)
            {
                hitLayers = LayerMask.GetMask("Target_Head", "Target_Body", "Environment");
            }

            if (crosshairRenderer == null)
            {
                crosshairRenderer = FindFirstObjectByType<CrosshairRenderer>();
            }

            // Initialize ammo
            if (currentWeapon != null)
            {
                _currentAmmo = currentWeapon.MagazineSize;
            }
        }

        private void OnEnable()
        {
            EventBus.OnSessionStart += HandleSessionStart;
            EventBus.OnSessionEnd += DisableShooting;
            EventBus.OnSessionPause += DisableShooting;
            EventBus.OnSessionResume += EnableShooting;
        }

        private void OnDisable()
        {
            EventBus.OnSessionStart -= HandleSessionStart;
            EventBus.OnSessionEnd -= DisableShooting;
            EventBus.OnSessionPause -= DisableShooting;
            EventBus.OnSessionResume -= EnableShooting;
        }

        private void Update()
        {
            if (!_canShoot) return;
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (_isReloading) return;

            // Update spread decay
            UpdateSpread();

            // Handle shooting based on fire mode
            if (currentWeapon != null)
            {
                if (currentWeapon.IsAutomatic)
                {
                    // Full auto - hold to fire
                    if (Mouse.current.leftButton.isPressed && CanFireNow())
                    {
                        Shoot();
                    }
                }
                else
                {
                    // Semi-auto - click to fire
                    if (Mouse.current.leftButton.wasPressedThisFrame && CanFireNow())
                    {
                        Shoot();
                    }
                }
            }
            else
            {
                // Fallback: no weapon data, instant fire
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Shoot();
                }
            }

            // Manual reload with R key
            if (Keyboard.current.rKey.wasPressedThisFrame && _currentAmmo < currentWeapon?.MagazineSize)
            {
                StartReload();
            }
        }

        private bool CanFireNow()
        {
            // Check fire rate cooldown
            if (Time.time < _nextFireTime)
            {
                return false;
            }

            // Check ammo (skip if unlimited)
            if (currentWeapon != null && !currentWeapon.UnlimitedAmmo && _currentAmmo <= 0)
            {
                StartReload();
                return false;
            }

            return true;
        }

        private void Shoot()
        {
            // Set next fire time based on weapon fire rate
            if (currentWeapon != null)
            {
                _nextFireTime = Time.time + currentWeapon.TimeBetweenShots;

                // Only decrease ammo if not unlimited
                if (!currentWeapon.UnlimitedAmmo)
                {
                    _currentAmmo--;
                }

                // Increase spread
                _currentSpread += currentWeapon.SpreadIncreasePerShot;
                _lastShotTime = Time.time;
            }

            // Trigger firing error on crosshair
            crosshairRenderer?.TriggerFiringError(1f);

            // Play weapon viewmodel animation (muzzle flash handled via EventBus)
            weaponViewmodel?.PlayFireAnimation();

            // Fire event
            EventBus.TriggerShoot();

            // Raycast
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitLayers))
            {
                ProcessHit(hit);
            }
            else
            {
                ProcessMiss();
            }

            // Auto reload when empty (skip if unlimited ammo)
            if (currentWeapon != null && !currentWeapon.UnlimitedAmmo && _currentAmmo <= 0)
            {
                StartReload();
            }
        }

        private void UpdateSpread()
        {
            if (currentWeapon == null) return;

            // Decay spread over time
            float timeSinceLastShot = Time.time - _lastShotTime;
            if (timeSinceLastShot > 0)
            {
                float decayRate = currentWeapon.FirstShotSpread / currentWeapon.SpreadResetTime;
                _currentSpread = Mathf.Max(currentWeapon.FirstShotSpread, _currentSpread - decayRate * Time.deltaTime);
            }
        }

        private void ProcessHit(RaycastHit hit)
        {
            HitZone hitZone = hit.collider.GetComponent<HitZone>();

            if (hitZone != null)
            {
                bool isHeadshot = hitZone.IsHead;
                hitZone.RegisterHit();
                EventBus.TriggerHit(hit.point, isHeadshot);
            }
            else
            {
                ProcessMiss();
            }
        }

        private void ProcessMiss()
        {
            EventBus.TriggerMiss();
        }

        private void StartReload()
        {
            if (_isReloading) return;
            if (currentWeapon == null) return;

            _isReloading = true;

            // Play reload animation on viewmodel
            weaponViewmodel?.PlayReloadAnimation();

            StartCoroutine(ReloadCoroutine());
        }

        private System.Collections.IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(currentWeapon.ReloadTime);

            _currentAmmo = currentWeapon.MagazineSize;
            _isReloading = false;

            // Notify viewmodel that reload is complete
            weaponViewmodel?.OnReloadComplete();
        }

        private void HandleSessionStart()
        {
            EnableShooting();
            // Reset ammo on session start
            if (currentWeapon != null)
            {
                _currentAmmo = currentWeapon.MagazineSize;
            }
            _isReloading = false;
            _currentSpread = currentWeapon?.FirstShotSpread ?? 0f;
        }

        private void EnableShooting()
        {
            _canShoot = true;
        }

        private void DisableShooting()
        {
            _canShoot = false;
        }

        public void SetWeapon(WeaponData weapon)
        {
            currentWeapon = weapon;
            if (weapon != null)
            {
                _currentAmmo = weapon.MagazineSize;
            }
        }

        public void SetCamera(Camera cam)
        {
            mainCamera = cam;
        }
    }
}
