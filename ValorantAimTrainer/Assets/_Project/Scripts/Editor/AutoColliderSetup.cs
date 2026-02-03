using UnityEngine;
using UnityEditor;

namespace ValorantAimTrainer.Editor
{
    /// <summary>
    /// Ajoute automatiquement des Mesh Colliders à tous les objets d'une map.
    /// </summary>
    public class AutoColliderSetup : EditorWindow
    {
        [MenuItem("Tools/Valorant Aim Trainer/Add Colliders to Selection")]
        public static void AddCollidersToSelection()
        {
            GameObject[] selected = Selection.gameObjects;

            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Aucune sélection",
                    "Sélectionne d'abord un ou plusieurs GameObjects dans la Hierarchy.",
                    "OK"
                );
                return;
            }

            int addedCount = 0;
            int skippedCount = 0;

            foreach (GameObject obj in selected)
            {
                // Traiter l'objet et tous ses enfants
                MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);

                foreach (MeshFilter mf in meshFilters)
                {
                    GameObject go = mf.gameObject;

                    // Skip si déjà un collider
                    if (go.GetComponent<Collider>() != null)
                    {
                        skippedCount++;
                        continue;
                    }

                    // Ajouter Mesh Collider
                    MeshCollider mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;

                    // Marquer comme modifié
                    EditorUtility.SetDirty(go);
                    addedCount++;
                }
            }

            // Sauvegarder
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Colliders ajoutés",
                $"✅ {addedCount} Mesh Colliders ajoutés\n⏭️ {skippedCount} objets ignorés (avaient déjà un collider)",
                "OK"
            );

            Debug.Log($"[AutoColliderSetup] {addedCount} colliders ajoutés, {skippedCount} ignorés");
        }

        [MenuItem("Tools/Valorant Aim Trainer/Remove All Mesh Colliders from Selection")]
        public static void RemoveCollidersFromSelection()
        {
            GameObject[] selected = Selection.gameObjects;

            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Aucune sélection",
                    "Sélectionne d'abord un ou plusieurs GameObjects dans la Hierarchy.",
                    "OK"
                );
                return;
            }

            int removedCount = 0;

            foreach (GameObject obj in selected)
            {
                MeshCollider[] colliders = obj.GetComponentsInChildren<MeshCollider>(true);

                foreach (MeshCollider mc in colliders)
                {
                    DestroyImmediate(mc);
                    removedCount++;
                }
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Colliders supprimés",
                $"🗑️ {removedCount} Mesh Colliders supprimés",
                "OK"
            );
        }
    }
}
