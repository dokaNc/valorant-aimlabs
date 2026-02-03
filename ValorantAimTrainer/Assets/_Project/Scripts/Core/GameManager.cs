using UnityEngine;
using ValorantAimTrainer.Data;

namespace ValorantAimTrainer.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private AgentData[] availableAgents;
        [SerializeField] private MapData[] availableMaps;

        private GameState _currentState = GameState.Loading; // Start with Loading so SetState(MainMenu) triggers event
        private AgentData _selectedAgent;
        private MapData _selectedMap;
        private TrainingMode _selectedMode;

        public GameState CurrentState => _currentState;
        public AgentData SelectedAgent => _selectedAgent;
        public MapData SelectedMap => _selectedMap;
        public TrainingMode SelectedMode => _selectedMode;
        public AgentData[] AvailableAgents => availableAgents;
        public MapData[] AvailableMaps => availableMaps;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            EventBus.TriggerGameStateChanged(newState);
        }

        public void SelectAgent(AgentData agent)
        {
            _selectedAgent = agent;
        }

        public void SelectMap(MapData map)
        {
            _selectedMap = map;
        }

        public void SelectMode(TrainingMode mode)
        {
            _selectedMode = mode;
        }

        public void StartGame()
        {
            if (_selectedAgent == null)
            {
                Debug.LogWarning("[GameManager] Cannot start game: No agent selected");
                return;
            }

            SetState(GameState.Loading);
        }

        public void PauseGame()
        {
            if (_currentState != GameState.Playing) return;

            SetState(GameState.Paused);
            Time.timeScale = 0f;
            EventBus.TriggerSessionPause();
        }

        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;

            SetState(GameState.Playing);
            Time.timeScale = 1f;
            EventBus.TriggerSessionResume();
        }

        public void EndGame()
        {
            Time.timeScale = 1f;
            SetState(GameState.Results);
            EventBus.TriggerSessionEnd();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            _selectedAgent = null;
            _selectedMap = null;
            SetState(GameState.MainMenu);
        }
    }

    public enum TrainingMode
    {
        Flick,
        Tracking,
        Speed,
        Headshot,
        Elimination
    }
}
