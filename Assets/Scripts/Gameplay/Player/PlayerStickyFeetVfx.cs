using UnityEngine;
using Sirenix.OdinInspector;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Player
{
    public class PlayerStickyFeetVfx : MonoBehaviour
    {
        [Required]
        [SerializeField] private ParticleSystem gooParticles;

        [SerializeField] private string heavyCalamityId = "heavy";

        private IPhaseService _phaseService;
        private PlayerController _controller;
        private bool _isPlaying;

        public void Initialize(IPhaseService phaseService, PlayerController controller)
        {
            _phaseService = phaseService;
            _controller = controller;
        }

        private void Update()
        {
            if (_controller == null || gooParticles == null) return;

            bool shouldPlay = _controller.IsGrounded && IsHeavyActive();
            if (shouldPlay == _isPlaying) return;

            _isPlaying = shouldPlay;

            if (shouldPlay)
                gooParticles.Play();
            else
                gooParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private bool IsHeavyActive()
        {
            return _phaseService != null
                && _phaseService.BiomeCalamityId == heavyCalamityId
                && _phaseService.CurrentLevel != CalamityLevel.None;
        }
    }
}
