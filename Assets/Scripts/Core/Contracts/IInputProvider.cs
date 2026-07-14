using UnityEngine;

namespace SweetSweeps.Core.Contracts
{
    public interface IInputProvider
    {
        Vector2 MovementInput { get; }
        bool JumpPressed { get; }
        bool DropPressed { get; }
        void Poll();
    }
}