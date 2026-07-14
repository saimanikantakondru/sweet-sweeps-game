namespace SweetSweeps.Core.Contracts
{
    public interface IPlayerGravityService
    {
        float CurrentGravity { get; }
        void SetFalling(bool isFalling);
        void SetGrounded(bool isGrounded);
        void Disable();
        void Enable();
    }
}