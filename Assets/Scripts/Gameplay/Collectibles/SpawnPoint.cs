using UnityEngine;

namespace SweetSweeps.Gameplay.Collectibles
{
    public class SpawnPoint
    {
        public Vector2 Position { get; }
        public bool IsAvailable => !_used;

        private float _cooldownTimer;
        private bool _used;
        private bool _forceUnlocked;

        public SpawnPoint(Vector2 position)
        {
            Position = position;
        }

        public void MarkUsed()
        {
            _used = true;
        }

        public void ForceUnlock()
        {
            if (_forceUnlocked) return;
            
            _used = false;
            _forceUnlocked = true;
        }
        
        public void ResetForRound()
        {
            _used = false;
            _forceUnlocked = false;
        }
    }
}