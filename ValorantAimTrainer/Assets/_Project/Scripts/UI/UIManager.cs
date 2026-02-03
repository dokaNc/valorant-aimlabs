using UnityEngine;
using ValorantAimTrainer.Core;

namespace ValorantAimTrainer.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Screen References")]
        [SerializeField] private MainMenuScreen mainMenuScreen;
        [SerializeField] private PauseScreen pauseScreen;
        [SerializeField] private ResultsScreen resultsScreen;
        [SerializeField] private SettingsScreen settingsScreen;
        [SerializeField] private CrosshairSettingsPanel crosshairSettingsPanel;
        [SerializeField] private KeybindSettingsPanel keybindSettingsPanel;
        [SerializeField] private HUDController hudController;
        [SerializeField] private CrosshairRenderer crosshairRenderer;

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
            EventBus.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            // Start with main menu
            ShowMainMenu();
        }

        private void HandleGameStateChanged(GameState newState)
        {
            HideAllScreens();

            switch (newState)
            {
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;

                case GameState.Countdown:
                case GameState.Playing:
                    ShowGameplay();
                    break;

                case GameState.Paused:
                    ShowPause();
                    break;

                case GameState.Results:
                    ShowResults();
                    break;
            }
        }

        private void HideAllScreens()
        {
            if (mainMenuScreen != null) mainMenuScreen.Hide();
            if (pauseScreen != null) pauseScreen.Hide();
            if (resultsScreen != null) resultsScreen.Hide();
            if (settingsScreen != null) settingsScreen.Hide();
            if (crosshairSettingsPanel != null) crosshairSettingsPanel.Hide();
            if (keybindSettingsPanel != null) keybindSettingsPanel.Hide();
            if (hudController != null) hudController.HideHUD();
            if (crosshairRenderer != null) crosshairRenderer.Hide();
        }

        public void ShowSettings(bool fromPause = false)
        {
            HideAllScreens();
            if (settingsScreen != null) settingsScreen.Show(fromPause);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowPauseFromSettings()
        {
            HideAllScreens();
            if (pauseScreen != null) pauseScreen.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowMainMenuFromSettings()
        {
            HideAllScreens();
            ShowMainMenu();
        }

        public void ShowCrosshairSettings(bool fromPause = false)
        {
            HideAllScreens();
            if (crosshairSettingsPanel != null)
            {
                crosshairSettingsPanel.Show();
            }
            if (crosshairRenderer != null) crosshairRenderer.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowSettingsFromCrosshair()
        {
            HideAllScreens();
            if (settingsScreen != null) settingsScreen.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowKeybindSettings(bool fromPause = false)
        {
            HideAllScreens();
            if (keybindSettingsPanel != null)
            {
                keybindSettingsPanel.Show(fromPause);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowSettingsFromKeybinds(bool fromPause = false)
        {
            HideAllScreens();
            if (settingsScreen != null) settingsScreen.Show(fromPause);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ShowMainMenu()
        {
            if (mainMenuScreen != null) mainMenuScreen.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ShowGameplay()
        {
            if (hudController != null) hudController.ShowHUD();
            if (crosshairRenderer != null) crosshairRenderer.Show();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ShowPause()
        {
            if (pauseScreen != null) pauseScreen.Show();
            if (crosshairRenderer != null) crosshairRenderer.Hide();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ShowResults()
        {
            if (resultsScreen != null) resultsScreen.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
