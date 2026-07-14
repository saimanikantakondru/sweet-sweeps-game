using System;

namespace SweetSweeps.Menu.Contracts
{
    public interface IConsoleService
    {
        bool IsMuted              { get; }
        bool IsFullscreen         { get; }
        bool FullscreenSupported  { get; }
        bool HomeAvailable        { get; }

        event Action<bool> OnMuteChanged;
        event Action<bool> OnFullscreenChanged;

        void ToggleMute();
        void ToggleFullscreen();
        void GoHome();
    }
}
