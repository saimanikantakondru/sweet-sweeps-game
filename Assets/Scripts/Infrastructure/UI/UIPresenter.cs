using System;
using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Core.StateMachine.States;
using SweetSweeps.Gameplay.Input;
using SweetSweeps.Data;
using SweetSweeps.Server;
using SweetSweeps.Server.Contracts;

namespace SweetSweeps.Infrastructure.UI
{
    public class UIPresenter : MonoBehaviour
    {
        [SerializeField, Required] private HUDView hudView;
        [SerializeField, Required] private GameOverView gameOverView;
        [SerializeField, Required] private IdleView idleView;
        [SerializeField, Required] private CountdownView countdownView;
        [SerializeField, Required] private DamageVignetteView damageVignetteView;

        private IScoreService _scoreService;
        private ITimeService _timeService;
        private IHealthService _healthService;
        private IGameStateMachine _stateMachine;
        private ISceneService _sceneService;
        private ICollectionTracker _collectionTracker;
        private IGameSessionService _sessionService;
        private GameSettingsSO _gameSettings;
        private GameOverState _gameOverState;
        private CountdownState _countdownState;

        private CancellationTokenSource _countdownCts;
        private PlayerInputActions _input;

        private void Awake()
        {
            _input = new PlayerInputActions();
            _input.Enable();
        }

        public void Initialize(
            IScoreService scoreService,
            ITimeService timeService,
            IHealthService healthService,
            IGameStateMachine stateMachine,
            ISceneService sceneService,
            ICollectionTracker collectionTracker,
            IGameSessionService sessionService,
            GameSettingsSO gameSettings,
            GameOverState gameOverState,
            CountdownState countdownState)
        {
            _scoreService = scoreService;
            _timeService = timeService;
            _healthService = healthService;
            _stateMachine = stateMachine;
            _sceneService = sceneService;
            _collectionTracker = collectionTracker;
            _sessionService = sessionService;
            _gameSettings = gameSettings;
            _gameOverState = gameOverState;
            _countdownState = countdownState;

            SubscribeEvents();
            HideAll();
        }

        private void Update()
        {
            if (_input == null) return;
            if (!_input.Player.Jump.WasPressedThisFrame()) return;

            if (gameOverView.isActiveAndEnabled && gameOverView.ResultResolved)
                OnMainMenuPressed();
        }

        private void SubscribeEvents()
        {
            _scoreService.OnScoreChanged += OnScoreChanged;
            _scoreService.OnSsCoinsChanged += OnSsCoinsChanged;
            _timeService.OnTimeChanged += OnTimeChanged;
            _timeService.OnTimeExpired += OnTimeExpired;
            _healthService.OnHealthChanged += OnHealthChanged;
            _healthService.OnDamaged += OnDamaged;
            _gameOverState.OnEntered += OnGameOver;
            _sessionService.OnRoundResultReceived += OnRoundResultReceived;
            _sessionService.OnRoundResultFailed += OnRoundResultFailed;
            _countdownState.OnEntered += OnCountdownEntered;

            gameOverView.OnRestartPressed += OnPlayAgainPressed;
            gameOverView.OnMainMenuPressed += OnMainMenuPressed;
        }

        private void UnsubscribeEvents()
        {
            if (_scoreService != null)
            {
                _scoreService.OnScoreChanged -= OnScoreChanged;
                _scoreService.OnSsCoinsChanged -= OnSsCoinsChanged;
            }
            if (_timeService != null)
            {
                _timeService.OnTimeChanged -= OnTimeChanged;
                _timeService.OnTimeExpired -= OnTimeExpired;
            }
            if (_healthService != null)
            {
                _healthService.OnHealthChanged -= OnHealthChanged;
                _healthService.OnDamaged -= OnDamaged;
            }
            if (_gameOverState != null) _gameOverState.OnEntered -= OnGameOver;
            if (_sessionService != null)
            {
                _sessionService.OnRoundResultReceived -= OnRoundResultReceived;
                _sessionService.OnRoundResultFailed -= OnRoundResultFailed;
            }
            if (_countdownState != null)
                _countdownState.OnEntered -= OnCountdownEntered;

            gameOverView.OnRestartPressed -= OnPlayAgainPressed;
            gameOverView.OnMainMenuPressed -= OnMainMenuPressed;

            _countdownCts?.Cancel();
            _countdownCts?.Dispose();
            _countdownCts = null;
        }

        private void HideAll()
        {
            hudView.Hide();
            gameOverView.Hide();
            idleView.Hide();
            countdownView.Hide();
        }

        private void OnCountdownEntered()
        {
            _countdownCts?.Cancel();
            _countdownCts?.Dispose();
            _countdownCts = new CancellationTokenSource();
            RunCountdownAsync(_countdownCts.Token).Forget();
        }

        private async UniTaskVoid RunCountdownAsync(CancellationToken token)
        {
            ShowHUD();
            countdownView.Show();

            int from = Mathf.Max(1, _gameSettings.CountdownFrom);
            var stepDelay = TimeSpan.FromSeconds(Mathf.Max(0.05f, _gameSettings.CountdownStepSeconds));

            try
            {
                for (int n = from; n >= 1; n--)
                {
                    countdownView.SetText(n.ToString());
                    await UniTask.Delay(stepDelay, cancellationToken: token);
                }

                countdownView.SetText("GO");
                await UniTask.Delay(stepDelay, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            countdownView.Hide();
            _stateMachine.ChangeState<PlayingState>();
        }

        private void ShowHUD()
        {
            idleView.Hide();
            gameOverView.Hide();

            hudView.Show();
            hudView.SetTime(_timeService.RemainingTime);
            hudView.SetHealth(_healthService.CurrentHealth, _healthService.MaxHealth);
            hudView.SetGoldCoins(_scoreService.CurrentValue);
            hudView.SetSsCoins(_scoreService.SsCoins);
        }

        private void ShowGameOver()
        {
            hudView.Hide();
            idleView.Hide();

            var report = _collectionTracker?.Build();
            bool awaitResult = !_gameSettings.UseSimulation;
            gameOverView.Show(report, awaitResult);

            if (awaitResult && _sessionService.LastRoundResult != null)
                ApplyRoundResult(_sessionService.LastRoundResult);
        }

        private void OnRoundResultReceived(PlayCompleteData data)
        {
            if (!gameOverView.isActiveAndEnabled) return;
            ApplyRoundResult(data);
        }

        private void OnRoundResultFailed()
        {
            if (!gameOverView.isActiveAndEnabled) return;
            gameOverView.SetWinFailed();
        }

        private void ApplyRoundResult(PlayCompleteData data)
        {
            if (data == null)
            {
                gameOverView.SetWinFailed();
                return;
            }

            string currency = _sessionService.CurrentWallet?.currency;
            gameOverView.SetWinResult(data.totalWin, data.goldCoinScore, currency, data.jackpotAwarded);
        }

        private void OnPlayAgainPressed()
        {
            if (_gameSettings.UseSimulation)
                _stateMachine.ChangeState<ResettingState>();
            else
                _sceneService.LoadMenuAsync().Forget();
        }

        private void OnMainMenuPressed() => _sceneService.LoadMenuAsync().Forget();

        private void OnScoreChanged(float value) => hudView.SetGoldCoins(value);
        private void OnSsCoinsChanged(int amount) => hudView.SetSsCoins(amount);
        private void OnTimeChanged(float time) => hudView.SetTime(time);
        private void OnHealthChanged(int health) => hudView.SetHealth(health, _healthService.MaxHealth);

        private void OnDamaged(DamageType damageType) => damageVignetteView.Flash();

        private void OnTimeExpired() => _stateMachine.ChangeState<GameOverState>();

        private void OnGameOver() => ShowGameOver();

        private void OnDestroy()
        {
            UnsubscribeEvents();
            _input?.Disable();
            _input?.Dispose();
        }
    }
}
