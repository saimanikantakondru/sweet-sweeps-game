using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.StateMachine.States
{
    public class CountdownState : IState
    {
        public event Action OnEntered;
        public event Action OnExited;

        public void Enter()
        {
            Debug.Log("[CountdownState] Enter — pre-round countdown.");
            OnEntered?.Invoke();
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[CountdownState] Exit.");
            OnExited?.Invoke();
        }
    }
}
