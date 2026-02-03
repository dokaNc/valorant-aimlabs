using UnityEngine;
using ValorantAimTrainer.Core;

namespace ValorantAimTrainer.Gameplay
{
    /// <summary>
    /// Controls the visible weapon model in FPS view.
    /// Handles fire animations, muzzle flash, and reload animations.
    /// The actual shooting is handled by ShootingSystem via raycast.
    /// </summary>
    public class WeaponViewmodel : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The 3D weapon model")]
        [SerializeField] private GameObject weaponModel;

        [Tooltip("Animator for weapon animations")]
        [SerializeField] private Animator animator;

        [Tooltip("Muzzle flash particle system")]
        [SerializeField] private ParticleSystem muzzleFlash;

        [Tooltip("Optional: Weapon sway component")]
        [SerializeField] private WeaponSway weaponSway;

        [Header("Animation Parameters")]
        [SerializeField] private string fireAnimTrigger = "Fire";
        [SerializeField] private string reloadAnimTrigger = "Reload";
        [SerializeField] private string idleAnimState = "Idle";

        [Header("Fire Effect Settings")]
        [Tooltip("Duration of muzzle flash")]
        [SerializeField] private float muzzleFlashDuration = 0.05f;

        [Tooltip("Recoil kick amount for weapon sway")]
        [SerializeField] private float recoilKickAmount = 2f;

        [Header("Position Settings")]
        [Tooltip("Offset from camera for viewmodel position")]
        [SerializeField] private Vector3 viewmodelOffset = new Vector3(0.2f, -0.15f, 0.4f);

        [Tooltip("Rotation offset for viewmodel")]
        [SerializeField] private Vector3 viewmodelRotation = Vector3.zero;

        // State
        private bool _isReloading = false;
        private Coroutine _muzzleFlashCoroutine;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (weaponSway == null)
            {
                weaponSway = GetComponent<WeaponSway>();
            }

            if (muzzleFlash != null)
            {
                muzzleFlash.Stop();
            }
        }

        private void OnEnable()
        {
            EventBus.OnShoot += HandleShoot;
            EventBus.OnSessionStart += HandleSessionStart;
            EventBus.OnSessionEnd += HandleSessionEnd;
        }

        private void OnDisable()
        {
            EventBus.OnShoot -= HandleShoot;
            EventBus.OnSessionStart -= HandleSessionStart;
            EventBus.OnSessionEnd -= HandleSessionEnd;
        }

        private void HandleShoot()
        {
            PlayFireAnimation();
        }

        private void HandleSessionStart()
        {
            Show();
            ResetToIdle();
        }

        private void HandleSessionEnd()
        {
            ResetToIdle();
        }

        /// <summary>
        /// Play the fire animation and muzzle flash effect.
        /// Called automatically when EventBus.OnShoot is triggered.
        /// </summary>
        public void PlayFireAnimation()
        {
            if (_isReloading) return;

            // Play animation
            if (animator != null)
            {
                animator.SetTrigger(fireAnimTrigger);
            }

            // Play muzzle flash
            PlayMuzzleFlash();

            // Add recoil kick to weapon sway
            if (weaponSway != null)
            {
                weaponSway.AddRecoilKick(recoilKickAmount);
            }
        }

        /// <summary>
        /// Play the reload animation.
        /// Should be called by ShootingSystem when reload starts.
        /// </summary>
        public void PlayReloadAnimation()
        {
            if (_isReloading) return;

            _isReloading = true;

            if (animator != null)
            {
                animator.SetTrigger(reloadAnimTrigger);
            }
        }

        /// <summary>
        /// Called when reload animation completes.
        /// Can be called by animation event or manually.
        /// </summary>
        public void OnReloadComplete()
        {
            _isReloading = false;
        }

        /// <summary>
        /// Reset the viewmodel to idle state.
        /// </summary>
        public void ResetToIdle()
        {
            _isReloading = false;

            if (animator != null)
            {
                animator.Play(idleAnimState, 0, 0f);
            }

            if (weaponSway != null)
            {
                weaponSway.ResetSway();
            }

            StopMuzzleFlash();
        }

        /// <summary>
        /// Show the weapon viewmodel.
        /// </summary>
        public void Show()
        {
            if (weaponModel != null)
            {
                weaponModel.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the weapon viewmodel.
        /// </summary>
        public void Hide()
        {
            if (weaponModel != null)
            {
                weaponModel.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Setup the viewmodel position relative to camera.
        /// </summary>
        public void SetupPosition(Transform cameraTransform)
        {
            if (cameraTransform == null) return;

            transform.SetParent(cameraTransform);
            transform.localPosition = viewmodelOffset;
            transform.localRotation = Quaternion.Euler(viewmodelRotation);
        }

        private void PlayMuzzleFlash()
        {
            if (muzzleFlash == null) return;

            // Stop any existing muzzle flash
            if (_muzzleFlashCoroutine != null)
            {
                StopCoroutine(_muzzleFlashCoroutine);
            }

            muzzleFlash.Play();
            _muzzleFlashCoroutine = StartCoroutine(StopMuzzleFlashAfterDelay());
        }

        private System.Collections.IEnumerator StopMuzzleFlashAfterDelay()
        {
            yield return new WaitForSeconds(muzzleFlashDuration);
            StopMuzzleFlash();
        }

        private void StopMuzzleFlash()
        {
            if (muzzleFlash != null && muzzleFlash.isPlaying)
            {
                muzzleFlash.Stop();
            }
            _muzzleFlashCoroutine = null;
        }

        /// <summary>
        /// Set viewmodel offset at runtime.
        /// </summary>
        public void SetViewmodelOffset(Vector3 offset)
        {
            viewmodelOffset = offset;
            transform.localPosition = viewmodelOffset;
        }

        /// <summary>
        /// Set viewmodel rotation at runtime.
        /// </summary>
        public void SetViewmodelRotation(Vector3 rotation)
        {
            viewmodelRotation = rotation;
            transform.localRotation = Quaternion.Euler(viewmodelRotation);
        }

        // Properties
        public bool IsReloading => _isReloading;
        public WeaponSway Sway => weaponSway;
    }
}
