using UnityEngine;
using Unity.Cinemachine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Infrastructure.Camera
{
    public class CinemachineService : MonoBehaviour, ICameraService
    {
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private CinemachineConfiner2D confiner;

        public void SetFollowTarget(Transform target)
        {
            if (virtualCamera == null)
            {
                Debug.LogError("[CinemachineService] virtualCamera is not assigned.");
                return;
            }

            virtualCamera.Target.TrackingTarget = target;
            virtualCamera.Target.CustomLookAtTarget = false;
        }

        public void SetConfinerBounds(Collider2D bounds)
        {
            if (confiner == null)
            {
                Debug.LogError("[CinemachineService] confiner is not assigned.");
                return;
            }

            confiner.BoundingShape2D = bounds;
            confiner.InvalidateBoundingShapeCache();
        }

        public void ClearConfinerBounds()
        {
            if (confiner == null) return;

            confiner.BoundingShape2D = null;
            confiner.InvalidateBoundingShapeCache();
        }

        public void SnapToTarget()
        {
            if (virtualCamera == null)
            {
                Debug.LogError("[CinemachineService] virtualCamera is not assigned.");
                return;
            }

            virtualCamera.PreviousStateIsValid = false;
        }
    }
}