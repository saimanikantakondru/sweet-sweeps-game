using UnityEngine;
using SweetSweeps.Data;

namespace SweetSweeps.Core.Contracts
{
    public interface IPlatformMover
    {
        bool IsMoving { get; }

        void Initialize(Vector2 pointA, Vector2 pointB, PlatformDataSO data);
        void SetPaused(bool paused);
        void Freeze();
    }
}