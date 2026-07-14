using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.StateMachine.States
{
    public class GameOverState : IState
    {
        public event Action OnEntered;

        public void Enter()
        {
            Debug.Log("[GameOverState] Enter.");
            OnEntered?.Invoke();
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[GameOverState] Exit.");
        }
    }
}