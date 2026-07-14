using System;
using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.StateMachine
{
    public class GameStateMachine : IGameStateMachine
    {
        private readonly Dictionary<Type, IState> _states = new();

        public IState CurrentState { get; private set; }

        public void RegisterState(IState state)
        {
            _states[state.GetType()] = state;
            Debug.Log($"[GameStateMachine] Registered state: {state.GetType().Name}");
        }

        public void ChangeState<T>() where T : IState
        {
            var type = typeof(T);

            if (!_states.TryGetValue(type, out var next))
            {
                Debug.LogError($"[GameStateMachine] State {type.Name} not registered.");
                return;
            }

            if (ReferenceEquals(CurrentState, next))
            {
                Debug.Log($"[GameStateMachine] Already in {type.Name} — transition ignored.");
                return;
            }

            CurrentState?.Exit();
            Debug.Log($"[GameStateMachine] {CurrentState?.GetType().Name} -> {type.Name}");
            CurrentState = next;
            CurrentState.Enter();
        }
    }
}