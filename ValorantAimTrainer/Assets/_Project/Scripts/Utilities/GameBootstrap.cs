using UnityEngine;
using ValorantAimTrainer.Core;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.Gameplay;

namespace ValorantAimTrainer.Utilities
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Auto-Start Settings")]
        [SerializeField] private bool autoStartSession = false; // Changed to false - show menu first
        [SerializeField] private float autoStartDelay = 1f;

        [Header("Session Settings")]
        [SerializeField] private float sessionDuration = 60f;

        [Header("Agent Settings")]
        [SerializeField] private AgentData defaultAgent;

        private void Start()
        {
            InitializeGame();

            // Start at main menu
            GameManager.Instance?.SetState(GameState.MainMenu);

            if (autoStartSession)
            {
                Invoke(nameof(StartSession), autoStartDelay);
            }
        }

        private void InitializeGame()
        {
            // Initialize TargetSpawner with selected or default agent
            AgentData agent = GameManager.Instance?.SelectedAgent ?? defaultAgent;

            if (agent != null && TargetSpawner.Instance != null)
            {
                TargetSpawner.Instance.Initialize(agent);
            }
            else
            {
                Debug.LogWarning("[GameBootstrap] No agent available for initialization");
            }

            // Set session duration
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.SetSessionDuration(sessionDuration);
            }
        }

        private void StartSession()
        {
            SessionManager.Instance?.StartCountdown();
        }

        [ContextMenu("Start Session Now")]
        public void StartSessionManual()
        {
            StartSession();
        }

        [ContextMenu("End Session Now")]
        public void EndSessionManual()
        {
            SessionManager.Instance?.EndSession();
        }
    }
}
