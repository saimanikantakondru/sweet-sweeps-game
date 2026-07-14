using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.StateMachine.States
{
    public class PlayingState : IState
    {
        private readonly ITimeService _timeService;

        public event Action OnEntered;
        public event Action OnExited;

        public PlayingState(ITimeService timeService)
        {
            _timeService = timeService;
        }

        public void Enter()
        {
            Debug.Log("[PlayingState] Enter — round started.");
            OnEntered?.Invoke();
        }

        public void Tick() { }

        public void Exit()
        {
            _timeService.StopTimer();
            Debug.Log("[PlayingState] Exit.");
            OnExited?.Invoke();
        }
    }
}