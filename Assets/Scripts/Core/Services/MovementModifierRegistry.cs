using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Gameplay.Player;

namespace SweetSweeps.Core.Services
{
    public class MovementModifierRegistry : IMovementModifierRegistry
    {
        private readonly List<IMovementModifierSource> _sources = new();

        public void Register(IMovementModifierSource source)
        {
            if (!_sources.Contains(source))
                _sources.Add(source);

            Debug.Log($"[MovementModifierRegistry] Registered: {source.GetType().Name}");
        }

        public void Unregister(IMovementModifierSource source)
        {
            _sources.Remove(source);
            Debug.Log($"[MovementModifierRegistry] Unregistered: {source.GetType().Name}");
        }

        public MovementModifier GetCombined()
        {
            var result = MovementModifier.Identity;

            foreach (var source in _sources)
                result = MovementModifier.Multiply(result, source.GetModifier());

            return result;
        }
    }
}