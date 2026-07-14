using System.Collections;
using UnityEngine;

namespace SweetSweeps.Gameplay.Calamities
{
    [RequireComponent(typeof(CalamityObstacleView))]
    public class TileHazardOverlay : MonoBehaviour
    {
        [SerializeField] private float appearDuration = 0.3f;

        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _collider = GetComponentInChildren<Collider2D>();
        }

        public void Appear()
        {
            StartCoroutine(AppearRoutine());
        }

        public IEnumerator DisappearRoutine()
        {
            _collider.enabled = false;
            float timer = appearDuration;

            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                SetAlpha(Mathf.Clamp01(timer / appearDuration));
                yield return null;
            }
        }

        private IEnumerator AppearRoutine()
        {
            _collider.enabled = false;
            SetAlpha(0f);
            float timer = 0f;

            while (timer < appearDuration)
            {
                timer += Time.deltaTime;
                SetAlpha(Mathf.Clamp01(timer / appearDuration));
                yield return null;
            }

            SetAlpha(1f);
            _collider.enabled = true;
        }

        private void SetAlpha(float alpha)
        {
            var color = _spriteRenderer.color;
            color.a = alpha;
            _spriteRenderer.color = color;
        }
    }
}