using UnityEngine;
using ValorantAimTrainer.Data;
using ValorantAimTrainer.Gameplay;

namespace ValorantAimTrainer.Utilities
{
    public class TargetPrefabBuilder : MonoBehaviour
    {
        [Header("Agent Data")]
        [SerializeField] private AgentData agentData;

        [Header("Model Reference")]
        [SerializeField] private GameObject modelObject;

        [ContextMenu("Build Target Prefab Structure")]
        public void BuildTargetStructure()
        {
            if (agentData == null)
            {
                Debug.LogError("AgentData is required to build target structure");
                return;
            }

            // Add Target component to root
            Target target = gameObject.GetComponent<Target>();
            if (target == null)
            {
                target = gameObject.AddComponent<Target>();
            }

            // Create Colliders parent
            Transform collidersParent = transform.Find("Colliders");
            if (collidersParent == null)
            {
                GameObject collidersGO = new GameObject("Colliders");
                collidersGO.transform.SetParent(transform);
                collidersGO.transform.localPosition = Vector3.zero;
                collidersParent = collidersGO.transform;
            }

            // Create Head Collider
            Transform headCollider = collidersParent.Find("HeadCollider");
            if (headCollider == null)
            {
                GameObject headGO = new GameObject("HeadCollider");
                headGO.transform.SetParent(collidersParent);
                headCollider = headGO.transform;
            }

            SphereCollider headSphere = headCollider.GetComponent<SphereCollider>();
            if (headSphere == null)
            {
                headSphere = headCollider.gameObject.AddComponent<SphereCollider>();
            }
            headSphere.center = agentData.HeadColliderCenter;
            headSphere.radius = agentData.HeadColliderRadius;
            headSphere.isTrigger = true;

            HitZone headHitZone = headCollider.GetComponent<HitZone>();
            if (headHitZone == null)
            {
                headHitZone = headCollider.gameObject.AddComponent<HitZone>();
            }
            // Note: isHead is private, will need to be set in inspector

            // Create Body Collider
            Transform bodyCollider = collidersParent.Find("BodyCollider");
            if (bodyCollider == null)
            {
                GameObject bodyGO = new GameObject("BodyCollider");
                bodyGO.transform.SetParent(collidersParent);
                bodyCollider = bodyGO.transform;
            }

            CapsuleCollider bodyCapsule = bodyCollider.GetComponent<CapsuleCollider>();
            if (bodyCapsule == null)
            {
                bodyCapsule = bodyCollider.gameObject.AddComponent<CapsuleCollider>();
            }
            bodyCapsule.center = agentData.BodyColliderCenter;
            bodyCapsule.height = agentData.BodyColliderHeight;
            bodyCapsule.radius = agentData.BodyColliderRadius;
            bodyCapsule.direction = 1; // Y-Axis
            bodyCapsule.isTrigger = true;

            HitZone bodyHitZone = bodyCollider.GetComponent<HitZone>();
            if (bodyHitZone == null)
            {
                bodyHitZone = bodyCollider.gameObject.AddComponent<HitZone>();
            }

            Debug.Log($"Target structure built for {agentData.AgentName}. Remember to:");
            Debug.Log("1. Set HeadCollider layer to 'Target_Head'");
            Debug.Log("2. Set BodyCollider layer to 'Target_Body'");
            Debug.Log("3. Set HeadCollider's HitZone.isHead to TRUE in inspector");
            Debug.Log("4. Set BodyCollider's HitZone.isHead to FALSE in inspector");
        }
    }
}
