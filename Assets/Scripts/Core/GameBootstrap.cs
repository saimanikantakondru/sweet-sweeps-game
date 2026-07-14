using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Core.StateMachine.States;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Collectibles;
using SweetSweeps.Gameplay.Player;
using SweetSweeps.Infrastructure.UI;
using SweetSweeps.Menu.Contracts;
using SweetSweeps.Server.Contracts;

namespace SweetSweeps.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Title("Scene References")]
        [SerializeField, Required] private UIPresenter uiPresenter;
        [SerializeField, Required] private Grid levelRootGrid;
        [SerializeField] private SpawnPoolDebugView spawnPoolDebugView;
        [SerializeField] private DebugNetworkPanelView debugNetworkPanel;

        [Title("Level")]
        [SerializeField, Tooltip("Fallback when launched directly from Editor without going through Menu.")]
        private string startWorldId = "World_01";

        [Title("Simulation (Editor only)")]
        [SerializeField] private StepSimulationPresetSO simulationPreset;

        private IGameStateMachine _stateMachine;
        private IHealthService _healthService;
        private IScoreService _scoreService;
        private ITimeService _timeService;
        private ICameraService _cameraService;
        private IFrameService _iFrameService;
        private IPhaseService _phaseService;
        private ICollectionTracker _collectionTracker;
        private IMessageBoxService _messageBox;
        private ISceneService _sceneService;
        private ILoadingScreenService _loadingScreenService;
        private IGameSessionService _sessionService;
        private GameSettingsSO _gameSettings;
        private PlayerStatsSO _playerStats;
        private PlayerController _playerController;

        private CountdownState _countdownState;
        private PlayingState _playingState;
        private GameOverState _gameOverState;
        private ResettingState _resettingState;

        private LevelCoordinator _levelCoordinator;
        private RoundCoordinator _roundCoordinator;
        private AudioCoordinator _audioCoordinator;
        private NetworkCoordinator _networkCoordinator;
        private ILevelHistoryService _levelHistory;
        private IWorldSelectService _worldSelectService;

        private string ActiveWorldId
        {
            get
            {
                string forced = SROptions.Current.ResolveForcedWorldId();
                if (!string.IsNullOrEmpty(forced)) return forced;

                return _worldSelectService?.CurrentWorld != null
                    ? _worldSelectService.CurrentWorld.WorldId
                    : startWorldId;
            }
        }

        [Inject]
        public void Construct(
            IGameStateMachine stateMachine,
            IHealthService healthService,
            IScoreService scoreService,
            ITimeService timeService,
            ICameraService cameraService,
            IFrameService iFrameService,
            IPhaseService phaseService,
            ICollectionTracker collectionTracker,
            IMessageBoxService messageBox,
            ISceneService sceneService,
            ILoadingScreenService loadingScreenService,
            IGameSessionService sessionService,
            GameSettingsSO gameSettings,
            PlayerStatsSO playerStats,
            PlayerController playerController,
            CountdownState countdownState,
            PlayingState playingState,
            GameOverState gameOverState,
            ResettingState resettingState,
            LevelCoordinator levelCoordinator,
            RoundCoordinator roundCoordinator,
            AudioCoordinator audioCoordinator,
            NetworkCoordinator networkCoordinator,
            ILevelHistoryService levelHistory,
            IWorldSelectService worldSelectService)
        {
            _stateMachine = stateMachine;
            _healthService = healthService;
            _scoreService = scoreService;
            _timeService = timeService;
            _cameraService = cameraService;
            _iFrameService = iFrameService;
            _phaseService = phaseService;
            _collectionTracker = collectionTracker;
            _messageBox = messageBox;
            _sceneService = sceneService;
            _loadingScreenService = loadingScreenService;
            _sessionService = sessionService;
            _gameSettings = gameSettings;
            _playerStats = playerStats;
            _playerController = playerController;
            _countdownState = countdownState;
            _playingState = playingState;
            _gameOverState = gameOverState;
            _resettingState = resettingState;
            _levelCoordinator = levelCoordinator;
            _roundCoordinator = roundCoordinator;
            _audioCoordinator = audioCoordinator;
            _networkCoordinator = networkCoordinator;
            _levelHistory = levelHistory;
            _worldSelectService = worldSelectService;

            Debug.Log($"[GameBootstrap] Active world: {ActiveWorldId} " +
                      $"(from {(_worldSelectService?.CurrentWorld != null ? "menu selection" : "fallback")}).");

            _levelCoordinator.Configure(levelRootGrid, spawnPoolDebugView, ActiveWorldId);

            var pitDetector = playerController.GetComponentInChildren<PitDetector>();
            pitDetector?.Initialize(healthService, gameSettings, stateMachine);
        }

        private void Start()
        {
            WireUI();
            SubscribeStates();
            WireDebugPanel();
            BootLevel();
            SROptions.Bind(_playerStats);
            SROptions.Bind(_healthService);
            //SRDebug.Instance.PinAllOptions("Cheats");
        }

        private void OnDestroy()
        {
            UnsubscribeStates();
            UnwireDebugPanel();

            _audioCoordinator?.OnRoundEnded();

            _networkCoordinator?.Disconnect();
        }

        public void ExitApplication()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _sceneService.LoadBootstrapAsync().Forget();
            //Application.OpenURL("about:blank");
#else
            Application.Quit();
#endif
        }

        private void WireUI()
        {
            _healthService.SetIFrameService(_iFrameService);
            _cameraService.SetFollowTarget(_playerController.transform);

            uiPresenter?.Initialize(
                _scoreService,
                _timeService,
                _healthService,
                _stateMachine,
                _sceneService,
                _collectionTracker,
                _sessionService,
                _gameSettings,
                _gameOverState,
                _countdownState);
        }

        private void SubscribeStates()
        {
            _playingState.OnEntered += OnRoundStarted;
            _playingState.OnExited += OnRoundEnded;
            _gameOverState.OnEntered += OnGameOver;
            _resettingState.OnResetComplete += OnLevelReset;
            _phaseService.OnBiomeCalamityStarted += _audioCoordinator.OnBiomeCalamityStarted;
            _phaseService.OnLevelChanged += _audioCoordinator.OnCalamityLevelChanged;
            _networkCoordinator.OnConnectionFailed += OnNetworkConnectionFailed;
        }

        private void UnsubscribeStates()
        {
            if (_playingState != null)
            {
                _playingState.OnEntered -= OnRoundStarted;
                _playingState.OnExited -= OnRoundEnded;
            }
            if (_gameOverState != null) _gameOverState.OnEntered -= OnGameOver;
            if (_resettingState != null) _resettingState.OnResetComplete -= OnLevelReset;
            if (_phaseService != null)
            {
                _phaseService.OnBiomeCalamityStarted -= _audioCoordinator.OnBiomeCalamityStarted;
                _phaseService.OnLevelChanged -= _audioCoordinator.OnCalamityLevelChanged;
            }
            if (_networkCoordinator != null)
                _networkCoordinator.OnConnectionFailed -= OnNetworkConnectionFailed;
        }

        private void WireDebugPanel()
        {
            if (debugNetworkPanel == null) return;

            debugNetworkPanel.gameObject.SetActive(true);
            debugNetworkPanel.OnSimulateConnectionLost += _networkCoordinator.DebugSimulateConnectionLost;
            debugNetworkPanel.OnSimulateSilentDrop += _networkCoordinator.DebugSimulateSilentDrop;
        }

        private void UnwireDebugPanel()
        {
            if (debugNetworkPanel == null) return;

            debugNetworkPanel.OnSimulateConnectionLost -= _networkCoordinator.DebugSimulateConnectionLost;
            debugNetworkPanel.OnSimulateSilentDrop -= _networkCoordinator.DebugSimulateSilentDrop;
        }

        private void BootLevel()
        {
            if (_sessionService.HasUnfinishedGame)
            {
                int savedIndex = _levelHistory.GetLastPlayedIndex(ActiveWorldId);
                if (savedIndex >= 0)
                {
                    Debug.Log($"[GameBootstrap] Resuming session on level index {savedIndex}.");
                    _levelCoordinator.LoadResumed(savedIndex, OnInitialLevelReady);
                    return;
                }

                Debug.LogWarning("[GameBootstrap] Resume requested but no saved preset — starting fresh map for the unfinished game.");
            }

            _levelCoordinator.LoadNewSession(OnInitialLevelReady);
        }

        private void OnInitialLevelReady()
        {
            _levelCoordinator.PlacePlayerAt(_levelCoordinator.PlayerSpawnPosition);
            _playerController.SetActive(false);
            _healthService.ResetHealth();
            _collectionTracker.Reset();

            _roundCoordinator.PrepareRound(_gameSettings.UseSimulation, simulationPreset);

            _stateMachine.ChangeState<CountdownState>();
            _loadingScreenService.Hide();
            _audioCoordinator.OnRoundStarted();
            Debug.Log("[GameBootstrap] Initial level ready — round prepared, streaming started.");
        }

        private void OnRoundStarted()
        {
            // _audioCoordinator.OnRoundStarted();
            _roundCoordinator.BeginPlay();
        }

        private void OnRoundEnded()
        {
            _audioCoordinator.OnRoundEnded();
            _roundCoordinator.StopRound();
        }

        private void OnGameOver()
        {
            _audioCoordinator.OnGameOver();
            _roundCoordinator.CompleteRoundAsync(_gameSettings.UseSimulation).Forget();
        }

        private void OnLevelReset()
        {
            _roundCoordinator.ResetRoundState();
            _healthService.ResetHealth();
            _levelCoordinator.Reload(OnReloadComplete);
        }

        private void OnReloadComplete()
        {
            _levelCoordinator.PlacePlayerAt(_levelCoordinator.PlayerSpawnPosition);
            _playerController.SetActive(false);

            _roundCoordinator.PrepareRound(_gameSettings.UseSimulation, simulationPreset);

            _stateMachine.ChangeState<CountdownState>();
            Debug.Log("[GameBootstrap] Reset complete — round prepared.");
        }

        private async void OnNetworkConnectionFailed(string reason)
        {
            Debug.LogError($"[GameBootstrap] Network connection failed: {reason}");

            _roundCoordinator.StopRound();
            SetGamePaused(true);

            await _messageBox.ShowAsync(
                "Connection Lost",
                "The connection was interrupted. The game will restart.");

            SetGamePaused(false);
            await _sceneService.LoadMenuAsync();
        }

        private static void SetGamePaused(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;
        }
    }
}
