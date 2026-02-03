using UnityEngine;
using UnityEditor;
using ValorantAimTrainer.Gameplay;

namespace ValorantAimTrainer.Editor
{
    /// <summary>
    /// Editor utility to set up precise hitboxes on 3D models.
    /// Supports both humanoid rigs and generic models using MeshColliders.
    /// </summary>
    public class HitboxSetupEditor : EditorWindow
    {
        private GameObject targetModel;
        private bool includeHands = false;
        private bool includeFeet = false;
        private float colliderScale = 1f;
        private float headHeightPercent = 0.85f; // Head starts at 85% of total height
        private bool useMeshColliders = true;
        private bool convexMeshColliders = true;

        [MenuItem("Tools/Valorant Aim Trainer/Hitbox Setup")]
        public static void ShowWindow()
        {
            GetWindow<HitboxSetupEditor>("Hitbox Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Hitbox Setup Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            targetModel = (GameObject)EditorGUILayout.ObjectField("Target Model", targetModel, typeof(GameObject), true);

            GUILayout.Space(10);
            GUILayout.Label("Mode", EditorStyles.boldLabel);

            useMeshColliders = EditorGUILayout.Toggle("Use Mesh Colliders (Precise)", useMeshColliders);

            if (useMeshColliders)
            {
                convexMeshColliders = EditorGUILayout.Toggle("Convex (Required for Triggers)", convexMeshColliders);
                headHeightPercent = EditorGUILayout.Slider("Head Height %", headHeightPercent, 0.75f, 0.95f);

                EditorGUILayout.HelpBox(
                    "Mesh Colliders use the exact shape of each mesh for precise hit detection.\n" +
                    "Head detection: meshes above " + (headHeightPercent * 100f).ToString("F0") + "% height = headshot.",
                    MessageType.Info);
            }
            else
            {
                GUILayout.Label("Primitive Collider Options", EditorStyles.boldLabel);
                colliderScale = EditorGUILayout.Slider("Collider Scale", colliderScale, 0.5f, 2f);
                includeHands = EditorGUILayout.Toggle("Include Hands", includeHands);
                includeFeet = EditorGUILayout.Toggle("Include Feet", includeFeet);
            }

            GUILayout.Space(20);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Setup Hitboxes", GUILayout.Height(40)))
            {
                if (targetModel != null)
                {
                    if (useMeshColliders)
                    {
                        SetupMeshColliderHitboxes();
                    }
                    else
                    {
                        SetupHumanoidHitboxes();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a target model first.", "OK");
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
            if (GUILayout.Button("Remove All Hitboxes"))
            {
                if (targetModel != null)
                {
                    RemoveAllHitboxes();
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "How to use:\n\n" +
                "1. Drag the ISO_Target prefab (or model) into 'Target Model'\n" +
                "2. Keep 'Use Mesh Colliders' ON for precise hitboxes\n" +
                "3. Adjust 'Head Height %' to set where headshots start\n" +
                "4. Click 'Setup Hitboxes'\n" +
                "5. Apply changes to prefab if editing a prefab instance",
                MessageType.None);
        }

        private void SetupMeshColliderHitboxes()
        {
            Undo.RegisterCompleteObjectUndo(targetModel, "Setup Mesh Hitboxes");
            RemoveAllHitboxes();

            // Find all SkinnedMeshRenderers and MeshRenderers
            SkinnedMeshRenderer[] skinnedMeshes = targetModel.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            MeshFilter[] meshFilters = targetModel.GetComponentsInChildren<MeshFilter>(true);

            // Calculate bounds to determine head threshold
            Bounds totalBounds = CalculateTotalBounds();
            float headThresholdY = totalBounds.min.y + (totalBounds.size.y * headHeightPercent);

            int collidersCreated = 0;

            // Process SkinnedMeshRenderers
            foreach (var smr in skinnedMeshes)
            {
                if (smr.sharedMesh == null) continue;

                // Create a baked mesh for the collider
                Mesh bakedMesh = new Mesh();
                smr.BakeMesh(bakedMesh);

                // Determine if this is a head mesh based on position
                bool isHead = IsHeadMesh(smr.bounds, headThresholdY);

                CreateMeshHitbox(smr.gameObject, smr.sharedMesh, isHead);
                collidersCreated++;
            }

            // Process regular MeshFilters
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;

                MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                if (mr == null) continue;

                // Determine if this is a head mesh based on position
                bool isHead = IsHeadMesh(mr.bounds, headThresholdY);

                CreateMeshHitbox(mf.gameObject, mf.sharedMesh, isHead);
                collidersCreated++;
            }

            EditorUtility.SetDirty(targetModel);

            if (collidersCreated > 0)
            {
                EditorUtility.DisplayDialog("Success",
                    $"Created {collidersCreated} mesh collider hitboxes.\n\n" +
                    $"Head threshold: {headThresholdY:F2}m from ground.\n" +
                    "Meshes above this height will register as headshots.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Warning",
                    "No meshes found in the model. Make sure the model has MeshRenderers or SkinnedMeshRenderers.",
                    "OK");
            }
        }

        private Bounds CalculateTotalBounds()
        {
            Bounds bounds = new Bounds(targetModel.transform.position, Vector3.zero);
            bool boundsInitialized = false;

            foreach (var renderer in targetModel.GetComponentsInChildren<Renderer>(true))
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }

        private bool IsHeadMesh(Bounds meshBounds, float headThresholdY)
        {
            // Check if the center or majority of the mesh is above the head threshold
            return meshBounds.center.y >= headThresholdY ||
                   meshBounds.min.y >= headThresholdY - 0.05f;
        }

        private void CreateMeshHitbox(GameObject meshObject, Mesh mesh, bool isHead)
        {
            // Check if a HitZone already exists on this object
            HitZone existingHitZone = meshObject.GetComponent<HitZone>();
            if (existingHitZone != null)
            {
                Undo.DestroyObjectImmediate(existingHitZone);
            }

            // Check if a collider already exists
            Collider existingCollider = meshObject.GetComponent<Collider>();
            if (existingCollider != null)
            {
                Undo.DestroyObjectImmediate(existingCollider);
            }

            // Add MeshCollider
            MeshCollider meshCollider = Undo.AddComponent<MeshCollider>(meshObject);
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = convexMeshColliders;
            meshCollider.isTrigger = convexMeshColliders; // Can only be trigger if convex

            // Set layer
            meshObject.layer = isHead
                ? LayerMask.NameToLayer("Target_Head")
                : LayerMask.NameToLayer("Target_Body");

            // Add HitZone component
            HitZone hitZone = Undo.AddComponent<HitZone>(meshObject);

            // Use SerializedObject to set the private isHead field
            SerializedObject so = new SerializedObject(hitZone);
            so.FindProperty("isHead").boolValue = isHead;
            so.ApplyModifiedProperties();
        }

        private void SetupHumanoidHitboxes()
        {
            Animator animator = targetModel.GetComponent<Animator>();
            if (animator == null)
            {
                animator = targetModel.GetComponentInChildren<Animator>();
            }

            if (animator == null || !animator.isHuman)
            {
                EditorUtility.DisplayDialog("Info",
                    "No humanoid Animator found. Falling back to Mesh Colliders mode.", "OK");
                SetupMeshColliderHitboxes();
                return;
            }

            Undo.RegisterCompleteObjectUndo(targetModel, "Setup Hitboxes");

            // Remove existing hitbox colliders first
            RemoveAllHitboxes();

            int collidersCreated = 0;

            // HEAD - Sphere
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head != null)
            {
                CreateHitbox(head, HitboxType.Sphere, new Vector3(0, 0.08f, 0), 0.11f * colliderScale, true);
                collidersCreated++;
            }

            // NECK - Small Capsule
            Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            if (neck != null)
            {
                CreateHitbox(neck, HitboxType.Capsule, Vector3.zero, 0.05f * colliderScale, false, 0.12f);
                collidersCreated++;
            }

            // SPINE (Upper Torso) - Capsule
            Transform spine2 = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (spine2 != null)
            {
                CreateHitbox(spine2, HitboxType.Capsule, new Vector3(0, 0.1f, 0), 0.15f * colliderScale, false, 0.25f);
                collidersCreated++;
            }

            // SPINE (Lower Torso) - Capsule
            Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            if (spine != null)
            {
                CreateHitbox(spine, HitboxType.Capsule, new Vector3(0, 0.08f, 0), 0.14f * colliderScale, false, 0.2f);
                collidersCreated++;
            }

            // HIPS - Capsule
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hips != null)
            {
                CreateHitbox(hips, HitboxType.Capsule, Vector3.zero, 0.13f * colliderScale, false, 0.18f);
                collidersCreated++;
            }

            // LEFT ARM
            Transform leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

            if (leftUpperArm != null)
            {
                float armLength = leftLowerArm != null ? Vector3.Distance(leftUpperArm.position, leftLowerArm.position) : 0.25f;
                CreateHitbox(leftUpperArm, HitboxType.Capsule, new Vector3(armLength * 0.4f, 0, 0), 0.045f * colliderScale, false, armLength * 0.8f, 0);
                collidersCreated++;
            }

            if (leftLowerArm != null)
            {
                Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                float forearmLength = leftHand != null ? Vector3.Distance(leftLowerArm.position, leftHand.position) : 0.22f;
                CreateHitbox(leftLowerArm, HitboxType.Capsule, new Vector3(forearmLength * 0.4f, 0, 0), 0.04f * colliderScale, false, forearmLength * 0.8f, 0);
                collidersCreated++;
            }

            // RIGHT ARM
            Transform rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

            if (rightUpperArm != null)
            {
                float armLength = rightLowerArm != null ? Vector3.Distance(rightUpperArm.position, rightLowerArm.position) : 0.25f;
                CreateHitbox(rightUpperArm, HitboxType.Capsule, new Vector3(-armLength * 0.4f, 0, 0), 0.045f * colliderScale, false, armLength * 0.8f, 0);
                collidersCreated++;
            }

            if (rightLowerArm != null)
            {
                Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                float forearmLength = rightHand != null ? Vector3.Distance(rightLowerArm.position, rightHand.position) : 0.22f;
                CreateHitbox(rightLowerArm, HitboxType.Capsule, new Vector3(-forearmLength * 0.4f, 0, 0), 0.04f * colliderScale, false, forearmLength * 0.8f, 0);
                collidersCreated++;
            }

            // LEFT LEG
            Transform leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);

            if (leftUpperLeg != null)
            {
                float thighLength = leftLowerLeg != null ? Vector3.Distance(leftUpperLeg.position, leftLowerLeg.position) : 0.4f;
                CreateHitbox(leftUpperLeg, HitboxType.Capsule, new Vector3(0, -thighLength * 0.4f, 0), 0.07f * colliderScale, false, thighLength * 0.85f);
                collidersCreated++;
            }

            if (leftLowerLeg != null)
            {
                Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                float calfLength = leftFoot != null ? Vector3.Distance(leftLowerLeg.position, leftFoot.position) : 0.38f;
                CreateHitbox(leftLowerLeg, HitboxType.Capsule, new Vector3(0, -calfLength * 0.4f, 0), 0.055f * colliderScale, false, calfLength * 0.85f);
                collidersCreated++;
            }

            // RIGHT LEG
            Transform rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Transform rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

            if (rightUpperLeg != null)
            {
                float thighLength = rightLowerLeg != null ? Vector3.Distance(rightUpperLeg.position, rightLowerLeg.position) : 0.4f;
                CreateHitbox(rightUpperLeg, HitboxType.Capsule, new Vector3(0, -thighLength * 0.4f, 0), 0.07f * colliderScale, false, thighLength * 0.85f);
                collidersCreated++;
            }

            if (rightLowerLeg != null)
            {
                Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                float calfLength = rightFoot != null ? Vector3.Distance(rightLowerLeg.position, rightFoot.position) : 0.38f;
                CreateHitbox(rightLowerLeg, HitboxType.Capsule, new Vector3(0, -calfLength * 0.4f, 0), 0.055f * colliderScale, false, calfLength * 0.85f);
                collidersCreated++;
            }

            // Optional: Hands
            if (includeHands)
            {
                Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

                if (leftHand != null)
                {
                    CreateHitbox(leftHand, HitboxType.Sphere, new Vector3(0.05f, 0, 0), 0.04f * colliderScale, false);
                    collidersCreated++;
                }
                if (rightHand != null)
                {
                    CreateHitbox(rightHand, HitboxType.Sphere, new Vector3(-0.05f, 0, 0), 0.04f * colliderScale, false);
                    collidersCreated++;
                }
            }

            // Optional: Feet
            if (includeFeet)
            {
                Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

                if (leftFoot != null)
                {
                    CreateHitbox(leftFoot, HitboxType.Capsule, new Vector3(0, 0, 0.06f), 0.04f * colliderScale, false, 0.15f, 2);
                    collidersCreated++;
                }
                if (rightFoot != null)
                {
                    CreateHitbox(rightFoot, HitboxType.Capsule, new Vector3(0, 0, 0.06f), 0.04f * colliderScale, false, 0.15f, 2);
                    collidersCreated++;
                }
            }

            // SHOULDERS (additional coverage)
            Transform leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            Transform rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

            if (leftShoulder != null)
            {
                CreateHitbox(leftShoulder, HitboxType.Sphere, new Vector3(0.05f, 0, 0), 0.06f * colliderScale, false);
                collidersCreated++;
            }
            if (rightShoulder != null)
            {
                CreateHitbox(rightShoulder, HitboxType.Sphere, new Vector3(-0.05f, 0, 0), 0.06f * colliderScale, false);
                collidersCreated++;
            }

            EditorUtility.SetDirty(targetModel);
            EditorUtility.DisplayDialog("Success",
                $"Created {collidersCreated} hitbox colliders on the model.", "OK");
        }

        private enum HitboxType { Sphere, Capsule, Box }

        private void CreateHitbox(Transform bone, HitboxType type, Vector3 centerOffset, float radius, bool isHead, float height = 0, int direction = 1)
        {
            // Create a child GameObject for the hitbox
            GameObject hitboxObj = new GameObject($"Hitbox_{bone.name}");
            hitboxObj.transform.SetParent(bone);
            hitboxObj.transform.localPosition = Vector3.zero;
            hitboxObj.transform.localRotation = Quaternion.identity;
            hitboxObj.transform.localScale = Vector3.one;

            // Set layer
            hitboxObj.layer = isHead
                ? LayerMask.NameToLayer("Target_Head")
                : LayerMask.NameToLayer("Target_Body");

            // Add collider
            Collider col = null;
            switch (type)
            {
                case HitboxType.Sphere:
                    var sphere = hitboxObj.AddComponent<SphereCollider>();
                    sphere.center = centerOffset;
                    sphere.radius = radius;
                    sphere.isTrigger = true;
                    col = sphere;
                    break;

                case HitboxType.Capsule:
                    var capsule = hitboxObj.AddComponent<CapsuleCollider>();
                    capsule.center = centerOffset;
                    capsule.radius = radius;
                    capsule.height = height > 0 ? height : radius * 4f;
                    capsule.direction = direction; // 0=X, 1=Y, 2=Z
                    capsule.isTrigger = true;
                    col = capsule;
                    break;

                case HitboxType.Box:
                    var box = hitboxObj.AddComponent<BoxCollider>();
                    box.center = centerOffset;
                    box.size = new Vector3(radius * 2, height > 0 ? height : radius * 2, radius * 2);
                    box.isTrigger = true;
                    col = box;
                    break;
            }

            // Add HitZone component
            var hitZone = hitboxObj.AddComponent<HitZone>();

            // Use SerializedObject to set the private isHead field
            SerializedObject so = new SerializedObject(hitZone);
            so.FindProperty("isHead").boolValue = isHead;
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(hitboxObj, "Create Hitbox");
        }

        private void RemoveAllHitboxes()
        {
            // Remove HitZone components and their colliders, but NOT the mesh GameObjects
            HitZone[] existingHitboxes = targetModel.GetComponentsInChildren<HitZone>(true);

            foreach (var hitbox in existingHitboxes)
            {
                GameObject obj = hitbox.gameObject;

                // Check if this object has a mesh - if so, only remove components, not the object
                bool hasMesh = obj.GetComponent<MeshFilter>() != null ||
                               obj.GetComponent<SkinnedMeshRenderer>() != null;

                if (hasMesh)
                {
                    // Only remove the HitZone and Collider components
                    Collider col = obj.GetComponent<Collider>();
                    if (col != null)
                    {
                        Undo.DestroyObjectImmediate(col);
                    }
                    Undo.DestroyObjectImmediate(hitbox);
                }
                else
                {
                    // This is a separate hitbox object (from primitive mode), delete it entirely
                    Undo.DestroyObjectImmediate(obj);
                }
            }

            EditorUtility.SetDirty(targetModel);
        }
    }
}
