#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ValorantAimTrainer.Editor
{
    public static class ProjectSetup
    {
        [MenuItem("ValorantAimTrainer/Setup Project")]
        public static void SetupProject()
        {
            SetupLayers();
            SetupTags();
            SetupPhysics();

            Debug.Log("[ProjectSetup] Project setup complete!");
            Debug.Log("Layers added: Target_Body (6), Target_Head (7), Environment (8)");
            Debug.Log("Tags added: Target, SpawnZone, PlayerSpawn, Manager");
        }

        [MenuItem("ValorantAimTrainer/Setup Layers Only")]
        public static void SetupLayers()
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
            );

            SerializedProperty layersProp = tagManager.FindProperty("layers");

            SetLayer(layersProp, 6, "Target_Body");
            SetLayer(layersProp, 7, "Target_Head");
            SetLayer(layersProp, 8, "Environment");

            tagManager.ApplyModifiedProperties();

            Debug.Log("[ProjectSetup] Layers configured!");
        }

        [MenuItem("ValorantAimTrainer/Setup Tags Only")]
        public static void SetupTags()
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
            );

            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            AddTag(tagsProp, "Target");
            AddTag(tagsProp, "SpawnZone");
            AddTag(tagsProp, "PlayerSpawn");
            AddTag(tagsProp, "Manager");

            tagManager.ApplyModifiedProperties();

            Debug.Log("[ProjectSetup] Tags configured!");
        }

        [MenuItem("ValorantAimTrainer/Setup Physics Only")]
        public static void SetupPhysics()
        {
            // Get layer indices
            int targetBodyLayer = 6;
            int targetHeadLayer = 7;

            // Disable collisions between targets
            Physics.IgnoreLayerCollision(targetBodyLayer, targetBodyLayer, true);
            Physics.IgnoreLayerCollision(targetHeadLayer, targetHeadLayer, true);
            Physics.IgnoreLayerCollision(targetBodyLayer, targetHeadLayer, true);

            Debug.Log("[ProjectSetup] Physics layer collision matrix configured!");
        }

        private static void SetLayer(SerializedProperty layersProp, int index, string layerName)
        {
            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(index);

            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = layerName;
                Debug.Log($"[ProjectSetup] Added layer '{layerName}' at index {index}");
            }
            else if (layerProp.stringValue != layerName)
            {
                Debug.LogWarning($"[ProjectSetup] Layer {index} already has value '{layerProp.stringValue}', replacing with '{layerName}'");
                layerProp.stringValue = layerName;
            }
        }

        private static void AddTag(SerializedProperty tagsProp, string tagName)
        {
            // Check if tag already exists
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
                {
                    return; // Tag already exists
                }
            }

            // Add new tag
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
            Debug.Log($"[ProjectSetup] Added tag '{tagName}'");
        }

        [MenuItem("ValorantAimTrainer/Create Test Scene")]
        public static void CreateTestScene()
        {
            // Create new scene
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single
            );

            // Create Managers GameObject
            GameObject managers = new GameObject("--- MANAGERS ---");

            GameObject gameManager = new GameObject("GameManager");
            gameManager.transform.SetParent(managers.transform);
            gameManager.AddComponent<Core.GameManager>();

            GameObject sessionManager = new GameObject("SessionManager");
            sessionManager.transform.SetParent(managers.transform);
            sessionManager.AddComponent<Core.SessionManager>();

            GameObject audioManager = new GameObject("AudioManager");
            audioManager.transform.SetParent(managers.transform);
            audioManager.AddComponent<Audio.AudioManager>();

            // Create Environment
            GameObject environment = new GameObject("--- ENVIRONMENT ---");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(environment.transform);
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
            ground.layer = LayerMask.NameToLayer("Environment");

            // Create Player
            GameObject playerParent = new GameObject("--- PLAYER ---");

            GameObject player = new GameObject("Player");
            player.transform.SetParent(playerParent.transform);
            player.transform.position = new Vector3(0f, 1.6f, -10f);
            player.AddComponent<Gameplay.PlayerController>();
            player.AddComponent<Gameplay.ShootingSystem>();

            // Move main camera to player
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.SetParent(player.transform);
                mainCam.transform.localPosition = Vector3.zero;
                mainCam.transform.localRotation = Quaternion.identity;
            }

            // Create Gameplay
            GameObject gameplay = new GameObject("--- GAMEPLAY ---");

            GameObject targetSpawner = new GameObject("TargetSpawner");
            targetSpawner.transform.SetParent(gameplay.transform);
            targetSpawner.AddComponent<Gameplay.TargetSpawner>();

            // Create UI
            GameObject uiParent = new GameObject("--- UI ---");

            // UI Manager
            GameObject uiManager = new GameObject("UIManager");
            uiManager.transform.SetParent(uiParent.transform);
            var uiMgr = uiManager.AddComponent<UI.UIManager>();

            GameObject crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(uiParent.transform);
            var crosshairRenderer = crosshair.AddComponent<UI.CrosshairRenderer>();

            GameObject hitFeedback = new GameObject("HitFeedback");
            hitFeedback.transform.SetParent(uiParent.transform);
            hitFeedback.AddComponent<Gameplay.HitFeedback>();

            GameObject hud = new GameObject("HUD");
            hud.transform.SetParent(uiParent.transform);
            var hudController = hud.AddComponent<UI.HUDController>();

            // Menu Screens
            GameObject mainMenu = new GameObject("MainMenuScreen");
            mainMenu.transform.SetParent(uiParent.transform);
            var mainMenuScreen = mainMenu.AddComponent<UI.MainMenuScreen>();

            GameObject pauseMenu = new GameObject("PauseScreen");
            pauseMenu.transform.SetParent(uiParent.transform);
            var pauseScreen = pauseMenu.AddComponent<UI.PauseScreen>();

            GameObject resultsMenu = new GameObject("ResultsScreen");
            resultsMenu.transform.SetParent(uiParent.transform);
            var resultsScreen = resultsMenu.AddComponent<UI.ResultsScreen>();

            // Link UI Manager references using SerializedObject
            var uiMgrSO = new UnityEditor.SerializedObject(uiMgr);
            uiMgrSO.FindProperty("mainMenuScreen").objectReferenceValue = mainMenuScreen;
            uiMgrSO.FindProperty("pauseScreen").objectReferenceValue = pauseScreen;
            uiMgrSO.FindProperty("resultsScreen").objectReferenceValue = resultsScreen;
            uiMgrSO.FindProperty("hudController").objectReferenceValue = hudController;
            uiMgrSO.FindProperty("crosshairRenderer").objectReferenceValue = crosshairRenderer;
            uiMgrSO.ApplyModifiedProperties();

            // Create Spawn Zone
            GameObject spawnZones = new GameObject("SpawnZones");
            spawnZones.transform.SetParent(gameplay.transform);

            GameObject spawnZone1 = new GameObject("SpawnZone_1");
            spawnZone1.transform.SetParent(spawnZones.transform);
            spawnZone1.transform.position = new Vector3(0f, 1f, 10f);
            BoxCollider zoneCollider = spawnZone1.AddComponent<BoxCollider>();
            zoneCollider.size = new Vector3(15f, 2f, 5f);
            zoneCollider.isTrigger = true;
            spawnZone1.AddComponent<Gameplay.SpawnZone>();

            // Create Bootstrap
            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.transform.SetParent(managers.transform);
            bootstrap.AddComponent<Utilities.GameBootstrap>();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/TestScene.unity";
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProjectSetup] Test scene created at {scenePath}");
            Debug.Log("Next steps:");
            Debug.Log("1. Create AgentData ScriptableObject for ISO");
            Debug.Log("2. Create Target prefab from ISO model");
            Debug.Log("3. Assign AgentData to GameBootstrap");
            Debug.Log("4. Create CrosshairSettings ScriptableObject");
        }
    }
}
#endif
