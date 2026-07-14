using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.StateMachine.States
{
    public class IdleState : IState
    {
        public void Enter()
        {
            Debug.Log("[IdleState] Enter — waiting for player to start.");
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[IdleState] Exit.");
        }
    }
}