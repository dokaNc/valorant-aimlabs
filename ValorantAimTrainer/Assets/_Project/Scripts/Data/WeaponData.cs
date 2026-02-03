using UnityEngine;

namespace ValorantAimTrainer.Data
{
    /// <summary>
    /// Weapon data matching Valorant's exact weapon specifications.
    ///
    /// VALORANT WEAPON STATS (verified sources):
    /// - Fire rate is measured in rounds per second
    /// - Time between shots = 1 / fireRate
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ValorantAimTrainer/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public enum WeaponType
        {
            Sidearm,
            SMG,
            Shotgun,
            Rifle,
            Sniper,
            MachineGun
        }

        public enum FireMode
        {
            SemiAuto,   // One shot per click
            FullAuto,   // Hold to spray
            Burst       // Multiple shots per click
        }

        [Header("Weapon Info")]
        [SerializeField] private string weaponName = "Classic";
        [SerializeField] private WeaponType weaponType = WeaponType.Sidearm;
        [SerializeField] private FireMode fireMode = FireMode.SemiAuto;

        [Header("Fire Rate")]
        [Tooltip("Rounds per second (Valorant's fire rate stat)")]
        [SerializeField] private float fireRate = 6.75f;

        [Header("Magazine")]
        [SerializeField] private int magazineSize = 12;
        [SerializeField] private float reloadTime = 1.75f;
        [SerializeField] private bool unlimitedAmmo = false;

        [Header("Accuracy (for visual feedback only)")]
        [Tooltip("First shot accuracy - 0 = perfect, higher = more spread")]
        [SerializeField] private float firstShotSpread = 0.4f;
        [Tooltip("Spread increase per shot when firing rapidly")]
        [SerializeField] private float spreadIncreasePerShot = 0.5f;
        [Tooltip("Time for spread to fully reset")]
        [SerializeField] private float spreadResetTime = 0.35f;

        [Header("Viewmodel")]
        [Tooltip("The weapon viewmodel prefab for FPS view")]
        [SerializeField] private GameObject viewmodelPrefab;

        // Properties
        public string WeaponName => weaponName;
        public WeaponType Type => weaponType;
        public FireMode Mode => fireMode;
        public float FireRate => fireRate;
        public int MagazineSize => magazineSize;
        public float ReloadTime => reloadTime;
        public bool UnlimitedAmmo => unlimitedAmmo;
        public float FirstShotSpread => firstShotSpread;
        public float SpreadIncreasePerShot => spreadIncreasePerShot;
        public float SpreadResetTime => spreadResetTime;
        public GameObject ViewmodelPrefab => viewmodelPrefab;

        /// <summary>
        /// Time between shots in seconds.
        /// Classic: 1/6.75 = 0.148s = 148ms
        /// </summary>
        public float TimeBetweenShots => 1f / fireRate;

        /// <summary>
        /// Can this weapon fire while holding the mouse button?
        /// </summary>
        public bool IsAutomatic => fireMode == FireMode.FullAuto;
    }
}
