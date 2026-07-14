using System;
using UnityEngine;

namespace SweetSweeps.Core.Contracts
{
    public interface IPlayerController
    {
        bool IsGrounded { get; }
        Vector2 Velocity { get; }

        event Action OnJumped;
        event Action OnDoubleJumped;
        event Action<bool> OnWalkStateChanged;

        void SetActive(bool active);
    }
}
