using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace ValorantAimTrainer.Editor
{
    public static class TargetAnimatorSetup
    {
        [MenuItem("Tools/Valorant Aim Trainer/1. Configure Animation FBX Files")]
        public static void ConfigureAnimationFBX()
        {
            string animPath = "Assets/_Project/Animations";
            string[] fbxFiles = { "idle.fbx", "strafe left.fbx", "strafe right.fbx" };

            foreach (string fbx in fbxFiles)
            {
                string path = $"{animPath}/{fbx}";
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

                if (importer == null)
                {
                    Debug.LogWarning($"[TargetAnimatorSetup] Could not find {path}");
                    continue;
                }

                // Set to Humanoid
                importer.animationType = ModelImporterAnimationType.Human;

                // Configure animation clips to loop
                ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                if (clips.Length > 0)
                {
                    ModelImporterClipAnimation[] newClips = new ModelImporterClipAnimation[clips.Length];
                    for (int i = 0; i < clips.Length; i++)
                    {
                        newClips[i] = clips[i];
                        newClips[i].loopTime = true;
                    }
                    importer.clipAnimations = newClips;
                }

                importer.SaveAndReimport();
                Debug.Log($"[TargetAnimatorSetup] Configured {fbx} as Humanoid with looping");
            }

            // Also configure ISO model
            string isoPath = "Assets/_Project/Models/ISO_RIG_V23.fbx";
            ModelImporter isoImporter = AssetImporter.GetAtPath(isoPath) as ModelImporter;
            if (isoImporter != null && isoImporter.animationType != ModelImporterAnimationType.Human)
            {
                isoImporter.animationType = ModelImporterAnimationType.Human;
                isoImporter.SaveAndReimport();
                Debug.Log("[TargetAnimatorSetup] Configured ISO model as Humanoid");
            }

            AssetDatabase.Refresh();
            Debug.Log("[TargetAnimatorSetup] All FBX files configured! Now run step 2.");
        }

        [MenuItem("Tools/Valorant Aim Trainer/2. Create Mixamo Target Prefab")]
        public static void CreateMixamoTargetPrefab()
        {
            string modelPath = "Assets/_Project/Models/mixamo.fbx";

            // Configure FBX as Humanoid
            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError("[TargetAnimatorSetup] Could not find mixamo.fbx at " + modelPath);
                return;
            }

            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.SaveAndReimport();
                Debug.Log("[TargetAnimatorSetup] Configured mixamo.fbx as Humanoid");
            }

            // Load the model
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelPrefab == null)
            {
                Debug.LogError("[TargetAnimatorSetup] Could not load model");
                return;
            }

            // Create instance
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            instance.name = "MixamoTarget";

            // Create a parent object to handle rotation offset
            GameObject root = new GameObject("MixamoTarget");
            instance.transform.SetParent(root.transform);
            instance.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face forward
            instance.name = "Model";

            instance = root; // Use root as the main prefab

            // Add Target component to root
            var target = instance.AddComponent<ValorantAimTrainer.Gameplay.Target>();

            // Get Animator from the model child (Mixamo FBX has it)
            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = instance.transform.GetChild(0).gameObject.AddComponent<Animator>();
            }

            // Create and assign controller
            CreateTargetAnimatorController();

            string controllerPath = "Assets/_Project/Animations/TargetAnimator.controller";
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            // Get avatar from mixamo model
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            foreach (Object asset in assets)
            {
                if (asset is Avatar avatar)
                {
                    animator.avatar = avatar;
                    Debug.Log($"[TargetAnimatorSetup] Assigned avatar: {avatar.name}, isHuman: {avatar.isHuman}");
                    break;
                }
            }

            // Add basic collider for body hit detection
            var collider = instance.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 0.9f, 0);
            collider.radius = 0.3f;
            collider.height = 1.8f;

            // Add HitZone to the collider
            var hitZone = instance.AddComponent<ValorantAimTrainer.Gameplay.HitZone>();

            // Save as prefab
            string prefabPath = "Assets/_Project/Prefabs/Agents/MixamoTarget.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            Debug.Log($"[TargetAnimatorSetup] Created prefab at {prefabPath}");
            Debug.Log("[TargetAnimatorSetup] Now update AgentData to use MixamoTarget prefab!");

            Selection.activeObject = prefab;
        }

        [MenuItem("Tools/Valorant Aim Trainer/3. Setup Animator Controller")]
        public static void CreateTargetAnimatorController()
        {
            string animPath = "Assets/_Project/Animations";

            // Create directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder(animPath))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Animations");
            }

            // Find animations
            AnimationClip idleClip = FindAnimationClip(animPath, "idle");
            AnimationClip strafeLeftClip = FindAnimationClip(animPath, "strafe left");
            AnimationClip strafeRightClip = FindAnimationClip(animPath, "strafe right");

            if (idleClip == null || strafeLeftClip == null || strafeRightClip == null)
            {
                Debug.LogError("[TargetAnimatorSetup] Could not find all animations. Make sure idle.fbx, strafe left.fbx, and strafe right.fbx are in Assets/_Project/Animations/");
                return;
            }

            string controllerPath = $"{animPath}/TargetAnimator.controller";

            // Delete existing controller
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            // Create the animator controller
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            // Add parameter
            controller.AddParameter("StrafeDirection", AnimatorControllerParameterType.Float);

            // Get the root state machine
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            // Create states with animations
            AnimatorState idleState = rootStateMachine.AddState("Idle", new Vector3(300, 0, 0));
            idleState.motion = idleClip;

            AnimatorState strafeLeftState = rootStateMachine.AddState("StrafeLeft", new Vector3(550, -80, 0));
            strafeLeftState.motion = strafeLeftClip;

            AnimatorState strafeRightState = rootStateMachine.AddState("StrafeRight", new Vector3(550, 80, 0));
            strafeRightState.motion = strafeRightClip;

            // Set Idle as default
            rootStateMachine.defaultState = idleState;

            // Idle -> StrafeLeft (when StrafeDirection < -0.5)
            AnimatorStateTransition toLeft = idleState.AddTransition(strafeLeftState);
            toLeft.AddCondition(AnimatorConditionMode.Less, -0.5f, "StrafeDirection");
            toLeft.hasExitTime = false;
            toLeft.duration = 0.15f;

            // Idle -> StrafeRight (when StrafeDirection > 0.5)
            AnimatorStateTransition toRight = idleState.AddTransition(strafeRightState);
            toRight.AddCondition(AnimatorConditionMode.Greater, 0.5f, "StrafeDirection");
            toRight.hasExitTime = false;
            toRight.duration = 0.15f;

            // StrafeLeft -> StrafeRight (when StrafeDirection > 0.5)
            AnimatorStateTransition leftToRight = strafeLeftState.AddTransition(strafeRightState);
            leftToRight.AddCondition(AnimatorConditionMode.Greater, 0.5f, "StrafeDirection");
            leftToRight.hasExitTime = false;
            leftToRight.duration = 0.15f;

            // StrafeRight -> StrafeLeft (when StrafeDirection < -0.5)
            AnimatorStateTransition rightToLeft = strafeRightState.AddTransition(strafeLeftState);
            rightToLeft.AddCondition(AnimatorConditionMode.Less, -0.5f, "StrafeDirection");
            rightToLeft.hasExitTime = false;
            rightToLeft.duration = 0.15f;

            // Save
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TargetAnimatorSetup] Animator Controller created with all animations assigned!");

            // Now setup the prefab
            SetupPrefab(controller);
        }

        private static AnimationClip FindAnimationClip(string folder, string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:AnimationClip", new[] { folder });

            if (guids.Length == 0)
            {
                // Try finding in FBX
                string fbxPath = $"{folder}/{name}.fbx";
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
                    {
                        return clip;
                    }
                }
            }
            else
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            }

            return null;
        }

        private static void SetupPrefab(AnimatorController controller)
        {
            string prefabPath = "Assets/_Project/Prefabs/Agents/ISO_Target_V2.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning("[TargetAnimatorSetup] Could not find prefab at " + prefabPath);
                return;
            }

            // Get the Avatar from ISO model
            string modelPath = "Assets/_Project/Models/ISO_RIG_V23.fbx";
            Avatar avatar = null;
            Object[] modelAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            foreach (Object asset in modelAssets)
            {
                if (asset is Avatar a)
                {
                    avatar = a;
                    break;
                }
            }

            // Open prefab for editing
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            // Add or get Animator component
            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null)
            {
                animator = prefabRoot.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;
            if (avatar != null)
            {
                animator.avatar = avatar;
            }
            animator.applyRootMotion = false;

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log("[TargetAnimatorSetup] Prefab ISO_Target_V2 configured with Animator!");
            Debug.Log("[TargetAnimatorSetup] Setup complete! Test the game.");
        }
    }
}
