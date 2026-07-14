using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.StateMachine.States
{
    public class ResettingState : IState
    {
        public event Action OnResetComplete;

        public void Enter()
        {
            Debug.Log("[ResettingState] Enter — resetting level.");
            OnResetComplete?.Invoke();
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[ResettingState] Exit.");
        }
    }
}