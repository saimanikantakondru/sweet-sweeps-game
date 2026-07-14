using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.StateMachine.States
{
    public class PausedState : IState
    {
        public void Enter()
        {
            UnityEngine.Time.timeScale = 0f;
            Debug.Log("[PausedState] Enter — timeScale=0.");
        }

        public void Tick() { }

        public void Exit()
        {
            UnityEngine.Time.timeScale = 1f;
            Debug.Log("[PausedState] Exit — timeScale=1.");
        }
    }
}