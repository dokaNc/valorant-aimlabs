using UnityEngine;
using ValorantAimTrainer.Core;

namespace ValorantAimTrainer.Gameplay
{
    public class HitFeedback : MonoBehaviour
    {
        [Header("Hit Markers")]
        [SerializeField] private Color bodyHitColor = new Color(1f, 0.5f, 0f); // Orange
        [SerializeField] private Color headHitColor = new Color(1f, 1f, 0f);   // Yellow
        [SerializeField] private float hitMarkerSize = 20f;
        [SerializeField] private float hitMarkerDuration = 0.1f;
        [SerializeField] private float hitMarkerThickness = 2f;

        [Header("Particle References")]
        [SerializeField] private ParticleSystem bodyHitParticles;
        [SerializeField] private ParticleSystem headHitParticles;

        private bool _showHitMarker;
        private float _hitMarkerTimer;
        private bool _wasHeadshot;
        private Texture2D _pixelTexture;

        private void Awake()
        {
            CreatePixelTexture();
        }

        private void CreatePixelTexture()
        {
            _pixelTexture = new Texture2D(1, 1);
            _pixelTexture.SetPixel(0, 0, Color.white);
            _pixelTexture.Apply();
        }

        private void OnEnable()
        {
            EventBus.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            EventBus.OnHit -= HandleHit;
        }

        private void Update()
        {
            if (_showHitMarker)
            {
                _hitMarkerTimer -= Time.deltaTime;
                if (_hitMarkerTimer <= 0f)
                {
                    _showHitMarker = false;
                }
            }
        }

        private void HandleHit(Vector3 position, bool isHeadshot)
        {
            _showHitMarker = true;
            _hitMarkerTimer = hitMarkerDuration;
            _wasHeadshot = isHeadshot;

            // Spawn particles at hit position
            SpawnHitParticles(position, isHeadshot);
        }

        private void SpawnHitParticles(Vector3 position, bool isHeadshot)
        {
            ParticleSystem particles = isHeadshot ? headHitParticles : bodyHitParticles;

            if (particles != null)
            {
                particles.transform.position = position;
                particles.Play();
            }
        }

        private void OnGUI()
        {
            if (!_showHitMarker) return;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Color hitColor = _wasHeadshot ? headHitColor : bodyHitColor;

            DrawHitMarker(center, hitColor);
        }

        private void DrawHitMarker(Vector2 center, Color color)
        {
            GUI.color = color;

            float halfSize = hitMarkerSize / 2f;
            float gap = hitMarkerSize / 4f;

            // Top-left diagonal
            DrawRotatedLine(
                center.x - gap, center.y - gap,
                center.x - halfSize, center.y - halfSize,
                hitMarkerThickness
            );

            // Top-right diagonal
            DrawRotatedLine(
                center.x + gap, center.y - gap,
                center.x + halfSize, center.y - halfSize,
                hitMarkerThickness
            );

            // Bottom-left diagonal
            DrawRotatedLine(
                center.x - gap, center.y + gap,
                center.x - halfSize, center.y + halfSize,
                hitMarkerThickness
            );

            // Bottom-right diagonal
            DrawRotatedLine(
                center.x + gap, center.y + gap,
                center.x + halfSize, center.y + halfSize,
                hitMarkerThickness
            );
        }

        private void DrawRotatedLine(float x1, float y1, float x2, float y2, float thickness)
        {
            Vector2 start = new Vector2(x1, y1);
            Vector2 end = new Vector2(x2, y2);
            Vector2 direction = (end - start).normalized;
            float length = Vector2.Distance(start, end);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(x1, y1 - thickness / 2f, length, thickness), _pixelTexture);
            GUIUtility.RotateAroundPivot(-angle, start);
        }

        private void OnDestroy()
        {
            if (_pixelTexture != null)
            {
                Destroy(_pixelTexture);
            }
        }
    }
}
