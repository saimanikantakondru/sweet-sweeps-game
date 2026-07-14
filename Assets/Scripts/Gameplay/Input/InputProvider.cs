using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Input
{
    public class InputProvider : IInputProvider, IDisposable
    {
        public Vector2 MovementInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool DropPressed { get; private set; }

        private readonly PlayerInputActions _actions;
        
        public InputProvider()
        {
            _actions = new PlayerInputActions();
            _actions.Enable();
        }
        
        public void Poll()
        {
            var move = _actions.Player.Move.ReadValue<Vector2>();
            MovementInput = move;
            JumpPressed = _actions.Player.Jump.WasPressedThisFrame();
            DropPressed = move.y < -0.5f;
        }

        public void Dispose()
        {
            _actions.Disable();
            _actions.Dispose();
        }
    }
}