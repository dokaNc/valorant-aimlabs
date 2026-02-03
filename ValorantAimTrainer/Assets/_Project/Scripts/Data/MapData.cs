using UnityEngine;

namespace ValorantAimTrainer.Data
{
    [CreateAssetMenu(fileName = "NewMap", menuName = "ValorantAimTrainer/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string mapName;
        [SerializeField] private string mapId;
        [SerializeField] private Sprite mapIcon;
        [SerializeField] [TextArea] private string description;

        [Header("Scene")]
        [SerializeField] private string sceneName;

        public string MapName => mapName;
        public string MapId => mapId;
        public Sprite MapIcon => mapIcon;
        public string Description => description;
        public string SceneName => sceneName;
    }
}
