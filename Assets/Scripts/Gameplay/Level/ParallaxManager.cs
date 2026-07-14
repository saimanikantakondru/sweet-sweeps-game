using UnityEngine;
using Unity.Cinemachine;

namespace SweetSweeps.Gameplay.Level
{
    public class ParallaxManager : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform target;

            [Tooltip("Override auto-calculated speed. 0 = use distance-based calculation.")]
            [Range(0f, 1f)]
            public float speedOverride = 0f;

            [HideInInspector] public Vector3 previousCameraPosition;
            [HideInInspector] public Vector3 anchorPosition;
            [HideInInspector] public float   resolvedSpeed;
        }

        [Header("Layers")]
        [SerializeField] private ParallaxLayer[] layers;

        [Header("Speed range (distance-based mapping)")]
        [SerializeField] [Range(0f, 1f)] private float minSpeed = 0.05f;
        [SerializeField] [Range(0f, 1f)] private float maxSpeed = 0.95f;

        [Header("Axes")]
        [SerializeField] private bool parallaxX = true;
        [SerializeField] private bool parallaxY = false;

        private Camera _camera;
        private bool _baselineCaptured;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Start()
        {
            ResolveSpeeds();
            CaptureAnchors();
        }

        private void CaptureAnchors()
        {
            if (layers == null) return;
            foreach (var layer in layers)
                if (layer.target != null) layer.anchorPosition = layer.target.position;
        }
        
        private void OnEnable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
        }

        private void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
        }

        private void OnCameraUpdated(CinemachineBrain brain)
        {
            if (brain.OutputCamera != _camera) return;

            if (!_baselineCaptured) return;

            Vector3 camPos = _camera.transform.position;

            foreach (var layer in layers)
            {
                if (layer.target == null) continue;

                Vector3 delta = camPos - layer.previousCameraPosition;

                float dx = parallaxX ? delta.x * layer.resolvedSpeed : 0f;
                float dy = parallaxY ? delta.y * layer.resolvedSpeed : 0f;

                layer.target.position      += new Vector3(dx, dy, 0f);
                layer.previousCameraPosition = camPos;
            }
        }

        public void RecalibrateBaseline(Vector2 travelFromSpawn)
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            Vector3 camPos = _camera.transform.position;
            foreach (var layer in layers)
            {
                if (layer.target == null) continue;

                float ox = parallaxX ? travelFromSpawn.x * layer.resolvedSpeed : 0f;
                float oy = parallaxY ? travelFromSpawn.y * layer.resolvedSpeed : 0f;

                layer.target.position        = layer.anchorPosition + new Vector3(ox, oy, 0f);
                layer.previousCameraPosition = camPos;
            }
            _baselineCaptured = true;
        }
        
        private void ResolveSpeeds()
        {
            if (layers == null || layers.Length == 0) return;

            float minDist = float.MaxValue;
            float maxDist = float.MinValue;

            foreach (var layer in layers)
            {
                if (layer.target == null) continue;
                float dist = Mathf.Abs(layer.target.position.z - _camera.transform.position.z);
                if (dist < minDist) minDist = dist;
                if (dist > maxDist) maxDist = dist;
            }

            float distRange = maxDist - minDist;

            foreach (var layer in layers)
            {
                if (layer.target == null) continue;

                if (layer.speedOverride > 0f)
                {
                    layer.resolvedSpeed = layer.speedOverride;
                }
                else
                {
                    float dist = Mathf.Abs(layer.target.position.z - _camera.transform.position.z);
                    float t    = distRange > 0f ? (dist - minDist) / distRange : 0f;
                    layer.resolvedSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (minSpeed > maxSpeed) minSpeed = maxSpeed;
        }
#endif
    }
}