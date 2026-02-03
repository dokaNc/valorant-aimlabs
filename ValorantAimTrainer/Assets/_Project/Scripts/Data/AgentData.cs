using UnityEngine;

namespace ValorantAimTrainer.Data
{
    [CreateAssetMenu(fileName = "NewAgent", menuName = "ValorantAimTrainer/Agent Data")]
    public class AgentData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string agentName;
        [SerializeField] private string agentId;
        [SerializeField] private Sprite agentIcon;
        [SerializeField] [TextArea] private string description;

        [Header("Model")]
        [SerializeField] private GameObject modelPrefab;

        [Header("Hitbox - Head")]
        [SerializeField] private Vector3 headColliderCenter = new Vector3(0f, 1.65f, 0f);
        [SerializeField] private float headColliderRadius = 0.11f;

        [Header("Hitbox - Body")]
        [SerializeField] private Vector3 bodyColliderCenter = new Vector3(0f, 0.95f, 0f);
        [SerializeField] private float bodyColliderHeight = 1.1f;
        [SerializeField] private float bodyColliderRadius = 0.22f;

        public string AgentName => agentName;
        public string AgentId => agentId;
        public Sprite AgentIcon => agentIcon;
        public string Description => description;
        public GameObject ModelPrefab => modelPrefab;
        public Vector3 HeadColliderCenter => headColliderCenter;
        public float HeadColliderRadius => headColliderRadius;
        public Vector3 BodyColliderCenter => bodyColliderCenter;
        public float BodyColliderHeight => bodyColliderHeight;
        public float BodyColliderRadius => bodyColliderRadius;
    }
}
