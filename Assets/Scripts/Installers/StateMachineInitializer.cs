using VContainer.Unity;
using SweetSweeps.Core.StateMachine;
using SweetSweeps.Core.StateMachine.States;

namespace SweetSweeps.Core
{
    public class StateMachineInitializer : IInitializable
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IdleState _idleState;
        private readonly CountdownState _countdownState;
        private readonly PlayingState _playingState;
        private readonly PausedState _pausedState;
        private readonly GameOverState _gameOverState;
        private readonly ResettingState _resettingState;

        public StateMachineInitializer(
            GameStateMachine stateMachine,
            IdleState idleState,
            CountdownState countdownState,
            PlayingState playingState,
            PausedState pausedState,
            GameOverState gameOverState,
            ResettingState resettingState)
        {
            _stateMachine = stateMachine;
            _idleState = idleState;
            _countdownState = countdownState;
            _playingState = playingState;
            _pausedState = pausedState;
            _gameOverState = gameOverState;
            _resettingState = resettingState;
        }

        public void Initialize()
        {
            _stateMachine.RegisterState(_idleState);
            _stateMachine.RegisterState(_countdownState);
            _stateMachine.RegisterState(_playingState);
            _stateMachine.RegisterState(_pausedState);
            _stateMachine.RegisterState(_gameOverState);
            _stateMachine.RegisterState(_resettingState);
        }
    }
}