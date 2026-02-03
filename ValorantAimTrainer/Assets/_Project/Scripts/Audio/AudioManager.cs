using System.Collections.Generic;
using UnityEngine;
using ValorantAimTrainer.Core;

namespace ValorantAimTrainer.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips - SFX")]
        [SerializeField] private AudioClip gunShot;
        [SerializeField] private AudioClip hitBody;
        [SerializeField] private AudioClip hitHeadDink;
        [SerializeField] private AudioClip targetSpawn;

        [Header("Audio Clips - UI")]
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonBack;

        [Header("Audio Clips - Session")]
        [SerializeField] private AudioClip countdownTick;
        [SerializeField] private AudioClip countdownGo;
        [SerializeField] private AudioClip sessionEnd;

        [Header("Audio Clips - Music")]
        [SerializeField] private AudioClip ambientMusic;
        [SerializeField] private bool playMusicOnStart = true;

        [Header("Volume Settings - Global")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float uiVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

        [Header("Volume Settings - Individual SFX")]
        [Range(0f, 2f)] [SerializeField] private float gunShotVolume = 1f;
        [Range(0f, 2f)] [SerializeField] private float hitBodyVolume = 1f;
        [Range(0f, 2f)] [SerializeField] private float hitHeadDinkVolume = 1f;
        [Range(0f, 2f)] [SerializeField] private float targetSpawnVolume = 0.7f;
        [Range(0f, 2f)] [SerializeField] private float countdownTickVolume = 1f;
        [Range(0f, 2f)] [SerializeField] private float countdownGoVolume = 1f;
        [Range(0f, 2f)] [SerializeField] private float sessionEndVolume = 1f;

        [Header("Volume Settings - Individual UI")]
        [Range(0f, 2f)] [SerializeField] private float buttonHoverVolume = 1f;
        [Range(0f, 2f)] [SerializeField] private float buttonClickVolume = 1f;
        [Range(0f, 2f)] [SerializeField] private float buttonBackVolume = 1f;

        [Header("Pool Settings")]
        [SerializeField] private int audioSourcePoolSize = 5;

        private Queue<AudioSource> _audioSourcePool;
        private List<AudioSource> _activeAudioSources;
        private AudioSource _musicSource;

        public float MasterVolume => masterVolume;
        public float SFXVolume => sfxVolume;
        public float UIVolume => uiVolume;
        public float MusicVolume => musicVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializeAudioPool();
            InitializeMusicSource();
        }

        private void Start()
        {
            if (playMusicOnStart && ambientMusic != null)
            {
                PlayMusic();
            }
        }

        private void OnEnable()
        {
            EventBus.OnShoot += PlayGunShot;
            EventBus.OnHit += HandleHit;
            EventBus.OnTargetSpawned += HandleTargetSpawned;
            EventBus.OnCountdownTick += HandleCountdownTick;
            EventBus.OnCountdownComplete += PlayCountdownGo;
            EventBus.OnSessionEnd += PlaySessionEnd;
        }

        private void OnDisable()
        {
            EventBus.OnShoot -= PlayGunShot;
            EventBus.OnHit -= HandleHit;
            EventBus.OnTargetSpawned -= HandleTargetSpawned;
            EventBus.OnCountdownTick -= HandleCountdownTick;
            EventBus.OnCountdownComplete -= PlayCountdownGo;
            EventBus.OnSessionEnd -= PlaySessionEnd;
        }

        private void InitializeAudioPool()
        {
            _audioSourcePool = new Queue<AudioSource>(audioSourcePoolSize);
            _activeAudioSources = new List<AudioSource>(audioSourcePoolSize);

            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _audioSourcePool.Enqueue(source);
            }
        }

        private void InitializeMusicSource()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = masterVolume * musicVolume;
        }

        private AudioSource GetAudioSource()
        {
            AudioSource source;

            if (_audioSourcePool.Count > 0)
            {
                source = _audioSourcePool.Dequeue();
            }
            else
            {
                // Find oldest playing source and reuse it
                source = _activeAudioSources[0];
                _activeAudioSources.RemoveAt(0);
            }

            _activeAudioSources.Add(source);
            return source;
        }

        private void ReturnAudioSource(AudioSource source)
        {
            _activeAudioSources.Remove(source);
            _audioSourcePool.Enqueue(source);
        }

        private void PlaySound(AudioClip clip, float volumeMultiplier, AudioCategory category)
        {
            if (clip == null) return;

            float categoryVolume = category switch
            {
                AudioCategory.SFX => sfxVolume,
                AudioCategory.UI => uiVolume,
                AudioCategory.Music => musicVolume,
                _ => 1f
            };

            float finalVolume = masterVolume * categoryVolume * volumeMultiplier;

            AudioSource source = GetAudioSource();
            source.clip = clip;
            source.volume = finalVolume;
            source.Play();

            // Return to pool after clip finishes
            StartCoroutine(ReturnAfterPlay(source, clip.length));
        }

        private System.Collections.IEnumerator ReturnAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnAudioSource(source);
        }

        // SFX Methods
        public void PlayGunShot() => PlaySound(gunShot, gunShotVolume, AudioCategory.SFX);
        public void PlayBodyHit() => PlaySound(hitBody, hitBodyVolume, AudioCategory.SFX);
        public void PlayHeadshotDink() => PlaySound(hitHeadDink, hitHeadDinkVolume, AudioCategory.SFX);
        public void PlayTargetSpawn() => PlaySound(targetSpawn, targetSpawnVolume, AudioCategory.SFX);

        // UI Methods
        public void PlayUIHover() => PlaySound(buttonHover, buttonHoverVolume, AudioCategory.UI);
        public void PlayUIClick() => PlaySound(buttonClick, buttonClickVolume, AudioCategory.UI);
        public void PlayUIBack() => PlaySound(buttonBack, buttonBackVolume, AudioCategory.UI);

        // Session Methods
        public void PlayCountdownTick() => PlaySound(countdownTick, countdownTickVolume, AudioCategory.SFX);
        public void PlayCountdownGo() => PlaySound(countdownGo, countdownGoVolume, AudioCategory.SFX);
        public void PlaySessionEnd() => PlaySound(sessionEnd, sessionEndVolume, AudioCategory.SFX);

        // Event Handlers
        private void HandleHit(Vector3 position, bool isHeadshot)
        {
            if (isHeadshot)
                PlayHeadshotDink();
            else
                PlayBodyHit();
        }

        private void HandleTargetSpawned(GameObject target)
        {
            PlayTargetSpawn();
        }

        private void HandleCountdownTick(int seconds)
        {
            PlayCountdownTick();
        }

        // Music Methods
        public void PlayMusic()
        {
            if (_musicSource == null || ambientMusic == null) return;

            _musicSource.clip = ambientMusic;
            _musicSource.volume = masterVolume * musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource == null) return;
            _musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (_musicSource == null) return;
            _musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (_musicSource == null) return;
            _musicSource.UnPause();
        }

        // Volume Setters
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
        }

        private void UpdateMusicVolume()
        {
            if (_musicSource != null)
            {
                _musicSource.volume = masterVolume * musicVolume;
            }
        }

        private enum AudioCategory
        {
            SFX,
            UI,
            Music
        }
    }
}
