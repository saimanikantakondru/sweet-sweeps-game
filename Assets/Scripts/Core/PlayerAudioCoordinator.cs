using System;
using UnityEngine;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Core.StateMachine.States;

namespace SweetSweeps.Core
{
    public class PlayerAudioCoordinator : IStartable, ITickable, IDisposable
    {

        private readonly IPlayerController _player;
        private readonly IHealthService _health;
        private readonly ICollectibleService _collectibles;
        private readonly ISfxService _sfx;
        private readonly IBiomeAudioState _biome;
        private readonly SfxLibrarySO _library;
        private readonly AudioSettingsSO _audioSettings;
        private readonly PlayingState _playingState;

        private bool _isWalking;
        private float _stepTimer;

        public PlayerAudioCoordinator(
            IPlayerController player,
            IHealthService health,
            ICollectibleService collectibles,
            ISfxService sfx,
            IBiomeAudioState biome,
            SfxLibrarySO library,
            AudioSettingsSO audioSettings,
            PlayingState playingState)
        {
            _player = player;
            _health = health;
            _collectibles = collectibles;
            _sfx = sfx;
            _biome = biome;
            _library = library;
            _audioSettings = audioSettings;
            _playingState = playingState;
        }

        public void Start()
        {
            _player.OnJumped += OnJumped;
            _player.OnDoubleJumped += OnDoubleJumped;
            _player.OnWalkStateChanged += OnWalkStateChanged;
            _collectibles.OnCollected += OnCollected;
            _health.OnDamaged += OnDamaged;
            _health.OnDeath += OnDeath;
            _playingState.OnExited += OnRoundEnded;
        }

        public void Dispose()
        {
            _player.OnJumped -= OnJumped;
            _player.OnDoubleJumped -= OnDoubleJumped;
            _player.OnWalkStateChanged -= OnWalkStateChanged;
            _collectibles.OnCollected -= OnCollected;
            _health.OnDamaged -= OnDamaged;
            _health.OnDeath -= OnDeath;
            _playingState.OnExited -= OnRoundEnded;
        }

        private void OnJumped() => _sfx.PlayOneShot(_library.Jump);
        private void OnDoubleJumped() => _sfx.PlayOneShot(_library.JumpDouble);

        private void OnWalkStateChanged(bool isWalking)
        {
            _isWalking = isWalking;
            _stepTimer = 0f;
        }

        public void Tick()
        {
            if (!_isWalking || !_player.IsGrounded) return;

            _stepTimer -= Time.deltaTime;
            if (_stepTimer > 0f) return;

            _stepTimer = _audioSettings.WalkStepInterval;
            _sfx.PlayOneShot(_library.WalkFor(_biome.CurrentSurface));
        }

        private void OnCollected(CollectibleDataSO data)
        {
            _sfx.PlayOneShot(_library.CollectFor(data.Type));
        }

        private void OnDamaged(DamageType damageType)
        {
            if (damageType != DamageType.BiomeCalamity) return;
            _sfx.PlayOneShot(_library.DamageFor(_biome.CurrentSurface));
        }

        private void OnDeath(DamageType damageType)
        {
            _isWalking = false;
            _sfx.PlayOneShot(ResolveDeath(damageType));
        }

        private void OnRoundEnded()
        {
            _isWalking = false;
            _sfx.StopAll();
        }

        private SoundDefinition ResolveDeath(DamageType damageType)
        {
            if (damageType == DamageType.Fall) return _library.DeathFall;

            return _biome.CurrentSurface switch
            {
                Surface.Snow => _library.DeathFreeze,
                Surface.Lava => _library.DeathBurn,
                _            => _library.DeathFall
            };
        }
    }
}
