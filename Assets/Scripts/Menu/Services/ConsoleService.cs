using System;
using UnityEngine;
using VContainer.Unity;
using SweetSweeps.Infrastructure;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Menu.Contracts;

namespace SweetSweeps.Menu.Services
{
    public class ConsoleService : IConsoleService, ITickable
    {
        private const string MutePrefKey  = "SweetSweeps.Console.Muted";
        private const string HomeUrlParam = "homeUrl";

        private readonly IAudioService _audioService;
        private readonly string _homeUrl;

        private bool _lastFullscreen;

        public bool IsMuted             => _audioService.IsMuted;
        public bool IsFullscreen        => FullscreenBridge.IsFullscreen;
        public bool FullscreenSupported => Application.platform != RuntimePlatform.IPhonePlayer;
        public bool HomeAvailable       => !string.IsNullOrEmpty(_homeUrl);

        public event Action<bool> OnMuteChanged;
        public event Action<bool> OnFullscreenChanged;

        public ConsoleService(IAudioService audioService)
        {
            _audioService = audioService;
            _homeUrl = UrlParams.GetParam(HomeUrlParam);

            _lastFullscreen = FullscreenBridge.IsFullscreen;

            bool muted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
            _audioService.SetMuted(muted);

            Debug.Log($"[ConsoleService] Init. Muted={muted} HomeUrl={(HomeAvailable ? _homeUrl : "<none>")}");
        }

        public void Tick()
        {
            bool now = FullscreenBridge.IsFullscreen;
            if (now == _lastFullscreen) return;

            _lastFullscreen = now;
            OnFullscreenChanged?.Invoke(now);
        }

        public void ToggleMute()
        {
            _audioService.SetMuted(!_audioService.IsMuted);

            PlayerPrefs.SetInt(MutePrefKey, _audioService.IsMuted ? 1 : 0);
            PlayerPrefs.Save();

            OnMuteChanged?.Invoke(_audioService.IsMuted);
        }

        public void ToggleFullscreen()
        {
            if (!FullscreenSupported)
            {
                Debug.LogWarning("[ConsoleService] Fullscreen not supported on this platform.");
                return;
            }

            if (FullscreenBridge.IsFullscreen)
                FullscreenBridge.Exit();
            else
                FullscreenBridge.Request();
        }

        public void GoHome()
        {
            if (!HomeAvailable)
            {
                Debug.LogWarning("[ConsoleService] No homeUrl configured — skipping redirect.");
                return;
            }

            Debug.Log($"[ConsoleService] Redirecting to home: {_homeUrl}");
            Application.OpenURL(_homeUrl);
        }
    }
}
