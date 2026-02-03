using UnityEngine;

namespace ValorantAimTrainer.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private float weight = 1f;
        [SerializeField] private float spawnHeight = 0f;

        private BoxCollider _collider;

        public float Weight => weight;
        public Bounds WorldBounds
        {
            get
            {
                Vector3 center = transform.TransformPoint(_collider.center);
                Vector3 size = Vector3.Scale(_collider.size, transform.lossyScale);
                return new Bounds(center, size);
            }
        }

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _collider.isTrigger = true;
        }

        private void OnEnable()
        {
            TargetSpawner.Instance?.RegisterSpawnZone(this);
        }

        private void OnDisable()
        {
            TargetSpawner.Instance?.UnregisterSpawnZone(this);
        }

        public Vector3 GetRandomPointInZone()
        {
            Vector3 center = transform.TransformPoint(_collider.center);
            Vector3 size = Vector3.Scale(_collider.size, transform.lossyScale);

            // Random X and Z within bounds, fixed Y at spawnHeight
            return new Vector3(
                center.x + Random.Range(-size.x / 2f, size.x / 2f),
                spawnHeight,
                center.z + Random.Range(-size.z / 2f, size.z / 2f)
            );
        }

        private void OnDrawGizmos()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
