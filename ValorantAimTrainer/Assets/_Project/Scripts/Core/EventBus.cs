using System;
using UnityEngine;

namespace ValorantAimTrainer.Core
{
    public static class EventBus
    {
        // Session Events
        public static event Action OnSessionStart;
        public static event Action OnSessionEnd;
        public static event Action OnSessionPause;
        public static event Action OnSessionResume;

        // Target Events
        public static event Action<GameObject> OnTargetSpawned;
        public static event Action<GameObject, bool> OnTargetHit; // GameObject, isHeadshot
        public static event Action<GameObject> OnTargetMissed;
        public static event Action<GameObject> OnTargetDespawned;

        // Shooting Events
        public static event Action OnShoot;
        public static event Action<Vector3, bool> OnHit; // position, isHeadshot
        public static event Action OnMiss;

        // Game State Events
        public static event Action<GameState> OnGameStateChanged;

        // Stats Events
        public static event Action OnStatsUpdated;

        // Countdown Events
        public static event Action<int> OnCountdownTick; // seconds remaining
        public static event Action OnCountdownComplete;

        // Elimination Mode Events
        public static event Action OnAllTargetsEliminated;

        // Session Events Triggers
        public static void TriggerSessionStart() => OnSessionStart?.Invoke();
        public static void TriggerSessionEnd() => OnSessionEnd?.Invoke();
        public static void TriggerSessionPause() => OnSessionPause?.Invoke();
        public static void TriggerSessionResume() => OnSessionResume?.Invoke();

        // Target Events Triggers
        public static void TriggerTargetSpawned(GameObject target) => OnTargetSpawned?.Invoke(target);
        public static void TriggerTargetHit(GameObject target, bool isHeadshot) => OnTargetHit?.Invoke(target, isHeadshot);
        public static void TriggerTargetMissed(GameObject target) => OnTargetMissed?.Invoke(target);
        public static void TriggerTargetDespawned(GameObject target) => OnTargetDespawned?.Invoke(target);

        // Shooting Events Triggers
        public static void TriggerShoot() => OnShoot?.Invoke();
        public static void TriggerHit(Vector3 position, bool isHeadshot) => OnHit?.Invoke(position, isHeadshot);
        public static void TriggerMiss() => OnMiss?.Invoke();

        // Game State Events Triggers
        public static void TriggerGameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);

        // Stats Events Triggers
        public static void TriggerStatsUpdated() => OnStatsUpdated?.Invoke();

        // Countdown Events Triggers
        public static void TriggerCountdownTick(int seconds) => OnCountdownTick?.Invoke(seconds);
        public static void TriggerCountdownComplete() => OnCountdownComplete?.Invoke();

        // Elimination Mode Events Triggers
        public static void TriggerAllTargetsEliminated() => OnAllTargetsEliminated?.Invoke();

        // Clear all subscribers (useful for cleanup)
        public static void ClearAll()
        {
            OnSessionStart = null;
            OnSessionEnd = null;
            OnSessionPause = null;
            OnSessionResume = null;
            OnTargetSpawned = null;
            OnTargetHit = null;
            OnTargetMissed = null;
            OnTargetDespawned = null;
            OnShoot = null;
            OnHit = null;
            OnMiss = null;
            OnGameStateChanged = null;
            OnStatsUpdated = null;
            OnCountdownTick = null;
            OnCountdownComplete = null;
            OnAllTargetsEliminated = null;
        }
    }

    public enum GameState
    {
        MainMenu,
        ModeSelect,
        AgentSelect,
        MapSelect,
        Loading,
        Countdown,
        Playing,
        Paused,
        Results
    }
}
