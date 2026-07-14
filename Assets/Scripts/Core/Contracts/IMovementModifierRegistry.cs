namespace SweetSweeps.Core.Contracts
{
    public interface IMovementModifierRegistry
    {
        void Register(IMovementModifierSource source);
        void Unregister(IMovementModifierSource source);
        SweetSweeps.Gameplay.Player.MovementModifier GetCombined();
    }
}