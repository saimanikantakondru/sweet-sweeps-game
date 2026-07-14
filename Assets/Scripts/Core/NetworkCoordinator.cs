using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Core
{
    public class NetworkCoordinator : IDisposable
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IStepService _stepService;
        private readonly StepService _stepServiceConcrete;
        private readonly IGameSessionService _sessionService;
        private readonly IPurpleCoinValueService _valueService;

        private bool _perRoundSubscribed;
        private bool _expectedDisconnect;
        private bool _disposed;

        public event Action<string> OnConnectionFailed;

        public bool IsConnected => _webSocketService.IsConnected;

        public NetworkCoordinator(
            IWebSocketService webSocketService,
            IStepService stepService,
            StepService stepServiceConcrete,
            IGameSessionService sessionService,
            IPurpleCoinValueService valueService)
        {
            _webSocketService = webSocketService;
            _stepService = stepService;
            _stepServiceConcrete = stepServiceConcrete;
            _sessionService = sessionService;
            _valueService = valueService;

            _webSocketService.OnError += OnWebSocketError;
            _webSocketService.OnDisconnected += OnWebSocketDisconnected;
            _webSocketService.OnConnectionFailed += OnWebSocketConnectionFailed;
            _stepService.OnStreamStalled += OnStepStreamStalled;
        }

        public bool Connect()
        {
            if (!_sessionService.IsAuthenticated || string.IsNullOrEmpty(_sessionService.GamePlayUid))
            {
                Debug.LogError("[NetworkCoordinator] Cannot connect — no GamePlayUid.");
                return false;
            }

            SubscribePerRound();
            _webSocketService.Connect(_sessionService.GamePlayUid);

            Debug.Log($"[NetworkCoordinator] Connecting. GamePlayUid={_sessionService.GamePlayUid}");
            return true;
        }

        public void Disconnect()
        {
            _expectedDisconnect = true;
            UnsubscribePerRound();
            if (_webSocketService.IsConnected)
                _webSocketService.Disconnect();
        }

        public void SendPlayerDeath(CollectionReport report)
        {
            _webSocketService.SendPlayerDeath(report);
        }

        public void DebugSimulateConnectionLost()
        {
            Debug.LogWarning("[NetworkCoordinator] DEBUG: simulating unexpected disconnect.");

            if (_webSocketService.IsConnected)
            {
                _webSocketService.SimulateUnexpectedDrop();
                return;
            }

            _expectedDisconnect = false;
            UnsubscribePerRound();
            OnConnectionFailed?.Invoke("Simulated connection loss (debug)");
        }

        public void DebugSimulateSilentDrop()
        {
            Debug.LogWarning("[NetworkCoordinator] DEBUG: simulating silent drop.");
            _expectedDisconnect = false;
            UnsubscribePerRound();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _expectedDisconnect = true;
            UnsubscribePerRound();

            _webSocketService.OnError -= OnWebSocketError;
            _webSocketService.OnDisconnected -= OnWebSocketDisconnected;
            _webSocketService.OnConnectionFailed -= OnWebSocketConnectionFailed;
            _stepService.OnStreamStalled -= OnStepStreamStalled;

            if (_webSocketService.IsConnected)
                _webSocketService.Disconnect();
        }

        private void SubscribePerRound()
        {
            if (_perRoundSubscribed) return;
            _perRoundSubscribed = true;

            _webSocketService.OnSessionReceived += OnWebSocketSessionReceived;
            _webSocketService.OnStepReceived += OnWebSocketStepReceived;
            _webSocketService.OnCalamityValuesReceived += OnWebSocketCalamityValuesReceived;
        }

        private void UnsubscribePerRound()
        {
            if (!_perRoundSubscribed) return;
            _perRoundSubscribed = false;

            _webSocketService.OnSessionReceived -= OnWebSocketSessionReceived;
            _webSocketService.OnStepReceived -= OnWebSocketStepReceived;
            _webSocketService.OnCalamityValuesReceived -= OnWebSocketCalamityValuesReceived;
        }

        private void OnWebSocketSessionReceived(SessionDataJson session)
        {
            Debug.Log($"[NetworkCoordinator] Session received. TotalSteps={session.totalSteps}");
            _stepServiceConcrete.StartSession(session);
            _valueService.SetNormalValues(session.purpleCoinValues?.normal);
        }

        private void OnWebSocketCalamityValuesReceived(CalamityValuesJson values)
        {
            _valueService.SetCalamityValues(values?.calamity);
        }

        private void OnWebSocketStepReceived(StepDataJson stepJson)
        {
            _stepService.ReceiveStep(stepJson.ToStepData());
        }

        private void OnWebSocketError(string error)
        {
            Debug.LogError($"[NetworkCoordinator] WebSocket error: {error}");
            if (_webSocketService.IsConnected)
                _webSocketService.Disconnect();
        }

        private void OnWebSocketDisconnected()
        {
            Debug.Log($"[NetworkCoordinator] WebSocket disconnected. Expected={_expectedDisconnect}");

            _expectedDisconnect = false;
            UnsubscribePerRound();
        }

        private void OnWebSocketConnectionFailed(string reason)
        {
            UnsubscribePerRound();
            OnConnectionFailed?.Invoke(reason);
        }

        private void OnStepStreamStalled()
        {
            Debug.LogWarning("[NetworkCoordinator] Step stream stalled — treating as connection failure.");

            _expectedDisconnect = true;
            UnsubscribePerRound();

            if (_webSocketService.IsConnected)
                _webSocketService.Disconnect();

            OnConnectionFailed?.Invoke("Step stream stalled.");
        }
    }
}
