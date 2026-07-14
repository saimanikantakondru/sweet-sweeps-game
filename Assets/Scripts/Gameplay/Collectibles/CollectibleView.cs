using UnityEngine;
using SweetSweeps.Data;

namespace SweetSweeps.Gameplay.Collectibles
{
    public class CollectibleView : MonoBehaviour
    {
        private MeshRenderer _meshRenderer;
        private Rigidbody2D _rigidbody2D;
        private MaterialPropertyBlock _propertyBlock;

        private float _lifetime;
        private float _fadeDuration;
        private float _timer;
        private bool  _isActive;
        private Color  _baseColor;

        public CollectibleDataSO Data { get; private set; }
        
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _rigidbody2D = GetComponentInChildren<Rigidbody2D>();
            
            _baseColor = _meshRenderer != null
                ? _meshRenderer.sharedMaterial.GetColor(ColorProperty)
                : Color.white;
        }

        public void Initialize(CollectibleDataSO data)
        {
            Data = data;
            _lifetime = data.Lifetime;
            _fadeDuration = data.FadeDuration;
            _timer = 0f;
            _rigidbody2D.simulated = false;
            SetAlpha(1f);
        }
        
        private void OnBecameVisible()
        {
            _isActive = true;
            _rigidbody2D.simulated = true;
        }

        private void Update()
        {
            if (!_isActive) return;
            
            _timer += Time.deltaTime;
            HandleLifetime();
        }

        private void HandleLifetime()
        {
            float remaining = _lifetime - _timer;
            if (remaining <= _fadeDuration)
            {
                float alpha = Mathf.Clamp01(remaining / _fadeDuration);
                SetAlpha(alpha);
            }

            if (_timer >= _lifetime)
            {
                ForceCollect();
            }
        }
        
        private void SetAlpha(float alpha)
        {
            if (_meshRenderer == null) return;
            _meshRenderer.GetPropertyBlock(_propertyBlock);
            var color = _baseColor;
            color.a = alpha;
            _propertyBlock.SetColor(ColorProperty, color);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void ForceCollect()
        {
            Destroy(gameObject);
        }
    }
}