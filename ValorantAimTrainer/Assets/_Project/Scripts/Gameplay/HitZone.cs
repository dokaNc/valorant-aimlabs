using UnityEngine;

namespace ValorantAimTrainer.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class HitZone : MonoBehaviour
    {
        [SerializeField] private bool isHead;

        private Target _parentTarget;

        public bool IsHead => isHead;
        public Target ParentTarget => _parentTarget;

        private void Awake()
        {
            _parentTarget = GetComponentInParent<Target>();

            Collider col = GetComponent<Collider>();

            // Only set isTrigger if it's not a concave MeshCollider
            // (concave MeshColliders don't support triggers, but raycasts still work)
            if (col is MeshCollider meshCol && !meshCol.convex)
            {
                // Keep as regular collider - raycasts will still detect it
            }
            else
            {
                col.isTrigger = true;
            }

            gameObject.layer = isHead
                ? LayerMask.NameToLayer("Target_Head")
                : LayerMask.NameToLayer("Target_Body");
        }

        public void RegisterHit()
        {
            if (_parentTarget != null)
            {
                _parentTarget.OnHit(isHead);
            }
        }
    }
}
