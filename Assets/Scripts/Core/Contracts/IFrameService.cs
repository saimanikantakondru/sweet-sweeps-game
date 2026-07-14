using System;

namespace SweetSweeps.Core.Contracts
{
    public interface IFrameService
    {
        bool IsInvincible { get; }
        event Action OnInvincibilityStarted;
        event Action OnInvincibilityEnded;
        void TriggerInvincibility(bool isRespawn);
    }
}