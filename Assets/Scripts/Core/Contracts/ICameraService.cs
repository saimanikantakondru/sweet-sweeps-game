using UnityEngine;

namespace SweetSweeps.Core.Contracts
{
    public interface ICameraService
    {
        void SetFollowTarget(Transform target);
        void SetConfinerBounds(Collider2D bounds);
        void ClearConfinerBounds();
        void SnapToTarget();
    }
}