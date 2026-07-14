using System;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Core.StateMachine.States;

namespace SweetSweeps.Core
{
    public class CalamityAudioCoordinator : IStartable, IDisposable
    {
        private readonly IPhaseService _phase;
        private readonly ISfxService _sfx;
        private readonly BiomeAudioState _biomeState;
        private readonly SfxLibrarySO _library;
        private readonly PlayingState _playingState;

        private Surface _currentSurface = Surface.Grass;

        public CalamityAudioCoordinator(
            IPhaseService phase,
            ISfxService sfx,
            BiomeAudioState biomeState,
            SfxLibrarySO library,
            PlayingState playingState)
        {
            _phase = phase;
            _sfx = sfx;
            _biomeState = biomeState;
            _library = library;
            _playingState = playingState;
        }

        public void Start()
        {
            _phase.OnBiomeCalamityStarted += OnBiomeCalamityStarted;
            _phase.OnLevelChanged += OnLevelChanged;
            _phase.OnDecayTriggered += OnDecayTriggered;
            _playingState.OnExited += OnRoundEnded;
        }

        public void Dispose()
        {
            _phase.OnBiomeCalamityStarted -= OnBiomeCalamityStarted;
            _phase.OnLevelChanged -= OnLevelChanged;
            _phase.OnDecayTriggered -= OnDecayTriggered;
            _playingState.OnExited -= OnRoundEnded;
        }

        private void OnBiomeCalamityStarted(string calamityId)
        {
            _currentSurface = SurfaceFor(calamityId);
            _biomeState.SetBiome(calamityId, _currentSurface);
        }

        private void OnLevelChanged(CalamityLevel level)
        {
            _sfx.PlayOneShot(AmbientForLevel(level));
        }

        private SoundDefinition AmbientForLevel(CalamityLevel level) => level switch
        {
            CalamityLevel.LevelOne => _library.AmbientFor(_currentSurface),
            CalamityLevel.LevelTwo => _library.AmbientLevelTwoFor(_currentSurface),
            _                      => null
        };

        private void OnDecayTriggered()
        {
            _sfx.PlayLoop(_library.CalamityEnd);
        }

        private void OnRoundEnded()
        {
            _biomeState.Reset();
        }

        private static Surface SurfaceFor(string calamityId) => calamityId switch
        {
            "frost" => Surface.Snow,
            "heavy" => Surface.Lava,
            _       => Surface.Grass
        };
    }
}
