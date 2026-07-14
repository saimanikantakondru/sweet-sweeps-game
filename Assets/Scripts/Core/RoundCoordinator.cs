using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Player;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Server.Services;

namespace SweetSweeps.Core
{
    public class RoundCoordinator
    {
        private const string RecordPrefsKey = "Record";
        private const int SurvivalMsPerStep = 1000;
        private const int SurvivalMsOffset = 2000;

        private readonly IStepService _stepService;
        private readonly StepService _stepServiceConcrete;
        private readonly ICollectionTracker _collectionTracker;
        private readonly IHealthService _healthService;
        private readonly IScoreService _scoreService;
        private readonly ITimeService _timeService;
        private readonly IPhaseService _phaseService;
        private readonly ICalamityService _calamityService;
        private readonly IPlatformService _platformService;
        private readonly IPurpleCoinValueService _valueService;
        private readonly IGameSessionService _sessionService;
        private readonly IGameApiService _apiService;
        private readonly IMessageBoxService _messageBox;
        private readonly ISceneService _sceneService;
        private readonly PlayerController _playerController;
        private readonly LevelCoordinator _levelCoordinator;
        private readonly NetworkCoordinator _networkCoordinator;

        private bool _roundCompleting;

        public RoundCoordinator(
            IStepService stepService,
            StepService stepServiceConcrete,
            ICollectionTracker collectionTracker,
            IHealthService healthService,
            IScoreService scoreService,
            ITimeService timeService,
            IPhaseService phaseService,
            ICalamityService calamityService,
            IPlatformService platformService,
            IPurpleCoinValueService valueService,
            IGameSessionService sessionService,
            IGameApiService apiService,
            IMessageBoxService messageBox,
            ISceneService sceneService,
            PlayerController playerController,
            LevelCoordinator levelCoordinator,
            NetworkCoordinator networkCoordinator)
        {
            _stepService = stepService;
            _stepServiceConcrete = stepServiceConcrete;
            _collectionTracker = collectionTracker;
            _healthService = healthService;
            _scoreService = scoreService;
            _timeService = timeService;
            _phaseService = phaseService;
            _calamityService = calamityService;
            _platformService = platformService;
            _valueService = valueService;
            _sessionService = sessionService;
            _apiService = apiService;
            _messageBox = messageBox;
            _sceneService = sceneService;
            _playerController = playerController;
            _levelCoordinator = levelCoordinator;
            _networkCoordinator = networkCoordinator;
        }

        public void PrepareRound(bool useSimulation, StepSimulationPresetSO simulationPreset)
        {
            _roundCompleting = false;
            _collectionTracker.Reset();

            float wager = _sessionService.CurrentRoundData != null
                ? _sessionService.CurrentRoundData.totalWager
                : 1f;
            _scoreService.SetWager(wager);

            var preset = _levelCoordinator.CurrentPreset;
            _stepServiceConcrete.SetPreset(preset);

            if (useSimulation)
                SimulationCoordinator.Run(_stepService, _stepServiceConcrete, preset, simulationPreset);
            else
                _networkCoordinator.Connect();
        }

        public void BeginPlay()
        {
            _playerController.SetActive(true);
        }

        public void StopRound()
        {
            _playerController.SetActive(false);
            _playerController.Freeze();
            _stepService.Stop();
            _calamityService.WindDown();
            _platformService.FreezeAll();
        }

        public async UniTaskVoid CompleteRoundAsync(bool useSimulation)
        {
            if (_roundCompleting) return;
            _roundCompleting = true;

            int survivalTimeMs = _stepService.CurrentStep * SurvivalMsPerStep + SurvivalMsOffset;
            bool survived = !_healthService.IsDead;

            _collectionTracker.TrackSurvival(survived, survivalTimeMs);
            var report = _collectionTracker.Build();

            PersistHighScore();

            if (useSimulation) return;

            if (!report.survived)
                _networkCoordinator.SendPlayerDeath(report);

            _networkCoordinator.Disconnect();

            try
            {
                var response = await _apiService.CompleteRound(
                    _sessionService.SessionUid,
                    _sessionService.Game,
                    _sessionService.GameRoundUid,
                    report);

                _sessionService.ApplyCompleteResponse(response);

                Debug.Log($"[RoundCoordinator] CompleteRound OK. " +
                          $"TotalWin={response.response?.totalWin} " +
                          $"Balance={_sessionService.CurrentWallet?.totalBalance}");
            }
            catch (ApiException e) when (e.IsNetworkError)
            {
                _sessionService.NotifyRoundResultFailed();
                await _messageBox.ShowAsync(
                    "Connection Error",
                    "Results could not be saved due to a network error.");
                await _sceneService.LoadMenuAsync();
            }
            catch (ApiException e) when (e.IsServerError)
            {
                _sessionService.NotifyRoundResultFailed();
                await _messageBox.ShowAsync(
                    "Server Error",
                    $"Server error ({e.HttpCode}). Results may not be saved.");
                await _sceneService.LoadMenuAsync();
            }
            catch (Exception e)
            {
                _sessionService.NotifyRoundResultFailed();
                Debug.LogError($"[RoundCoordinator] CompleteRound unexpected: {e.Message}");
            }
        }

        public void ResetRoundState()
        {
            _roundCompleting = false;
            _playerController.ResetPhysics();
            _sessionService.Clear();
            _collectionTracker.Reset();
            _scoreService.ResetScore();
            _timeService.ResetTimer();
            _stepService.Reset();
            _phaseService.Reset();
            _valueService.Reset();
        }

        private void PersistHighScore()
        {
            float value = _scoreService.CurrentValue;
            float record = PlayerPrefs.GetFloat(RecordPrefsKey, 0f);
            if (value <= record) return;

            PlayerPrefs.SetFloat(RecordPrefsKey, value);
            PlayerPrefs.Save();
            Debug.Log($"[RoundCoordinator] New record: {value}");
        }
    }
}
