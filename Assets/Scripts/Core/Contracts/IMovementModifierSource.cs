using SweetSweeps.Gameplay.Player;

namespace SweetSweeps.Core.Contracts
{
    public interface IMovementModifierSource
    {
        MovementModifier GetModifier();
    }
}