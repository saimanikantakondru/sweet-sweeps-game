using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Level
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformMover : MonoBehaviour, IPlatformMover
    {
        [Header("Path (world space)")]
        [SerializeField] private Vector2 pointA = Vector2.left * 2f;
        [SerializeField] private Vector2 pointB = Vector2.right * 2f;

        [Header("Behaviour")]
        [SerializeField] private PlatformDataSO data;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool startAtPointA = true;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0.5f, 0.6f, 1f);

        [Header("Editor Preview")]
#if UNITY_EDITOR
        [PropertyRange(0f, 1f), OnValueChanged(nameof(OnPreviewChanged))]
#endif
        [SerializeField, Tooltip("Edit-mode only. Lerps platform between A (0) and B (1).")]
        private float previewT;

        private Rigidbody2D _rigidbody;
        private Vector2 _runtimePointA;
        private Vector2 _runtimePointB;
        private PlatformDataSO _runtimeData;
        private Coroutine _moveRoutine;
        private bool _paused;
        private bool _initialized;

        public bool IsMoving => !_paused && _initialized;
        public Rigidbody2D PlatformRigidbody => _rigidbody;
        public Vector2 PointA => pointA;
        public Vector2 PointB => pointB;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void Start()
        {
            if (_initialized || !autoStart) return;

            if (data == null)
            {
                Debug.LogWarning($"[PlatformMover] '{name}' autoStart enabled but PlatformDataSO is null.");
                return;
            }

            InitializeInternal(pointA, pointB, data);
        }

        public void Initialize(Vector2 a, Vector2 b, PlatformDataSO platformData)
        {
            pointA = a;
            pointB = b;
            data = platformData;
            InitializeInternal(a, b, platformData);
        }

        public void SetPaused(bool paused) => _paused = paused;

        public void Freeze()
        {
            _paused = true;

            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }

            if (_rigidbody != null)
                _rigidbody.linearVelocity = Vector2.zero;
        }

        public void SetPointA(Vector2 worldPosition) => pointA = worldPosition;
        public void SetPointB(Vector2 worldPosition) => pointB = worldPosition;

        private void InitializeInternal(Vector2 a, Vector2 b, PlatformDataSO platformData)
        {
            _runtimePointA = a;
            _runtimePointB = b;
            _runtimeData = platformData;
            _initialized = true;

            if (startAtPointA && _rigidbody != null)
                _rigidbody.position = a;

            _moveRoutine = StartCoroutine(MoveLoop());
        }

        private IEnumerator MoveLoop()
        {
            while (true)
            {
                yield return MoveTo(_runtimePointB);
                yield return new WaitForSeconds(_runtimeData.PauseDuration);
                yield return MoveTo(_runtimePointA);
                yield return new WaitForSeconds(_runtimeData.PauseDuration);
            }
        }

        private IEnumerator MoveTo(Vector2 target)
        {
            while (!_paused)
            {
                Vector2 current = _rigidbody.position;

                if (Vector2.Distance(current, target) < 0.01f)
                {
                    _rigidbody.MovePosition(target);
                    _rigidbody.linearVelocity = Vector2.zero;
                    break;
                }

                Vector2 direction = (target - current).normalized;
                _rigidbody.linearVelocity = direction * _runtimeData.MoveSpeed;
                yield return new WaitForFixedUpdate();
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(pointA, 0.2f);
            Gizmos.DrawWireSphere(pointB, 0.2f);
            Gizmos.DrawLine(pointA, pointB);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(pointA + Vector2.up * 0.4f, "A");
            UnityEditor.Handles.Label(pointB + Vector2.up * 0.4f, "B");
#endif
        }

#if UNITY_EDITOR
        private void OnPreviewChanged()
        {
            if (Application.isPlaying) return;
            transform.position = Vector2.Lerp(pointA, pointB, previewT);
        }

        [ContextMenu("Set Point A to current position")]
        private void SetPointAHere()
        {
            pointA = transform.position;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Point B to current position")]
        private void SetPointBHere()
        {
            pointB = transform.position;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button("Snap to A"), HorizontalGroup("Snap")]
        private void SnapToA()
        {
            previewT = 0f;
            transform.position = pointA;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button("Snap to B"), HorizontalGroup("Snap")]
        private void SnapToB()
        {
            previewT = 1f;
            transform.position = pointB;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button("Center Path On Platform")]
        private void CenterPathOnPlatform()
        {
            Vector2 half = (pointB - pointA) * 0.5f;
            pointA = (Vector2)transform.position - half;
            pointB = (Vector2)transform.position + half;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}